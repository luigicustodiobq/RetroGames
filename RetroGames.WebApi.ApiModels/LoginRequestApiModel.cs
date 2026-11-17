namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de POST /api/Autenticacao/login.</summary>
public class LoginRequestApiModel
{
    public string? Email { get; set; }

    public string? Senha { get; set; }
}
