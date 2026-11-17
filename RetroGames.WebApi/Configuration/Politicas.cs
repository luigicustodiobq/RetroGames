namespace RetroGames.WebApi.Configuration;

/// <summary>Nomes das políticas de autorização usadas em [Authorize(Policy = ...)].</summary>
public static class Politicas
{
    /// <summary>Exige usuário logado com o perfil Administrador.</summary>
    public const string SomenteAdministrador = "SomenteAdministrador";
}
