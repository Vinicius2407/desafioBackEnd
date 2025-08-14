using Microsoft.EntityFrameworkCore;
using Engine.Singleton;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Engine.Interfaces;

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    try
    {
        // Codigo para geração de migration em desenvolvimento
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DBApp>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrações: {ex.Message}");
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
