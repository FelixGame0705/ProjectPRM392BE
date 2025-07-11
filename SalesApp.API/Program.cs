using System.Text;
using AutoMapper;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SalesApp.API.Configurations;
using SalesApp.API.SignalR;
using SalesApp.BLL.Mapping;
using SalesApp.BLL.Services;
using SalesApp.DAL.Data;
using SalesApp.DAL.Repositories;
using SalesApp.DAL.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<SalesAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Register repositories and unit of work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartItemService, CartItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<ITransactionMappingService, TransactionMappingService>();
builder.Services.AddScoped<IStoreLocationService, StoreLocationService>();

// Register memory cache for billing details and transaction mapping
builder.Services.AddMemoryCache();

// Register HttpClient for payment gateway services
builder.Services.AddHttpClient();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);
builder.Services.AddSingleton<Cloudinary>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
    return new Cloudinary(account);
});
builder.Services.AddSingleton(provider =>
    new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<MappingProfile>();
    }).CreateMapper()
);

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
//    {
//        Title = "Sales App API",
//        Version = "v1",
//        Description = "API for Sales Application"
//    });
//});

builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your JWT Access Token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme,
        },
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } }
    );
});

// Add JWT Authentication
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
            ValidAudience = builder.Configuration["JwtConfig:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"])
            ),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });
builder.Services.AddAuthorization();

// Add SingalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Enable detailed errors for debugging
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sales App API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at apps root
    });
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sales App API V1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at apps root
});
app.MapHub<ChatHub>("/chathub"); // Map SignalR hub
app.UseStaticFiles();

//app.UseHttpsRedirection();

app.MapGet("/", () => "It works!");
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
