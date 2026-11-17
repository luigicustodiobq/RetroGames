using FluentValidation;
using Microsoft.AspNetCore.Identity;
using RetroGames.Model.Model;
using RetroGames.Services;
using RetroGames.WebApi.Configuration;
using RetroGames.WebApi.Filters;
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

// Hash de senha (PBKDF2 com salt) e autenticação.
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IAutenticacaoService, AutenticacaoService>();
builder.Services.AddAutenticacaoPorCookie();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Todos os validators (AbstractValidator<T>) deste projeto entram na injeção de dependência.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services
    .AddControllers(opcoes =>
    {
        opcoes.Filters.Add<FluentValidationFilter>();
        // Corpo vazio chega como null na action, e o FluentValidationFilter responde MODEL_NULL (2).
        opcoes.AllowEmptyInputInBodyModelBinding = true;
        // Sem "required" implícito para string não anulável: o formato é validado só pelo FluentValidation.
        opcoes.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .ConfigureApiBehaviorOptions(opcoes =>
        opcoes.InvalidModelStateResponseFactory = ModelStateInvalidoConfig.CriarResposta);
builder.Services.AddOpenApi();

var app = builder.Build();

// Cria o administrador inicial (dados do .env) se ainda não existir nenhum administrador.
// Os Services são Scoped, por isso é preciso abrir um escopo fora de uma requisição.
using (var escopo = app.Services.CreateScope())
{
    var emailAdmin = app.Configuration["AdminInicial:Email"];
    var senhaAdmin = app.Configuration["AdminInicial:Senha"];
    if (string.IsNullOrWhiteSpace(emailAdmin) || string.IsNullOrWhiteSpace(senhaAdmin))
    {
        throw new InvalidOperationException(
            "Dados do administrador inicial não encontrados. " +
            "Informe AdminInicial__Email e AdminInicial__Senha no arquivo .env.");
    }

    var autenticacaoService = escopo.ServiceProvider.GetRequiredService<IAutenticacaoService>();
    await autenticacaoService.GarantirAdministradorInicialAsync(emailAdmin, senhaAdmin);
}

// Primeiro do pipeline: captura exceções de tudo que vem depois.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI em /openapi/v1.json e a interface do Swagger em /swagger.
    app.MapOpenApi();
    app.UseSwaggerUI(opcoes => opcoes.SwaggerEndpoint("/openapi/v1.json", "RetroGames API v1"));
}

// Nesta ordem: primeiro descobre quem é o usuário (cookie), depois confere se ele pode acessar a rota.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
