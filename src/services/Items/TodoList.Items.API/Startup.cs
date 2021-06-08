using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Profiling;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TodoList.Items.API.BackgroundServices;
using TodoList.Items.API.Filters;
using TodoList.Items.API.Options;
using TodoList.Items.API.Swagger;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Domain.Shared;
using TodoList.Items.Infrastructure;
using TodoList.Items.Infrastructure.Repositories;

namespace TodoList.Items.API
{
  public class Startup
  {
    protected virtual string ConnectionStringName { get; } = "DefaultConnection";

    private readonly IConfiguration configuration;
    private readonly IWebHostEnvironment webHostEnvironment;

    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
      this.configuration = configuration;
      this.webHostEnvironment = webHostEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services
        .AddDbContext<ItemsDbContext>(o => o.UseSqlServer(configuration.GetConnectionString(ConnectionStringName)));

      if (webHostEnvironment.IsDevelopment())
      {
        services
          .AddMiniProfiler(o => o.RouteBasePath = "/profiler")
          .AddEntityFramework();
      }

      services
        .Configure<ApiBehaviorOptions>(o => o.SuppressModelStateInvalidFilter = true);

      services
        .AddAuthentication(o =>
        {
          o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
          o.Authority = configuration["IdentityUrl"];
          o.Audience = "items";
          o.RequireHttpsMetadata = false;
        });

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
          Version = "v1",
          Title = "Todo List",
          Description = "Simple Todo List API developed using C#, ASP.NET Core 5.0 and EF Core 5.0"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
          Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
          Name = "Authorization",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.ApiKey
        });

        c.OperationFilter<AuthResponsesOperationFilter>();

        c.DocumentFilter<LowercaseDocumentFilter>();

        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
      });

      services.AddCors();

      services.AddMemoryCache();

      services
        .AddControllers(o => o.Filters.Add(typeof(ModelStateInvalidFilter)))
        .AddApplicationPart(typeof(Startup).Assembly);

      services.AddMediatR(Assembly.GetExecutingAssembly());

      services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ItemsDbContext>());

      services.AddScoped<IItemRepository, ItemRepository>();
      services.AddScoped<IUserRepository, UserRepository>();

      services.AddHostedService<EventBusHostedService>();

      services.Configure<EventBusOptions>(configuration.GetSection("EventBus"));
    }

    public void Configure(IApplicationBuilder app)
    {
      app.UseHttpsRedirection();

      if (webHostEnvironment.IsDevelopment())
      {
        app.UseMiniProfiler();
      }

      app.UseSwagger(c => c.RouteTemplate = "api-docs/{documentName}/swagger.json");

      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/api-docs/v1/swagger.json", "Todo List v1");
        c.IndexStream = () => File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Swagger/index.html"));
      });

      app.UseRouting();
      app.UseCors(b => b.WithOrigins(configuration["Cors:Origins"]?.Split(",").Select(o => o.Trim()).ToArray() ?? Array.Empty<string>()).AllowAnyHeader().AllowAnyMethod());

      ConfigureAuth(app);

      app.UseEndpoints(endpoints => endpoints.MapControllers());
    }

    protected virtual void ConfigureAuth(IApplicationBuilder app)
    {
      app.UseAuthentication();
      app.UseAuthorization();
    }
  }
}
