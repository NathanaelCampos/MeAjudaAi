namespace MeAjudaAi.Application.DTOs.Admin;

public class CancelarBackgroundJobAdminResponse
{
    public string JobId { get; set; } = string.Empty;
    public int Canceladas { get; set; }
}
