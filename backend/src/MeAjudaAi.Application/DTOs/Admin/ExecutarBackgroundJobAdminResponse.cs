namespace MeAjudaAi.Application.DTOs.Admin;

public class ExecutarBackgroundJobAdminResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int RegistrosProcessados { get; set; }
    public DateTime ExecutadoEm { get; set; }
}
