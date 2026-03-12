namespace MeAjudaAi.Infrastructure.Configurations;

public class EmailNotificacaoOptions
{
    public bool ProcessadorHabilitado { get; set; }
    public int LoteProcessamento { get; set; } = 20;
    public int IntervaloSegundos { get; set; } = 60;
    public int AtrasoBaseSegundos { get; set; } = 60;
    public int MaxTentativas { get; set; } = 3;
    public bool SimularEnvio { get; set; } = true;
    public string RemetenteEmail { get; set; } = string.Empty;
    public string RemetenteNome { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 25;
    public string SmtpUsuario { get; set; } = string.Empty;
    public string SmtpSenha { get; set; } = string.Empty;
    public bool SmtpSsl { get; set; } = true;
}
