using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Admin;

public class ImpulsionamentoAdminDetalheResponse
{
    public Guid Id { get; set; }
    public Guid ProfissionalId { get; set; }
    public Guid PlanoImpulsionamentoId { get; set; }
    public string NomeProfissional { get; set; } = string.Empty;
    public string EmailProfissional { get; set; } = string.Empty;
    public string NomePlano { get; set; } = string.Empty;
    public StatusImpulsionamento Status { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public decimal ValorPago { get; set; }
    public string CodigoReferenciaPagamento { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}
