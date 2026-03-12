using FluentValidation;
using MeAjudaAi.Application.DTOs.Impulsionamentos;

namespace MeAjudaAi.Application.Validators.Impulsionamentos;

public class WebhookPagamentoImpulsionamentoRequestValidator : AbstractValidator<WebhookPagamentoImpulsionamentoRequest>
{
    public WebhookPagamentoImpulsionamentoRequestValidator()
    {
        RuleFor(x => x.CodigoReferenciaPagamento)
            .NotEmpty()
            .WithMessage("Código de referência de pagamento é obrigatório.")
            .MaximumLength(150)
            .WithMessage("Código de referência de pagamento deve ter no máximo 150 caracteres.");

        RuleFor(x => x.StatusPagamento)
            .NotEmpty()
            .WithMessage("Status do pagamento é obrigatório.")
            .Must(status => StatusSuportado(status))
            .WithMessage("Status do pagamento inválido.");

        RuleFor(x => x.EventoExternoId)
            .MaximumLength(150)
            .WithMessage("Id do evento externo deve ter no máximo 150 caracteres.");
    }

    private static bool StatusSuportado(string? statusPagamento)
    {
        if (string.IsNullOrWhiteSpace(statusPagamento))
            return false;

        var status = statusPagamento.Trim().ToLowerInvariant();

        return status is "pago" or "cancelado" or "recusado" or "estornado" or "expirado";
    }
}
