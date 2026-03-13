using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardEmailFalhoItemResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string EmailDestino { get; set; } = string.Empty;
    public TipoNotificacao TipoNotificacao { get; set; }
    public StatusEmailNotificacao Status { get; set; }
    public string UltimaMensagemErro { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}
