using MeAjudaAi.Application.DTOs.Impulsionamentos;

namespace MeAjudaAi.Api.Webhooks;

public interface IWebhookPagamentoPayloadAdapter
{
    string Provedor { get; }
    WebhookPagamentoImpulsionamentoRequest? Parse(string corpoBruto);
}
