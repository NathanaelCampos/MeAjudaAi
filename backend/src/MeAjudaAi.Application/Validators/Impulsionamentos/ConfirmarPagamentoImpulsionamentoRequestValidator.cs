using FluentValidation;
using MeAjudaAi.Application.DTOs.Impulsionamentos;

namespace MeAjudaAi.Application.Validators.Impulsionamentos;

public class ConfirmarPagamentoImpulsionamentoRequestValidator : AbstractValidator<ConfirmarPagamentoImpulsionamentoRequest>
{
    public ConfirmarPagamentoImpulsionamentoRequestValidator()
    {
        RuleFor(x => x.CodigoReferenciaPagamento)
            .NotEmpty()
            .WithMessage("Código de referência de pagamento é obrigatório.")
            .MaximumLength(150)
            .WithMessage("Código de referência de pagamento deve ter no máximo 150 caracteres.");
    }
}
