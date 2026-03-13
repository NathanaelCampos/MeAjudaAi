namespace MeAjudaAi.Application.DTOs.Admin;

public class AdminDashboardUsuariosResponse
{
    public int Total { get; set; }
    public int Ativos { get; set; }
    public int Inativos { get; set; }
    public int Clientes { get; set; }
    public int Profissionais { get; set; }
    public int Administradores { get; set; }
}
