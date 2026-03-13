namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardAvaliacaoPendenteItemResponse
{
    public Guid Id { get; set; }
    public Guid ServicoId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string NomeProfissional { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}
