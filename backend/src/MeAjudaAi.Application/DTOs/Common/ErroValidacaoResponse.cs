namespace MeAjudaAi.Application.DTOs.Common;

public class ErroValidacaoResponse
{
    public string Mensagem { get; set; } = string.Empty;
    public IReadOnlyList<CampoErroValidacaoResponse> Erros { get; set; } = Array.Empty<CampoErroValidacaoResponse>();
}
