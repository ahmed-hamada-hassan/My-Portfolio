using Application;
using Application.Interfaces.DI;
using Infrastructure;
using Infrastructure.Data.EF_Core;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.Scan(scan => scan
    .FromAssemblies(
        typeof(IApplicationAssemblyMarker).Assembly,
        typeof(IInfrastructureAssemblyMarker).Assembly)

    .AddClasses(classes => classes.AssignableTo<ISingletonMark>(), publicOnly: false)
        .AsImplementedInterfaces(type => type != typeof(ISingletonMark))
        .AsSelf()
        .WithSingletonLifetime()

    .AddClasses(classes => classes.AssignableTo<IScopedMark>(), publicOnly: false)
        .AsImplementedInterfaces(type => type != typeof(IScopedMark))
        .AsSelf()
        .WithScopedLifetime()
);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
