using System.Text;
using AIW.Application.Common.Interfaces;
using AIW.Infrastructure.Identity;
using AIW.Application.Documents.Services;
using AIW.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using AIW.Application.Analysis.Services;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var aiServiceBaseUrl = builder.Configuration["AiService:BaseUrl"] ?? "http://localhost:8000";
var timeoutSeconds = builder.Configuration.GetValue<int>("AiService:TimeoutSeconds", 120);

// 1. Controller & validation configuration
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<IAiServiceClient, AiServiceClient>(client =>
{
    client.BaseAddress = new Uri(aiServiceBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddHttpClient<IAgentReviewClient, AgentReviewClient>(client =>
{
    client.BaseAddress = new Uri(aiServiceBaseUrl);
    client.Timeout = Timeout.InfiniteTimeSpan; 
});

// Register .NET 9 native OpenAPI
builder.Services.AddOpenApi();

// 2. Database registration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    ));

builder.Services.AddScoped<IApplicationDbContext>(sp => 
    sp.GetRequiredService<ApplicationDbContext>());

builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentExtractor, DocumentExtractor>();
// 3. CORS policy for Angular dev server
const string corsPolicyName = "AllowAngularDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
builder.Services.AddScoped<IIdentityService, IdentityService>();

var jwtKey = builder.Configuration["Jwt:SecretKey"] ?? "AIW_Platform_Super_Secret_Key_32_Bytes_Long_Minimum!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "AIW-Platform",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "AIW-Platform-Client",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
    options.AddPolicy("RequireMemberOrAdmin", p => p.RequireRole("Member", "Admin"));
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // Expose /openapi/v1.json
    app.MapOpenApi();
    // Expose interactive API documentation at /scalar/v1
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);
app.UseAuthorization();
app.MapControllers();

app.Run();