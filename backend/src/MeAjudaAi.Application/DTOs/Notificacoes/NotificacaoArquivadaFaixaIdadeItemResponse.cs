namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class NotificacaoArquivadaFaixaIdadeItemResponse
{
    public string Faixa { get; set; } = string.Empty;
    public int DiasIniciais { get; set; }
    public int? DiasFinais { get; set; }
    public int Quantidade { get; set; }
}
