using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Validators;

namespace RetroGames.WebApi.UnitTests.Validators;

/// <summary>
/// Formato de POST /api/Pedido (C04). Um erro aqui vira HTTP 400 com responseCode 3
/// (FluentValidationFilter), antes de chegar ao Service.
/// </summary>
public class FinalizarPedidoRequestValidatorTests
{
    private readonly FinalizarPedidoRequestValidator validator = new();

    private static FinalizarPedidoRequestApiModel Pedido(params (int IdJogo, int Quantidade)[] itens) => new()
    {
        Itens = itens.Select(i => new ItemPedidoRequestApiModel { IdJogo = i.IdJogo, Quantidade = i.Quantidade }).ToList()
    };

    [Fact]
    public void Validar_ItensValidos_NaoTemErros()
    {
        var resultado = validator.Validate(Pedido((1, 2), (2, 10)));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Validar_ListaVazia_TemErroEmItens()
    {
        var resultado = validator.Validate(Pedido());

        Assert.Contains(resultado.Errors, erro => erro.PropertyName == "Itens");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Validar_QuantidadeForaDaFaixa_TemErro(int quantidade)
    {
        var resultado = validator.Validate(Pedido((1, quantidade)));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Validar_JogoRepetidoSomandoMaisDeDez_TemErroDeQuantidadeTotal()
    {
        // 6 + 5 = 11 unidades do mesmo jogo: cada linha é válida, mas o item consolidado não.
        var resultado = validator.Validate(Pedido((1, 6), (1, 5)));

        Assert.Contains(resultado.Errors, erro => erro.ErrorMessage == "A quantidade total de cada jogo deve ser de 1 a 10.");
    }

    [Fact]
    public void Validar_JogoRepetidoSomandoAteDez_EhValido()
    {
        // 4 + 6 = 10: o Service consolida num único item de 10 unidades.
        var resultado = validator.Validate(Pedido((1, 4), (1, 6)));

        Assert.True(resultado.IsValid);
    }
}
