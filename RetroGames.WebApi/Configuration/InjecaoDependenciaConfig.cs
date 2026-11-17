using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.Configuration;

/// <summary>Registro das classes das camadas Repository e Service na injeção de dependência.</summary>
public static class InjecaoDependenciaConfig
{
    /// <summary>
    /// RepositoryConfig é Singleton (a connection string não muda durante a execução);
    /// os repositórios e o RepositorySet são Scoped (um objeto por requisição).
    /// </summary>
    public static IServiceCollection AddRepositorios(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(new RepositoryConfig { ConnectionString = connectionString });

        services.AddScoped<IPlataformaRepository, PlataformaRepository>();
        services.AddScoped<IJogoRepository, JogoRepository>();
        services.AddScoped<IJogoGeneroRepository, JogoGeneroRepository>();
        services.AddScoped<IGeneroRepository, GeneroRepository>();
        services.AddScoped<ICondicaoJogoRepository, CondicaoJogoRepository>();
        services.AddScoped<IRepositorySet, RepositorySet>();

        return services;
    }

    /// <summary>Os Services de negócio, um objeto por requisição.</summary>
    public static IServiceCollection AddServicosDeNegocio(this IServiceCollection services)
    {
        services.AddScoped<IPlataformaService, PlataformaService>();
        services.AddScoped<IGeneroService, GeneroService>();
        services.AddScoped<ICondicaoJogoService, CondicaoJogoService>();
        services.AddScoped<IJogoService, JogoService>();

        return services;
    }
}
