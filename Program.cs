using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Starter_NET_7.AppSettings;
using Starter_NET_7.Interfaces;
using Starter_NET_7.Services;
using System.Text;
using System.Text.Json.Serialization;
using Starter_NET_7.Database;
using Starter_NET_7.Services.Databse;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(
        x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
    );

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo()
  {
    Version = "v1",
    Title = "Starter Kit | Net 7",
    Description = "This is the documentation of Starter Kit endpoints",
    Contact = new OpenApiContact()
    {
      Name = "Alex García",
      Email = "developer6@grupotransforma.mx"
    }
  });

  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
  {
    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
  });

  c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

// Add Database Connections
builder.Services.AddDbContext<AppDbContext>(opciones => opciones.UseSqlServer("name=DefaultConnection"));

// Add JWT Auth Service
builder.Services.AddAuthentication(x =>
    {
      x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.RequireHttpsMetadata = true;
      options.SaveToken = true;

      options.TokenValidationParameters = new TokenValidationParameters()
      {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
      };
    });

builder.Services.AddHttpContextAccessor();

/*
 * Dependency injection
 */

// GenerateToken
builder.Services.AddScoped<IToken, ITokenService>();

// Database Services
builder.Services.AddScoped<RoleService, RoleService>();
builder.Services.AddScoped<UserService, UserService>();
builder.Services.AddScoped<PermissionService, PermissionService>();
builder.Services.AddScoped<PermissionsRolesService, PermissionsRolesService>();
builder.Services.AddScoped<PermissionsUsersServices, PermissionsUsersServices>();
builder.Services.AddScoped<UserValidationService, UserValidationService>();

// Email Sender
builder.Services.AddTransient<IEmailSender, IEmailSenderService>();

// ConfigApp
builder.Services.AddTransient<ConfigApp, ConfigApp>();

/*
 * Configuration Dependency
 */
builder.Services.Configure<EmailSmtpSettings>(builder.Configuration.GetSection("EmailSmtpSettings"));

/* Add cors config.*/
var policyName = "CorsPolicy";

builder.Services.AddCors(options =>
{
  options.AddPolicy(name: policyName,
      builder =>
      {
        builder
          .AllowAnyOrigin()
          .AllowAnyMethod()
          //.WithOrigins("http://localhost:3000")
          //.WithMethods("GET")
          .AllowAnyHeader();
      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(policyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
