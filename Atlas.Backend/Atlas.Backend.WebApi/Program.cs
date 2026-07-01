using FluentValidation;
using Atlas.Backend.Application.DTOs.Entities;
using Atlas.Backend.Application.DTOs.Relationships;
using Atlas.Backend.Application.Services;
using Atlas.Backend.Infrastructure;
using Atlas.Backend.Infrastructure.Neo4j;
using Atlas.Backend.WebApi.Converters;
using Atlas.Backend.WebApi.CustomExceptions;
using Atlas.Backend.WebApi.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new CustomEnumConverterFactory());
        o.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

// Authentication (Entra ID)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(jwtOptions =>
    {
        jwtOptions.MapInboundClaims = false;

        jwtOptions.TokenValidationParameters.RoleClaimType = "roles";
        jwtOptions.TokenValidationParameters.NameClaimType = "preferred_username";
    },
    identityOptions =>
    {
        builder.Configuration.Bind("AzureAd", identityOptions);
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ReaderPolicy", policy =>
        policy.RequireClaim("roles", "Reader", "Editor"));

    options.AddPolicy("EditorPolicy", policy =>
        policy.RequireClaim("roles", "Editor"));
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<AssetCreateDtoValidator>();

// SharpGrip auto-validation
builder.Services.AddFluentValidationAutoValidation(configuration =>
{
    configuration.DisableBuiltInModelValidation = true;

    configuration.EnableBodyBindingSourceAutomaticValidation = true;
    configuration.EnableFormBindingSourceAutomaticValidation = true;
    configuration.EnableQueryBindingSourceAutomaticValidation = true;
    configuration.EnablePathBindingSourceAutomaticValidation = true;
    configuration.EnableCustomBindingSourceAutomaticValidation = true;

    configuration.OverrideDefaultResultFactoryWith<SharpGripValidationResultFactory>();
});

builder.Services.AddEndpointsApiExplorer();

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? throw new InvalidOperationException("Config mancante: 'Cors:AllowedOrigins'.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
            .WithHeaders("Authorization", "Content-Type")
            .AllowCredentials();
    });
});

// Neo4j config...
string GetRequired(string configKey, string envKey)
    => Environment.GetEnvironmentVariable(envKey)
    ?? builder.Configuration[configKey]
    ?? throw new InvalidOperationException(
        $"Config mancante: '{configKey}' / '{envKey}'");

var neo4jSettings = new Neo4jSettings
{
    Uri = GetRequired("Neo4j:Uri", "NEO4J_URI"),
    User = GetRequired("Neo4j:User", "NEO4J_USER"),
    Password = GetRequired("Neo4j:Password", "NEO4J_PASSWORD"),
    Database = GetRequired("Neo4j:Database", "NEO4J_DATABASE"),
};

builder.Services.AddInfrastructure(neo4jSettings);

// Application services
builder.Services.AddScoped<AssetService>();
builder.Services.AddScoped<DivisionService>();
builder.Services.AddScoped<ServiceService>();
builder.Services.AddScoped<ProcessService>();
builder.Services.AddScoped<AcnMacroAreaService>();
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<SettingService>();
builder.Services.AddScoped<LoginTypeService>();
builder.Services.AddScoped<CloudProviderService>();
builder.Services.AddScoped<VirtualMachineService>();
builder.Services.AddScoped<RelationshipsV2Service>();
builder.Services.AddScoped<RelationshipGraphService>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// Trust X-Forwarded-* headers when running behind reverse proxy (Traefik in Docker).
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

//app.UseHttpsRedirection();

app.UseCors("ReactFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow }));

app.Run();