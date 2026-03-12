using FluentValidation;
using MeAjudaAi.Application.DTOs.Profissionais;

namespace MeAjudaAi.Application.Validators.Profissionais;

public class AtualizarFormasRecebimentoRequestValidator : AbstractValidator<AtualizarFormasRecebimentoRequest>
{
    public AtualizarFormasRecebimentoRequestValidator()
    {
        RuleFor(x => x.Itens)
            .NotNull().WithMessage("A lista de formas de recebimento é obrigatória.");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.Descricao)
                .MaximumLength(300).WithMessage("Descrição deve ter no máximo 300 caracteres.");
        });
    }
}