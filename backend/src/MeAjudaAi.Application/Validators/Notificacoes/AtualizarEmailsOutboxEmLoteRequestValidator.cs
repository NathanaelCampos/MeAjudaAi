using FluentValidation;
using MeAjudaAi.Application.DTOs.Notificacoes;

namespace MeAjudaAi.Application.Validators.Notificacoes;

public class AtualizarEmailsOutboxEmLoteRequestValidator : AbstractValidator<AtualizarEmailsOutboxEmLoteRequest>
{
    public AtualizarEmailsOutboxEmLoteRequestValidator()
    {
        RuleFor(x => x.Limite)
            .InclusiveBetween(1, 500);

        RuleFor(x => x)
            .Must(TemAlgumFiltro)
            .WithMessage("Informe pelo menos um filtro para a operação em lote.");
    }

    private static bool TemAlgumFiltro(AtualizarEmailsOutboxEmLoteRequest request)
    {
        return request.Status.HasValue
            || request.UsuarioId.HasValue
            || request.TipoNotificacao.HasValue
            || !string.IsNullOrWhiteSpace(request.EmailDestino)
            || request.DataCriacaoInicial.HasValue
            || request.DataCriacaoFinal.HasValue;
    }
}
