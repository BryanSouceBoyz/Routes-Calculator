using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RoutesCalculator.Application.Contracts;
using RoutesCalculator.Application.Services;
using RoutesCalculator.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "_allowLocal";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy
                .SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            var allowed = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            if (allowed.Length > 0)
            {
                policy.WithOrigins(allowed)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(er => er.ErrorMessage))
            .ToList();

        return new BadRequestObjectResult(new { errors });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Calculadora de Rutas y Costos",
        Version = "v1",
        Description = "Calcula costos de viaje y sugiere la opción más económica."
    });
});

builder.Services.AddScoped<ICostCalculator, PublicCarCalculator>();
builder.Services.AddScoped<ICostCalculator, UberCalculator>();
builder.Services.AddScoped<ICostCalculator, OwnCarCalculator>();
builder.Services.AddScoped<ICostService, CostService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthorization();

app.MapControllers();

app.Run();
