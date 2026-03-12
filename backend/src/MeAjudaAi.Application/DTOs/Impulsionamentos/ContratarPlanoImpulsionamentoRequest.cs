namespace MeAjudaAi.Application.DTOs.Impulsionamentos;

public class ContratarPlanoImpulsionamentoRequest
{
    public Guid PlanoImpulsionamentoId { get; set; }
    public string CodigoReferenciaPagamento { get; set; } = string.Empty;
}