using AutoMapper;
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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
        ),

        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();


builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conn))
{
    
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlServer(conn));
}

builder.Services.AddScoped<IRepository, EfRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}





using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    DbInitializer.Seed(db);

    var repo = scope.ServiceProvider.GetRequiredService<IRepository>();

    if (!db.Set<Merchant>().Any(m => m.MerchantId == "SHOP-123"))
    {
        var (raw, stored) = ApiKeyHasher.Generate();
        var merchant = new Merchant
        {
            MerchantId = "SHOP-123",
            Name = "Dev Shop 123",
            ApiKeyHash = stored,
            IsActive = true,
            AllowedReturnUrls = System.Text.Json.JsonSerializer.Serialize(new[] { "https://localhost:3000" }),
            Domain = "localhost"
        };
        repo.AddMerchant(merchant);

        Console.WriteLine("Seeded merchant SHOP-123 raw key (store safely): " + raw);
    }
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseMiddleware<TenantResolverMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
