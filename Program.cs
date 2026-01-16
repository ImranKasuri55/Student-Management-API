//Program.cs
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Serilog;
using Student_Management_API.Data;
using Student_Management_API.Policies;
using Student_Management_API.Services;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// ======================
// SERILOG CONFIGURATION
// ======================

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
//==============================================================
// JWT
//============================================
var jwtKey = builder.Configuration["JwtSettings:Key"];
var keyBytes = Encoding.UTF8.GetBytes(jwtKey!);

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
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

builder.Services.AddAuthorization();
//================================================================
//Register Policy
//=====================================
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AtLeast18", policy =>
//        policy.Requirements.Add(new MinimumAgeRequirement(18)));
//});

//builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

//======================================================
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
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


//==================================================

// ======================
// SERVICES
// ======================

builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddEndpointsApiExplorer();
//===============================================================
//Swagger Config
//========================================================

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Student Management API",
//        Version = "v1"
//    });

//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter JWT token like: Bearer {your token}"
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            new string[] {}
//        }
//    });
//});
//========================================================

///Add Swagger gen for version(v1,v2)
//=======================================================
//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
//    {
//        Title = "Student Management API",
//        Version = "v1"
//    });

//    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
//    {
//        Title = "Student Management API",
//        Version = "v2"
//    });
//});


//=========================================================
   //Day 19 Update Swagger Configuration
//=====================================================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student Management API",
        Version = "v1",
        Description = "Student Management System APIs (JWT Secured)",
        Contact = new OpenApiContact
        {
            Name = "API Developer",
            Email = "developer@company.com"
        }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Student Management API",
        Version = "v2",
        Description = "Student Management API Version 2"
    });

    // 🔐 JWT AUTH CONFIG
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {your_token_here}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//================================================================
//Configure Versioning in Program.cs
//============================================================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});



//====================================================
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITokenService, TokenService>();

// ======================
// APP PIPELINE
// ======================
var app = builder.Build();

// ⭐ GLOBAL EXCEPTION MIDDLEWARE (MUST BE FIRST)
app.UseMiddleware<ExceptionMiddleware>();

// ⭐ REQUEST LOGGING
app.UseSerilogRequestLogging();

// File Upload API
app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();

//app.UseSwagger();
//app.UseSwaggerUI();
//===============================================
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();

//======================================
//if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//====================================

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Student API v2");
});

//===============================================
app.MapControllers();

app.Run();
