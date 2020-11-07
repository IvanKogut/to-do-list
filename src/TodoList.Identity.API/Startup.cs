using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using TodoList.Identity.API.Data;
using TodoList.Identity.API.Data.Entities;

namespace TodoList.Identity.API
{
  public class Startup
  {
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration)
    {
      this.configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services
        .AddControllersWithViews();

      string connectionString = configuration.GetConnectionString("DefaultConnection");

      services
        .AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

      services
        .AddIdentity<User, Role>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

      string? migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

      services
        .AddIdentityServer()
        .AddConfigurationStore(options =>
        {
          options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
        })
        .AddOperationalStore(options =>
        {
          options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
        })
        .AddAspNetIdentity<User>()
        .AddDeveloperSigningCredential();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseHttpsRedirection();

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseMiniProfiler();
      }

      app.UseStaticFiles();

      app.UseRouting();
      app.UseIdentityServer();
      app.UseAuthorization();
      app.UseEndpoints(endpoints =>
      {
        endpoints.MapDefaultControllerRoute();
      });
    }
  }
}
