using FluentValidation;
using MeAjudaAi.Application.DTOs.Notificacoes;

namespace MeAjudaAi.Application.Validators.Notificacoes;

public class ExportarEmailsOutboxRequestValidator : AbstractValidator<ExportarEmailsOutboxRequest>
{
    public ExportarEmailsOutboxRequestValidator()
    {
        RuleFor(x => x.Limite)
            .InclusiveBetween(1, 5000);

        RuleFor(x => x)
            .Must(TemAlgumFiltro)
            .WithMessage("Informe pelo menos um filtro para a exportação.");
    }

    private static bool TemAlgumFiltro(ExportarEmailsOutboxRequest request)
    {
        return request.Status.HasValue
            || request.UsuarioId.HasValue
            || request.TipoNotificacao.HasValue
            || !string.IsNullOrWhiteSpace(request.EmailDestino)
            || request.DataCriacaoInicial.HasValue
            || request.DataCriacaoFinal.HasValue;
    }
}
