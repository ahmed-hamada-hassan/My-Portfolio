using API.Extenstions;
using API.Middlewares;
using Application;
using Application.Behaviors;
using Application.Interfaces.DI;
using FluentValidation;
using Infrastructure;
using Infrastructure.Data.EF_Core;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOptions<CloudinaryOptions>()
    .Bind(builder.Configuration.GetSection(CloudinaryOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddOptions<SiteSettingsOptions>()
    .Bind(builder.Configuration.GetSection(SiteSettingsOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "portfolio_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddHttpContextAccessor();

builder.Services.AddAppRateLimiter();

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

builder.Services.AddValidatorsFromAssembly(typeof(IApplicationAssemblyMarker).Assembly, includeInternalTypes: true);

builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies = [typeof(IApplicationAssemblyMarker).Assembly];
    options.PipelineBehaviors = [
        typeof(LoggingPipelineBehavior<,>),
        typeof(ValidatePipelineBehavior<,>)
    ];
});

var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins(allowedOrigins!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Host.UseSerilog((context, service, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(service)
    .Enrich.FromLogContext());

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseCors("AngularClient");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();
