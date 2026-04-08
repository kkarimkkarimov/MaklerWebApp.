using MaklerWebApp.API.Middleware;
using MaklerWebApp.API.Services;
using MaklerWebApp.BLL.Extensions;
using MaklerWebApp.BLL.Models;
using MaklerWebApp.DAL.Data;
using MaklerWebApp.DAL.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace MaklerWebApp.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddScoped<IImageStorageService, LocalImageStorageService>();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage).ToArray());

                return new BadRequestObjectResult(new
                {
                    message = "Validation failed.",
                    errors,
                    traceId = context.HttpContext.TraceIdentifier
                });
            };
        });

        builder.Services.AddDataAccess(builder.Configuration);
        builder.Services.AddBusinessLayer();

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddHealthChecks().AddDbContextCheck<MaklerDbContext>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MaklerWebApp API",
                Version = "v1",
                Description = "Professional API for authentication, listings, favorites, payments, and profile operations."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token as: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseMiddleware<ApiExceptionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", () => Results.Redirect("/swagger"));
        app.MapHealthChecks("/health");
        app.MapControllers();

        app.Run();
    }
}
