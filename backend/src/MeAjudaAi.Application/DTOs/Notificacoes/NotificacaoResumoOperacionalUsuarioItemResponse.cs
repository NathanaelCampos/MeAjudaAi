namespace MeAjudaAi.Application.DTOs.Notificacoes;

public class NotificacaoResumoOperacionalUsuarioItemResponse
{
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Lidas { get; set; }
    public int NaoLidas { get; set; }
}
