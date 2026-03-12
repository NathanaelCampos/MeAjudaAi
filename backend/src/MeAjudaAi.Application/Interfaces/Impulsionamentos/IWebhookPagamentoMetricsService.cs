using MeAjudaAi.Application.DTOs.Impulsionamentos;

namespace MeAjudaAi.Application.Interfaces.Impulsionamentos;

public interface IWebhookPagamentoMetricsService
{
    void RegistrarRecebido(string provedor, string statusRecebido);
    void RegistrarProcessado(string provedor, string statusRecebido);
    void RegistrarDuplicado(string provedor, string statusRecebido);
    void RegistrarRejeitado(string provedor, string statusRecebido);
    void RegistrarErro(string provedor, string statusRecebido);
    WebhookPagamentoMetricasResponse ObterSnapshot();
    void Reset();
}
