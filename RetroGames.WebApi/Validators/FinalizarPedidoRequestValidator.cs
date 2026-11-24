using FluentValidation;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Validators;

/// <summary>
/// Formato da finalização da compra (C04). Se o jogo existe, está ativo e tem estoque
/// é conferido no Service, que consulta o banco.
/// </summary>
public class FinalizarPedidoRequestValidator : AbstractValidator<FinalizarPedidoRequestApiModel>
{
    public FinalizarPedidoRequestValidator()
    {
        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("Informe ao menos um item.");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.IdJogo)
                .GreaterThan(0).WithMessage("Informe o jogo.");

            item.RuleFor(i => i.Quantidade)
                .InclusiveBetween(1, 10).WithMessage("A quantidade deve ser de 1 a 10.");
        });

        // tb_item_pedido tem UNIQUE (co_pedido, co_jogo): o Service junta o mesmo jogo num único item.
        // Por isso a quantidade final de cada jogo (soma das repetições) também precisa ficar entre 1 e 10.
        RuleFor(x => x.Itens)
            .Must(QuantidadeFinalPorJogoAteDez)
            .WithMessage("A quantidade total de cada jogo deve ser de 1 a 10.")
            .When(x => x.Itens is { Count: > 0 });
    }

    private static bool QuantidadeFinalPorJogoAteDez(List<ItemPedidoRequestApiModel>? itens)
    {
        return (itens ?? new List<ItemPedidoRequestApiModel>())
            .GroupBy(item => item.IdJogo)
            .All(grupo => grupo.Sum(item => item.Quantidade) is >= 1 and <= 10);
    }
}
