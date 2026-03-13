using MeAjudaAi.Application.DTOs.Notificacoes;

namespace MeAjudaAi.Application.Interfaces.Notificacoes;

public interface INotificacaoRetentionMetricsService
{
    void RegistrarInicio(DateTime iniciadoEm);
    void RegistrarSucesso(DateTime finalizadoEm, int quantidadeArquivada);
    void RegistrarErro(DateTime finalizadoEm, string mensagemErro);
    RetencaoNotificacoesResumoResponse ObterResumo();
    void Reset();
}
