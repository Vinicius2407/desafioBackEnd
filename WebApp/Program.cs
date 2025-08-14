using Microsoft.EntityFrameworkCore;
using Engine.Singleton;
using Microsoft.IdentityModel.Tokens;                // Adicione esta linha
using System.Text;
using System.Reflection;
using Engine.Interfaces;                                   // Adicione esta linha

var builder = WebApplication.CreateBuilder(args);

// Configuração do Entity Framework com PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DBApp");
builder.Services.AddDbContext<DBApp>(options => options.UseNpgsql(connectionString, b => b.MigrationsAssembly("Engine")));


// Services
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
