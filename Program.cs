using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Microsoft.Extensions.Caching.Memory;
using MokkilicoresExpressAPI.Models;
using MokkilicoresExpressAPI.Services;
using MokkilicoresExpressAPI.Data;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configurar servicios
        builder.Services.AddControllers();
        // Registro del servicio ITokenService
        builder.Services.AddSingleton<ITokenService, TokenService>();


        // Registro de DbContext
        //
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<ML_DbContext>(options =>
        {
        options.UseInMemoryDatabase("ML_DB");
        //options.UseSqlServer(connectionString);
        });

        // Registro de IMemoryCache
        builder.Services.AddMemoryCache();

        // Configurar proveedores de logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.AddEventSourceLogger();

        builder.Services.AddRazorPages();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configurar autenticación con JWT
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Configurar autorización
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
        });

        var app = builder.Build();

        // Configurar el pipeline HTTP
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        };

        app.UseRouting();

        app.UseExceptionHandler(a => a.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature.Error;

            //_logger.LogError(exception, "Ocurrió un error no manejado");

            await context.Response.WriteAsJsonAsync(new { error = "An error occurred while processing your request." });
        }));


        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        InitializeDatabase(app.Services);

        app.Run();

        void InitializeDatabase(IServiceProvider services)
        {
        using (var scope = services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ML_DbContext>();

            if (!dbContext.Cliente.Any())
            {
                List<Cliente> initialClientes = new List<Cliente>
                {
                    new Cliente { Identificacion = "023456789", Nombre = "Mario", Apellido = "Lopez" },
                    new Cliente { Identificacion = "admin", Nombre = "Admin", Apellido = "Admin" }
                };
                dbContext.Cliente.AddRange(initialClientes);
            }

            if (!dbContext.Inventario.Any())
            {
                List<Inventario> initialInventarios = new List<Inventario>
                {
                    new Inventario { CantidadEnExistencia = 100, BodegaId = 1, Precio = 10000, FechaIngreso = DateTime.Now, FechaVencimiento = DateTime.Now.AddYears(1), TipoLicor = "Vodka" },
                    new Inventario { CantidadEnExistencia = 50, BodegaId = 2, Precio = 35000, FechaIngreso = DateTime.Now, FechaVencimiento = DateTime.Now.AddYears(1), TipoLicor = "Whisky" }
                };
                dbContext.Inventario.AddRange(initialInventarios);
            }

            if (!dbContext.Direccion.Any())
            {
                List<Direccion> initialDirecciones = new List<Direccion>
                {
                    new Direccion { ClienteId = 1, Provincia = "Cartago", Canton = "La Union", Distrito = "Tres Rios", PuntoEnWaze = "waze://?ll=9.8998,-83.9876", EsCondominio = false, EsPrincipal = true },
                    new Direccion { ClienteId = 2, Provincia = "San Jose", Canton = "San Jose", Distrito = "San Jose", PuntoEnWaze = "waze://?ll=9.9325,-84.0796", EsCondominio = false, EsPrincipal = false }
                };
                dbContext.Direccion.AddRange(initialDirecciones);
            }

            dbContext.SaveChanges();
        }
        }
    }
}
