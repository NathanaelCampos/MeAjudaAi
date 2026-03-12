namespace MeAjudaAi.Application.DTOs.Profissionais;

public class AtualizarProfissionalRequest
{
    public string NomeExibicao { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string Facebook { get; set; } = string.Empty;
    public string OutraFormaContato { get; set; } = string.Empty;
    public bool AceitaContatoPeloApp { get; set; }
}