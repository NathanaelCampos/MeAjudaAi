namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class EmailNotificacaoResumoOperacionalDestinatarioFalhaResponse
{
    public Guid UsuarioId { get; set; }
    public string EmailDestino { get; set; } = string.Empty;
    public int QuantidadeFalhas { get; set; }
}
