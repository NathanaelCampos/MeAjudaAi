using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MeAjudaAi.Api.Configurations;
using MeAjudaAi.Api.Webhooks;
using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.Interfaces.Impulsionamentos;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Api.Controllers;

[ApiController]
[Route("api/webhooks/pagamentos")]
public class WebhooksPagamentosController : ControllerBase
{
    private const string ProvedorPadrao = "padrao";

    private readonly IImpulsionamentoService _impulsionamentoService;
    private readonly IConfiguration _configuration;
    private readonly IValidator<WebhookPagamentoImpulsionamentoRequest> _validator;
    private readonly IReadOnlyDictionary<string, IWebhookPagamentoPayloadAdapter> _payloadAdapters;

    public WebhooksPagamentosController(
        IImpulsionamentoService impulsionamentoService,
        IConfiguration configuration,
        IValidator<WebhookPagamentoImpulsionamentoRequest> validator,
        IEnumerable<IWebhookPagamentoPayloadAdapter> payloadAdapters)
    {
        _impulsionamentoService = impulsionamentoService;
        _configuration = configuration;
        _validator = validator;
        _payloadAdapters = payloadAdapters.ToDictionary(x => x.Provedor, StringComparer.OrdinalIgnoreCase);
    }

    [HttpPost("impulsionamentos")]
    public async Task<IActionResult> ReceberPagamentoImpulsionamento(
        CancellationToken cancellationToken = default)
    {
        return await ReceberPagamentoImpulsionamentoInterno(ProvedorPadrao, cancellationToken);
    }

    [HttpPost("{provedor}/impulsionamentos")]
    public async Task<IActionResult> ReceberPagamentoImpulsionamentoPorProvedor(
        [FromRoute] string provedor,
        CancellationToken cancellationToken = default)
    {
        return await ReceberPagamentoImpulsionamentoInterno(provedor, cancellationToken);
    }

    private async Task<IActionResult> ReceberPagamentoImpulsionamentoInterno(
        string provedor,
        CancellationToken cancellationToken)
    {
        var provedorNormalizado = string.IsNullOrWhiteSpace(provedor) ? ProvedorPadrao : provedor.Trim().ToLowerInvariant();
        var configuracao = ObterConfiguracaoProvedor(provedorNormalizado);
        var assinaturaRecebida = Request.Headers[configuracao.HeaderAssinatura].ToString();

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var corpoBruto = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(configuracao.Segredo) ||
            !AssinaturaValida(corpoBruto, configuracao.Segredo, assinaturaRecebida))
            return Unauthorized(new { mensagem = "Webhook não autorizado." });

        if (!_payloadAdapters.TryGetValue(provedorNormalizado, out var payloadAdapter))
            return BadRequest(new { mensagem = "Provedor de webhook não suportado." });

        var request = payloadAdapter.Parse(corpoBruto) ?? new WebhookPagamentoImpulsionamentoRequest();

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(new
            {
                mensagem = "Payload inválido.",
                erros = validationResult.Errors.Select(x => x.ErrorMessage).ToArray()
            });

        var response = await _impulsionamentoService.ProcessarWebhookPagamentoAsync(
            provedorNormalizado,
            request,
            corpoBruto,
            CapturarHeadersJson(),
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            HttpContext.TraceIdentifier,
            Request.Headers.UserAgent.ToString(),
            cancellationToken);
        return Ok(response);
    }

    private string CapturarHeadersJson()
    {
        var headers = Request.Headers
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.Value.ToArray(),
                StringComparer.OrdinalIgnoreCase);

        return JsonSerializer.Serialize(headers);
    }

    private WebhookPagamentoProviderConfiguration ObterConfiguracaoProvedor(string provedor)
    {
        var provedorNormalizado = string.IsNullOrWhiteSpace(provedor) ? ProvedorPadrao : provedor.Trim().ToLowerInvariant();

        if (provedorNormalizado == ProvedorPadrao)
        {
            return new WebhookPagamentoProviderConfiguration
            {
                Segredo = _configuration["Webhooks:Pagamentos:Segredo"] ?? string.Empty,
                HeaderAssinatura = _configuration["Webhooks:Pagamentos:HeaderAssinatura"] ?? "X-Webhook-Signature"
            };
        }

        var section = _configuration.GetSection($"Webhooks:Pagamentos:Provedores:{provedorNormalizado}");
        return section.Get<WebhookPagamentoProviderConfiguration>() ?? new WebhookPagamentoProviderConfiguration();
    }

    private static bool AssinaturaValida(string corpoBruto, string segredo, string assinaturaRecebida)
    {
        if (string.IsNullOrWhiteSpace(assinaturaRecebida))
            return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(segredo));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(corpoBruto));
        var assinaturaEsperada = Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(assinaturaEsperada),
            Encoding.UTF8.GetBytes(assinaturaRecebida.Trim().ToLowerInvariant()));
    }
}
