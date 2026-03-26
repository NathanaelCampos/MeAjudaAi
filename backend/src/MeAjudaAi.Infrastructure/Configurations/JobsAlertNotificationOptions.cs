namespace MeAjudaAi.Infrastructure.Configurations;

public class JobsAlertNotificationOptions
{
    public bool Habilitado { get; set; } = true;
    public int IntervaloSegundos { get; set; } = 60;
    public int IntervaloMinutosEntreNotificacoes { get; set; } = 30;
    public string[] NiveisParaNotificar { get; set; } = new[]
    {
        "Falhas",
        "Fila longa",
        "Processamento lento"
    };
}
