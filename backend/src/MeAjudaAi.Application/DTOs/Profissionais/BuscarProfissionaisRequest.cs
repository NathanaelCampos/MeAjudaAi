namespace MeAjudaAi.Application.DTOs.Profissionais;

public class BuscarProfissionaisRequest
{
    public string? Nome { get; set; }
    public Guid? ProfissaoId { get; set; }
    public Guid? EspecialidadeId { get; set; }
    public Guid? CidadeId { get; set; }
    public Guid? BairroId { get; set; }
    public bool SomenteAtivos { get; set; } = true;

    public decimal? NotaMinimaServico { get; set; }
    public decimal? NotaMinimaAtendimento { get; set; }
    public decimal? NotaMinimaPreco { get; set; }

    public OrdenacaoProfissionais Ordenacao { get; set; } = OrdenacaoProfissionais.Relevancia;

    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
}