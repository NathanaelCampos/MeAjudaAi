namespace MeAjudaAi.Api.Configurations;

public class WebhookPagamentoProviderConfiguration
{
    public string Segredo { get; set; } = string.Empty;
    public string HeaderAssinatura { get; set; } = "X-Webhook-Signature";
}
