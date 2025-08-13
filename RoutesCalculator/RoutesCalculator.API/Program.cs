using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RoutesCalculator.Application.Contracts;
using RoutesCalculator.Application.Services;
using RoutesCalculator.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ?? CORS
const string CorsPolicy = "CorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // En desarrollo: permitir todo
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // En producción: limitar a orígenes configurados
            var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            if (allowed.Length > 0)
            {
                policy.WithOrigins(allowed)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
            else
            {
                // Si no configuras nada, no permite orígenes cruzados
                policy.WithOrigins(Array.Empty<string>());
            }
        }
    });
});

// ?? DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ?? Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ?? Respuesta de validaciones (400 amigable)
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

// ?? Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Calculadora de Rutas y Costos",
        Version = "v1",
        Description = "Calcula costos de viaje (carro público, Uber, carro propio) y sugiere la opción más económica."
    });
});

// ?? DI de servicios
builder.Services.AddScoped<ICostCalculator, PublicCarCalculator>();
builder.Services.AddScoped<ICostCalculator, UberCalculator>();
builder.Services.AddScoped<ICostCalculator, OwnCarCalculator>();
builder.Services.AddScoped<ICostService, CostService>();

var app = builder.Build();

// ?? Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ?? Activar CORS
app.UseCors(CorsPolicy);

app.MapControllers();

app.Run();
