﻿using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Controllers.Tests.Extensions
{
  public static class ServerServiceExtensions
  {
    public static T GetService<T>(this TestServer server)
    {
      return server.Host.Services.CreateScope().ServiceProvider.GetRequiredService<T>();
    }
  }
}
