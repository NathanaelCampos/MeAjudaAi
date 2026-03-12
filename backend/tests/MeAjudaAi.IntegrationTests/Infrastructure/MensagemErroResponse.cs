using System.Text.Json.Serialization;

namespace MeAjudaAi.IntegrationTests.Infrastructure;

public class MensagemErroResponse
{
    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;
}
