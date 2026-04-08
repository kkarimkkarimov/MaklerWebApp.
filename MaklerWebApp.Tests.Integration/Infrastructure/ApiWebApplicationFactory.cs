using MaklerWebApp.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MaklerWebApp.Tests.Integration.Infrastructure;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public ApiWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "MaklerWebApp.Tests.SuperSecretKey.1234567890");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var inMemoryConfig = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=MaklerWebAppTests;Trusted_Connection=True;Encrypt=False;",
                ["Jwt:Issuer"] = "MaklerWebApp.Tests",
                ["Jwt:Audience"] = "MaklerWebApp.Tests.Client",
                ["Jwt:SecretKey"] = "MaklerWebApp.Tests.SuperSecretKey.1234567890",
                ["Jwt:AccessTokenMinutes"] = "30",
                ["Jwt:RefreshTokenDays"] = "30"
            };

            configBuilder.AddInMemoryCollection(inMemoryConfig);
        });
    }
}
