using System.Text.Json;
using MeAjudaAi.Application.DTOs.Impulsionamentos;

namespace MeAjudaAi.Api.Webhooks;

public class DefaultWebhookPagamentoPayloadAdapter : IWebhookPagamentoPayloadAdapter
{
    public string Provedor => "padrao";

    public WebhookPagamentoImpulsionamentoRequest? Parse(string corpoBruto)
    {
        return JsonSerializer.Deserialize<WebhookPagamentoImpulsionamentoRequest>(
            corpoBruto,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
