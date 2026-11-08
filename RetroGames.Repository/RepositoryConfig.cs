namespace RetroGames.Repository;

/// <summary>
/// Configuração usada por todos os repositórios.
/// O valor da connection string será preenchido pela API (aula 4), a partir do arquivo .env.
/// </summary>
public class RepositoryConfig
{
    public string ConnectionString { get; set; } = string.Empty;
}
