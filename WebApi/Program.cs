using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using WebApi.AutoMapper;
using WebApi.Models;
using WebApi.Utilities;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowFrontend_";

if (builder.Environment.IsDevelopment())
{
    // Disable Browser Link
    builder.WebHost.ConfigureServices(services =>
    {
        services.AddRouting(options => options.SuppressCheckForUnhandledSecurityMetadata = true);
    });
}

// 1) Registriramo CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy
              .AllowAnyOrigin() // For development - allows all origins
              .AllowAnyHeader()
              .AllowAnyMethod();
        });
});

// 2) Dodajemo kontrolere i konfiguriramo JSON (bez cikličkih referenca)
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// 3) Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "PRA Web API", Version = "v1" });

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter valid JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new List<string>()
        }
    });
});

// 4) JWT autentikacija
var secureKey = builder.Configuration["JWT:SecureKey"];
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        var Key = Encoding.UTF8.GetBytes(secureKey);
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Key)
        };
    });

// 5) Ostali servisi
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IDbService, DbServices>();

// Register QuizPlayService
builder.Services.AddScoped<IQuizPlayService, QuizPlayService>();

// Create a factory for IDbService
builder.Services.AddTransient<Func<IDbService>>(serviceProvider =>
    () => serviceProvider.GetRequiredService<IDbService>());

builder.Services.AddDbContext<PraContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PRAcs")
    );
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

// 6) Ako je razvojno, uključimo Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 7) OBAVEZNO: prvo CORS, pa autentikacija/autorizaija
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

// 9) Mapiranje kontrolera
app.MapControllers();

app.UseStaticFiles();
app.Run();
