using MeAjudaAi.Domain.Common;

namespace MeAjudaAi.Domain.Entities;

public class PortfolioFoto : EntityBase
{
    public Guid ProfissionalId { get; set; }
    public string UrlArquivo { get; set; } = string.Empty;
    public string Legenda { get; set; } = string.Empty;
    public int Ordem { get; set; }

    public Profissional Profissional { get; set; } = null!;
}