using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.Interfaces.Impulsionamentos;

namespace MeAjudaAi.Infrastructure.Services.Impulsionamentos;

public class WebhookPagamentoMetricsService : IWebhookPagamentoMetricsService
{
    private static readonly Meter Meter = new("MeAjudaAi.Webhooks.Pagamentos", "1.0.0");

    private readonly Counter<long> _recebidosCounter = Meter.CreateCounter<long>("webhook_pagamentos_recebidos_total");
    private readonly Counter<long> _processadosCounter = Meter.CreateCounter<long>("webhook_pagamentos_processados_total");
    private readonly Counter<long> _duplicadosCounter = Meter.CreateCounter<long>("webhook_pagamentos_duplicados_total");
    private readonly Counter<long> _rejeitadosCounter = Meter.CreateCounter<long>("webhook_pagamentos_rejeitados_total");
    private readonly Counter<long> _errosCounter = Meter.CreateCounter<long>("webhook_pagamentos_erros_total");

    private readonly ConcurrentDictionary<string, long> _valores = new(StringComparer.Ordinal);

    public void RegistrarRecebido(string provedor, string statusRecebido)
    {
        Registrar(_recebidosCounter, "recebido", provedor, statusRecebido);
    }

    public void RegistrarProcessado(string provedor, string statusRecebido)
    {
        Registrar(_processadosCounter, "processado", provedor, statusRecebido);
    }

    public void RegistrarDuplicado(string provedor, string statusRecebido)
    {
        Registrar(_duplicadosCounter, "duplicado", provedor, statusRecebido);
    }

    public void RegistrarRejeitado(string provedor, string statusRecebido)
    {
        Registrar(_rejeitadosCounter, "rejeitado", provedor, statusRecebido);
    }

    public void RegistrarErro(string provedor, string statusRecebido)
    {
        Registrar(_errosCounter, "erro", provedor, statusRecebido);
    }

    public WebhookPagamentoMetricasResponse ObterSnapshot()
    {
        var itens = _valores
            .Select(x =>
            {
                var partes = x.Key.Split('|');

                return new WebhookPagamentoMetricaItemResponse
                {
                    Resultado = partes[0],
                    Provedor = partes[1],
                    StatusRecebido = partes[2],
                    Quantidade = x.Value
                };
            })
            .OrderBy(x => x.Resultado)
            .ThenBy(x => x.Provedor)
            .ThenBy(x => x.StatusRecebido)
            .ToList();

        return new WebhookPagamentoMetricasResponse
        {
            Itens = itens
        };
    }

    public void Reset()
    {
        _valores.Clear();
    }

    private void Registrar(Counter<long> counter, string resultado, string provedor, string statusRecebido)
    {
        var provedorNormalizado = string.IsNullOrWhiteSpace(provedor) ? "padrao" : provedor.Trim().ToLowerInvariant();
        var statusNormalizado = string.IsNullOrWhiteSpace(statusRecebido) ? "desconhecido" : statusRecebido.Trim().ToLowerInvariant();

        counter.Add(1,
            new KeyValuePair<string, object?>("provedor", provedorNormalizado),
            new KeyValuePair<string, object?>("resultado", resultado),
            new KeyValuePair<string, object?>("status_recebido", statusNormalizado));

        _valores.AddOrUpdate(
            $"{resultado}|{provedorNormalizado}|{statusNormalizado}",
            1,
            (_, valorAtual) => valorAtual + 1);
    }
}
