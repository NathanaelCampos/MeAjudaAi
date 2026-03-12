namespace MeAjudaAi.Application.DTOs.Common;

public class CampoErroValidacaoResponse
{
    public string Campo { get; set; } = string.Empty;
    public IReadOnlyList<string> Mensagens { get; set; } = Array.Empty<string>();
}
