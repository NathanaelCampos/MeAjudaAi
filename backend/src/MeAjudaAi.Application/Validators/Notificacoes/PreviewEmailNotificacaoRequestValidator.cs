using FluentValidation;
using MeAjudaAi.Application.DTOs.Notificacoes;

namespace MeAjudaAi.Application.Validators.Notificacoes;

public class PreviewEmailNotificacaoRequestValidator : AbstractValidator<PreviewEmailNotificacaoRequest>
{
    public PreviewEmailNotificacaoRequestValidator()
    {
        RuleFor(x => x.Assunto)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Corpo)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
