using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Profissionais;

public class FormaRecebimentoResponse
{
    public Guid Id { get; set; }
    public TipoFormaRecebimento TipoFormaRecebimento { get; set; }
    public string Descricao { get; set; } = string.Empty;
}