using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Servicos;

public class ListarServicosRequest
{
    public StatusServico? Status { get; set; }
}