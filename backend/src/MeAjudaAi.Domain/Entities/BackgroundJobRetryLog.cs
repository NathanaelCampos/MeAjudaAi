namespace MeAjudaAi.Domain.Entities;

public class BackgroundJobRetryLog
{
    public Guid Id { get; set; }
    public Guid BackgroundJobExecucaoId { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public bool Ativo { get; set; } = true;
}
