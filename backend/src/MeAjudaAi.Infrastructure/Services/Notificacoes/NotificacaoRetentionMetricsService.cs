using MeAjudaAi.Application.DTOs.Notificacoes;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Infrastructure.Configurations;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class NotificacaoRetentionMetricsService : INotificacaoRetentionMetricsService
{
    private readonly object _lock = new();
    private DateTime? _ultimaExecucaoIniciadaEm;
    private DateTime? _ultimaExecucaoFinalizadaEm;
    private int? _ultimaQuantidadeArquivada;
    private long _totalArquivado;
    private string _ultimoStatus = "nao_executado";
    private string? _ultimaMensagemErro;

    public void RegistrarInicio(DateTime iniciadoEm)
    {
        lock (_lock)
        {
            _ultimaExecucaoIniciadaEm = iniciadoEm;
            _ultimoStatus = "em_execucao";
            _ultimaMensagemErro = null;
        }
    }

    public void RegistrarSucesso(DateTime finalizadoEm, int quantidadeArquivada)
    {
        lock (_lock)
        {
            _ultimaExecucaoFinalizadaEm = finalizadoEm;
            _ultimaQuantidadeArquivada = quantidadeArquivada;
            _totalArquivado += quantidadeArquivada;
            _ultimoStatus = "sucesso";
            _ultimaMensagemErro = null;
        }
    }

    public void RegistrarErro(DateTime finalizadoEm, string mensagemErro)
    {
        lock (_lock)
        {
            _ultimaExecucaoFinalizadaEm = finalizadoEm;
            _ultimoStatus = "erro";
            _ultimaMensagemErro = mensagemErro;
        }
    }

    public RetencaoNotificacoesResumoResponse ObterResumo()
    {
        lock (_lock)
        {
            return new RetencaoNotificacoesResumoResponse
            {
                UltimaExecucaoIniciadaEm = _ultimaExecucaoIniciadaEm,
                UltimaExecucaoFinalizadaEm = _ultimaExecucaoFinalizadaEm,
                UltimaQuantidadeArquivada = _ultimaQuantidadeArquivada,
                TotalArquivado = _totalArquivado,
                UltimoStatus = _ultimoStatus,
                UltimaMensagemErro = _ultimaMensagemErro
            };
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _ultimaExecucaoIniciadaEm = null;
            _ultimaExecucaoFinalizadaEm = null;
            _ultimaQuantidadeArquivada = null;
            _totalArquivado = 0;
            _ultimoStatus = "nao_executado";
            _ultimaMensagemErro = null;
        }
    }
}
