using FluentValidation;
using MeAjudaAi.Application.DTOs.Impulsionamentos;

namespace MeAjudaAi.Application.Validators.Impulsionamentos;

public class ContratarPlanoImpulsionamentoRequestValidator : AbstractValidator<ContratarPlanoImpulsionamentoRequest>
{
    public ContratarPlanoImpulsionamentoRequestValidator()
    {
        RuleFor(x => x.PlanoImpulsionamentoId)
            .NotEmpty()
            .WithMessage("Plano de impulsionamento é obrigatório.");

        RuleFor(x => x.CodigoReferenciaPagamento)
            .MaximumLength(150)
            .WithMessage("Código de referência de pagamento deve ter no máximo 150 caracteres.");
    }
}
