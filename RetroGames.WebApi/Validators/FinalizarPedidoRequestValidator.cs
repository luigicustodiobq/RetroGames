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
    }
}
