using JobHunter.Application;
using JobHunter.Api.Authorization;
using JobHunter.Api.Contracts;
using JobHunter.Api.Filters;
using JobHunter.Api.Middleware;
using JobHunter.Infrastructure;
using JobHunter.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ApiResponseEnvelopeFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var response = ApiErrorEnvelopeFactory.CreateFromModelState(context.ModelState);

            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure CORS policy to match Java implementation
const string corsPolicy = "JobHunterCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policyBuilder =>
    {
        policyBuilder
            .WithOrigins("http://localhost:3000", "http://localhost:4173", "http://localhost:5173")
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type", "Accept", "x-no-retry")
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition") // For file downloads
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
    });
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var bearerScheme = new OpenApiSecurityScheme
    {
        Description = "JWT access token. Example: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);

    var refreshTokenCookieScheme = new OpenApiSecurityScheme
    {
        Description = "Refresh token cookie value for refresh endpoint.",
        Name = "refresh_token",
        In = ParameterLocation.Cookie,
        Type = SecuritySchemeType.ApiKey
    };

    options.AddSecurityDefinition("RefreshTokenCookie", refreshTokenCookieScheme);

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DefaultModelsExpandDepth(-1);
        options.EnablePersistAuthorization();
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();

var uploadBasePath = builder.Configuration["FileStorage:BasePath"];
if (!string.IsNullOrWhiteSpace(uploadBasePath))
{
    var staticPath = Path.IsPathRooted(uploadBasePath)
        ? uploadBasePath
        : Path.Combine(app.Environment.ContentRootPath, uploadBasePath);

    Directory.CreateDirectory(staticPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(staticPath),
        RequestPath = "/storage"
    });
}

app.UseCors(corsPolicy);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
