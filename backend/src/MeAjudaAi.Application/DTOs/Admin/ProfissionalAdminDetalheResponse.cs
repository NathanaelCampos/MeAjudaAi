namespace MeAjudaAi.Application.DTOs.Admin;

public class ProfissionalAdminDetalheResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string NomeExibicao { get; set; } = string.Empty;
    public string NomeUsuario { get; set; } = string.Empty;
    public string EmailUsuario { get; set; } = string.Empty;
    public string TelefoneUsuario { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string Facebook { get; set; } = string.Empty;
    public string OutraFormaContato { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public bool PerfilVerificado { get; set; }
    public bool AceitaContatoPeloApp { get; set; }
    public DateTime DataCriacao { get; set; }
    public decimal? NotaMediaAtendimento { get; set; }
    public decimal? NotaMediaServico { get; set; }
    public decimal? NotaMediaPreco { get; set; }
}
