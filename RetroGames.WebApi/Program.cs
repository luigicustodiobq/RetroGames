using RetroGames.WebApi.Configuration;
using RetroGames.WebApi.Middlewares;

// Carrega o .env (procurando nas pastas acima, até a raiz do repositório) como variáveis de ambiente.
// "ConnectionStrings__Default" vira a chave de configuração "ConnectionStrings:Default".
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'ConnectionStrings__Default' não encontrada. " +
        "Crie o arquivo .env na raiz do repositório a partir do .env.example.");
}

builder.Services.AddRepositorios(connectionString);
builder.Services.AddServicosDeNegocio();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Primeiro do pipeline: captura exceções de tudo que vem depois.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI em /openapi/v1.json e a interface do Swagger em /swagger.
    app.MapOpenApi();
    app.UseSwaggerUI(opcoes => opcoes.SwaggerEndpoint("/openapi/v1.json", "RetroGames API v1"));
}

app.MapControllers();

app.Run();
