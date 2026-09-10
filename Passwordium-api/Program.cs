using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Passwordium_api;
using Passwordium_api.Data;
using Passwordium_api.Services;
using Passwordium_api.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Database

string? databaseKey =
    Environment.GetEnvironmentVariable(
        "DatabaseConnectionString"
    );

if (databaseKey == null) {
    builder.Services.AddDbContext<DatabaseContext>(
        options =>
            options.UseNpgsql(
                builder.Configuration[
                    "DatabaseConnectionString"
                ]
                ?? throw new InvalidOperationException(
                    "Database connection string not found."
                )
            )
    );
} else {
    builder.Services.AddDbContext<DatabaseContext>(
        options =>
            options.UseNpgsql(databaseKey)
    );
}

#endregion

#region JWT Key

string? jwtKey =
    Environment.GetEnvironmentVariable("JWT-Key")
    ?? builder.Configuration["JWT:Key"];

string? jwtIssuer =
    builder.Configuration["JWT:Issuer"];

string? jwtAudience =
    builder.Configuration["JWT:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience)) {
    throw new InvalidOperationException(
        "JWT configuration not found."
    );
}

builder.Services.AddSingleton(
    new AppConfiguration {
        JwtKey = jwtKey,
        JwtIssuer = jwtIssuer,
        JwtAudience = jwtAudience
    }
);

#endregion

#region Services

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<HashService>();
builder.Services.AddScoped<TokenService>();

#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

#region Authentication

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
        ),

        ValidateIssuerSigningKey = true,

        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    });

#endregion

#region Swagger

builder.Services.AddSwaggerGen();

builder.Services
    .AddTransient<
        IConfigureOptions<SwaggerGenOptions>,
        ConfigureSwaggerOptions>();

#endregion

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();