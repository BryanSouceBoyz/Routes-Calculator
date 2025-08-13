using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RoutesCalculator.Application.Contracts;
using RoutesCalculator.Application.Services;
using RoutesCalculator.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// --- CORS ---
const string CorsPolicy = "_allowLocal";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Opción 1 (cómoda): permitir todo en desarrollo
            policy
                .SetIsOriginAllowed(_ => true) // cualquier origen
                .AllowAnyHeader()
                .AllowAnyMethod();

            // Opción 2 (estricta): comenta lo de arriba y deja orígenes concretos
            //policy.WithOrigins(
            //        "http://localhost:5500",
            //        "http://127.0.0.1:5500",
            //        "https://localhost:5500"    // por si tu server dev usa https
            //    )
            //    .AllowAnyHeader()
            //    .AllowAnyMethod();
        }
        else
        {
            // Producción: solo orígenes configurados
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

// --- DbContext ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// --- MVC/JSON ---
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// --- 400 amigables ---
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

// --- Swagger ---
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

// --- DI ---
builder.Services.AddScoped<ICostCalculator, PublicCarCalculator>();
builder.Services.AddScoped<ICostCalculator, UberCalculator>();
builder.Services.AddScoped<ICostCalculator, OwnCarCalculator>();
builder.Services.AddScoped<ICostService, CostService>();

var app = builder.Build();

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
