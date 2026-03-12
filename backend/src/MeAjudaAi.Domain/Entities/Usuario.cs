using MeAjudaAi.Domain.Common;
using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Domain.Entities;

public class Usuario : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public TipoPerfil TipoPerfil { get; set; }
    public DateTime? DataUltimoLogin { get; set; }

    public Cliente? Cliente { get; set; }
    public Profissional? Profissional { get; set; }
}