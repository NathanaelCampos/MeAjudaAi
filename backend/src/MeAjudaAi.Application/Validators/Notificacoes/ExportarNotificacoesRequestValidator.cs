using FluentValidation;
using MeAjudaAi.Application.DTOs.Notificacoes;

namespace MeAjudaAi.Application.Validators.Notificacoes;

public class ExportarNotificacoesRequestValidator : AbstractValidator<ExportarNotificacoesRequest>
{
    public ExportarNotificacoesRequestValidator()
    {
        RuleFor(x => x.Limite)
            .InclusiveBetween(1, 5000);

        RuleFor(x => x)
            .Must(TemAlgumFiltro)
            .WithMessage("Informe pelo menos um filtro para a exportação.");
    }

    private static bool TemAlgumFiltro(ExportarNotificacoesRequest request)
    {
        return request.UsuarioId.HasValue
            || request.TipoNotificacao.HasValue
            || request.Lida.HasValue
            || request.DataCriacaoInicial.HasValue
            || request.DataCriacaoFinal.HasValue;
    }
}
