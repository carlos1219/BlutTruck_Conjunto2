using Application.Services;
using BlutTruck.Application_Layer.IServices;
using BlutTruck.Application_Layer.Models;
using BlutTruck.Application_Layer.Services;
using BlutTruck.Data_Access_Layer.IRepositories;
using BlutTruck.Data_Access_Layer.Repositories;
using BlutTruck.Transversal_Layer.Helper;
using BlutTruck.Transversal_Layer.IHelper;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.OpenApi.Models; // Asegúrate de tener esta importación para Swagger

var builder = WebApplication.CreateBuilder(args);

// **1. Add services to the container**
builder.Services.AddControllers();

// **2. Register HttpClient**
// This ensures the IHttpClientFactory is available for DI
builder.Services.AddHttpClient();


// **3. Register application services**
builder.Services.AddScoped<IHealthDataService, HealthDataService>();
builder.Services.AddScoped<IHealthDataRepository, HealthDataRepository>();
builder.Services.AddScoped<IHelper, Helper>();
builder.Services.AddScoped<IApiRepository, ApiRepository>();
builder.Services.AddScoped<IPrediccionService, PrediccionService>();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// **4. Add Swagger for API documentation**
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Definir un esquema de seguridad para JWT Bearer Token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Description = "Por favor ingresa tu token JWT con el prefijo 'Bearer'. Ejemplo: 'Bearer {tu_token}'"
    });

    // Agregar el requisito de seguridad para todas las operaciones
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] { }
        }
    });
});

// **5. Configure CORS policy**
// **5. Configure CORS policy**
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy
            .AllowAnyOrigin()  // Permite cualquier origen
            .AllowAnyMethod()  // Permite cualquier método HTTP (GET, POST, etc.)
            .AllowAnyHeader(); // Permite cualquier encabezado
    });
});



// **6. Firebase Initialization**
// Credenciales de Firebase: nunca se versionan. Se leen de variables de entorno:
//   FIREBASE_CREDENTIALS_JSON  -> contenido del JSON de la cuenta de servicio
//   FIREBASE_CREDENTIALS_PATH  -> ruta a un fichero montado (p. ej. como secret de Docker)
if (FirebaseApp.DefaultInstance == null)
{
    var credJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
    var credPath = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_PATH");
    GoogleCredential credential = !string.IsNullOrWhiteSpace(credJson)
        ? GoogleCredential.FromJson(credJson)
        : !string.IsNullOrWhiteSpace(credPath)
            ? GoogleCredential.FromFile(credPath)
            : throw new InvalidOperationException("Define FIREBASE_CREDENTIALS_JSON o FIREBASE_CREDENTIALS_PATH");
    FirebaseApp.Create(new AppOptions { Credential = credential });
}
// **7. Build the application**
var app = builder.Build();

// **8. Configure the HTTP request pipeline**
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Use the configured CORS policy
app.UseCors("AllowAllOrigins");


// Use Authorization Middleware
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Run the application
app.Run();
