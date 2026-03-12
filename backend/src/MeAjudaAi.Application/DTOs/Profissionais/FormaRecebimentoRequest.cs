using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.DTOs.Profissionais;

public class FormaRecebimentoRequest
{
    public TipoFormaRecebimento TipoFormaRecebimento { get; set; }
    public string Descricao { get; set; } = string.Empty;
}