namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de POST /api/Autenticacao/cadastrar (C01). O formato é conferido pelo validator.</summary>
public class CadastrarClienteRequestApiModel
{
    public string? Nome { get; set; }

    public string? Email { get; set; }

    public string? Senha { get; set; }
}
