using System.Net;
using MeAjudaAi.Application.Interfaces.Notificacoes;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Infrastructure.Services.Notificacoes;

public class EmailNotificacaoTemplateRenderer : IEmailNotificacaoTemplateRenderer
{
    private readonly EmailNotificacaoOptions _options;

    public EmailNotificacaoTemplateRenderer(IOptions<EmailNotificacaoOptions> options)
    {
        _options = options.Value;
    }

    public string RenderizarHtml(EmailNotificacaoOutbox email)
    {
        var assunto = WebUtility.HtmlEncode(email.Assunto);
        var corpo = WebUtility.HtmlEncode(email.Corpo).Replace("\n", "<br/>");
        var appNome = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(_options.RemetenteNome) ? "Me Ajuda AI" : _options.RemetenteNome);
        var tipoLabel = WebUtility.HtmlEncode(ObterLabelTipo(email.TipoNotificacao));
        var referencia = email.ReferenciaId.HasValue
            ? $"<p style=\"margin:12px 0 0;color:#666;font-size:12px;\">Referencia: {WebUtility.HtmlEncode(email.ReferenciaId.Value.ToString())}</p>"
            : string.Empty;

        return $"""
<!DOCTYPE html>
<html lang="pt-BR">
  <body style="margin:0;padding:24px;background:#f5f3ee;font-family:Arial,Helvetica,sans-serif;color:#1f1f1f;">
    <div style="max-width:640px;margin:0 auto;background:#ffffff;border:1px solid #e7e0d4;border-radius:12px;overflow:hidden;">
      <div style="padding:20px 24px;background:#204b57;color:#ffffff;">
        <div style="font-size:12px;letter-spacing:0.08em;text-transform:uppercase;opacity:0.85;">{appNome}</div>
        <h1 style="margin:8px 0 0;font-size:22px;line-height:1.3;">{assunto}</h1>
      </div>
      <div style="padding:24px;">
        <p style="margin:0 0 16px;font-size:13px;color:#7a6d58;text-transform:uppercase;letter-spacing:0.06em;">{tipoLabel}</p>
        <p style="margin:0;font-size:16px;line-height:1.6;">{corpo}</p>
        {referencia}
      </div>
    </div>
  </body>
</html>
""";
    }

    private static string ObterLabelTipo(TipoNotificacao tipoNotificacao)
    {
        return tipoNotificacao switch
        {
            TipoNotificacao.ServicoSolicitado => "Servico solicitado",
            TipoNotificacao.ServicoAceito => "Servico aceito",
            TipoNotificacao.ServicoConcluido => "Servico concluido",
            TipoNotificacao.AvaliacaoAprovada => "Avaliacao aprovada",
            TipoNotificacao.ImpulsionamentoAtivado => "Impulsionamento ativado",
            _ => "Notificacao"
        };
    }
}
