using Microsoft.EntityFrameworkCore;
using Engine.Singleton;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Engine.Interfaces;
using Models.Model;

var builder = WebApplication.CreateBuilder(args);

// Adicionando configuração do banco dentro do contexto do DBApp
var connectionString = builder.Configuration.GetConnectionString("DBApp");
builder.Services.AddDbContext<DBApp>(options => options.UseNpgsql(connectionString, b => b.MigrationsAssembly("Engine")));


// Services
// Modelo dinamico para carregar as Services
var assembly = typeof(Engine.Services.UserService).Assembly;
var serviceTypes = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(IService).IsAssignableFrom(t));

foreach (var type in serviceTypes)
{
    builder.Services.AddScoped(type);
}


// Configuração da autenticação JWT
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:SecretKey"])
            )
        };
    });


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DBApp>();
    // Codigo para geração de migration
    db.Database.Migrate();

    if (!db.Currencies.Any())
    {
        var currencies = new List<Currency>
        {
            new(){
                Code = "BRL",
                Name = "Real",
                Symbol = "R$"
            },
            new(){
                Code = "USD",
                Name = "Dolár",
                Symbol = "$"
            },
            new(){
                Code = "EUR",
                Name = "Euro",
                Symbol = "€"
            }
        };

        db.Currencies.AddRange(currencies);
        db.SaveChanges();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.Message}");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
