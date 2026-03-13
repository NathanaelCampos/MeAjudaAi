using FluentValidation;
using MeAjudaAi.Application.DTOs.Notificacoes;

namespace MeAjudaAi.Application.Validators.Notificacoes;

public class MarcarNotificacoesComoLidasEmLoteRequestValidator : AbstractValidator<MarcarNotificacoesComoLidasEmLoteRequest>
{
    public MarcarNotificacoesComoLidasEmLoteRequestValidator()
    {
        RuleFor(x => x.Limite)
            .InclusiveBetween(1, 500);

        RuleFor(x => x)
            .Must(TemAlgumFiltro)
            .WithMessage("Informe pelo menos um filtro para a operação em lote.");
    }

    private static bool TemAlgumFiltro(MarcarNotificacoesComoLidasEmLoteRequest request)
    {
        return request.UsuarioId.HasValue
            || request.TipoNotificacao.HasValue
            || request.DataCriacaoInicial.HasValue
            || request.DataCriacaoFinal.HasValue;
    }
}
