using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using webshop_back.Data;
using webshop_back.Data.Mapping;
using webshop_back.Data.Models;
using webshop_back.Data.Seed;
using webshop_back.Helpers;
using webshop_back.Service;
using webshop_back.Service.Interfaces;
using webshop_back.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// DbContext
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRepository, EfRepository>();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseMiddleware<TenantResolverMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// DB migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    DbInitializer.Seed(db);

    var repo = scope.ServiceProvider.GetRequiredService<IRepository>();

    if (!db.Set<Merchant>().Any())
    {
        var (raw, stored) = ApiKeyHasher.Generate();
        repo.AddMerchant(new Merchant
        {
            MerchantId = "SHOP-123",
            Name = "Dev Shop 123",
            ApiKeyHash = stored,
            IsActive = true,
            AllowedReturnUrls = System.Text.Json.JsonSerializer.Serialize(new[] { "http://localhost:5173/payment-result" }),
            Domain = "shop1.localhost"
        });
        Console.WriteLine("Seeded merchant SHOP-123 raw key: " + raw);

        (raw, stored) = ApiKeyHasher.Generate();
        repo.AddMerchant(new Merchant
        {
            MerchantId = "SHOP-321",
            Name = "Dev Shop 321",
            ApiKeyHash = stored,
            IsActive = true,
            AllowedReturnUrls = System.Text.Json.JsonSerializer.Serialize(new[] { "http://localhost:5174/payment-result" }),
            Domain = "shop2.localhost"
        });
        Console.WriteLine("Seeded merchant SHOP-321 raw key: " + raw);
    }
}

app.Run();

