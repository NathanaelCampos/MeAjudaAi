using FluentValidation;
using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.Validators.Notificacoes;

public class AtualizarPreferenciasNotificacaoRequestValidator : AbstractValidator<AtualizarPreferenciasNotificacaoRequest>
{
    public AtualizarPreferenciasNotificacaoRequestValidator()
    {
        RuleFor(x => x.Preferencias)
            .NotEmpty()
            .WithMessage("Informe ao menos uma preferência.")
            .Must(x => x.Select(p => p.Tipo).Distinct().Count() == x.Count)
            .WithMessage("Não é permitido informar tipos de notificação duplicados.");

        RuleForEach(x => x.Preferencias)
            .ChildRules(preferencia =>
            {
                preferencia.RuleFor(x => x.Tipo)
                    .IsInEnum()
                    .Must(TipoNotificacaoValido)
                    .WithMessage("Tipo de notificação inválido.");
            });
    }

    private static bool TipoNotificacaoValido(TipoNotificacao tipo)
    {
        return Enum.IsDefined(typeof(TipoNotificacao), tipo);
    }
}
