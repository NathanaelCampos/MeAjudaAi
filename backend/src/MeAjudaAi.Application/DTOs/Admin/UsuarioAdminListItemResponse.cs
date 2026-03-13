using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Admin;

public class UsuarioAdminListItemResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public TipoPerfil TipoPerfil { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataUltimoLogin { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;
    public bool? PerfilVerificado { get; set; }
}
