namespace MeAjudaAi.Application.DTOs.Admin;

public class EnfileirarBackgroundJobAdminResponse
{
    public Guid ExecucaoId { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string NomeJob { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EnfileiradoEm { get; set; }
}
