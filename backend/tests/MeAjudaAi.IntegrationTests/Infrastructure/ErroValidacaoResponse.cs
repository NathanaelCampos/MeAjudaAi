using System.Text.Json.Serialization;

namespace MeAjudaAi.IntegrationTests.Infrastructure;

public class ErroValidacaoResponse
{
    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;

    [JsonPropertyName("erros")]
    public List<CampoValidacaoResponse> Erros { get; set; } = [];
}

public class CampoValidacaoResponse
{
    [JsonPropertyName("Campo")]
    public string Campo { get; set; } = string.Empty;

    [JsonPropertyName("Mensagens")]
    public List<string> Mensagens { get; set; } = [];
}
