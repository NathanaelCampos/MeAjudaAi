namespace MeAjudaAi.Application.DTOs.Admin;

public class AuditoriaAdminListItemResponse
{
    public Guid Id { get; set; }
    public Guid AdminUsuarioId { get; set; }
    public string NomeAdmin { get; set; } = string.Empty;
    public string EmailAdmin { get; set; } = string.Empty;
    public string Entidade { get; set; } = string.Empty;
    public Guid EntidadeId { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}
