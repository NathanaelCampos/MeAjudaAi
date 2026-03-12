using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Configurations;
using MeAjudaAi.Infrastructure.Services.Notificacoes;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.UnitTests.Notificacoes;

public class EmailNotificacaoTemplateRendererTests
{
    [Fact]
    public void RenderizarHtml_DeveMontarTemplateComTipoEReferencia()
    {
        var renderer = new EmailNotificacaoTemplateRenderer(Options.Create(new EmailNotificacaoOptions
        {
            RemetenteNome = "Me Ajuda AI"
        }));

        var referenciaId = Guid.NewGuid();
        var html = renderer.RenderizarHtml(new EmailNotificacaoOutbox
        {
            TipoNotificacao = TipoNotificacao.ServicoConcluido,
            Assunto = "Servico finalizado",
            Corpo = "Seu atendimento foi concluido com sucesso.",
            ReferenciaId = referenciaId
        });

        Assert.Contains("Servico concluido", html);
        Assert.Contains("Servico finalizado", html);
        Assert.Contains("Seu atendimento foi concluido com sucesso.", html);
        Assert.Contains(referenciaId.ToString(), html);
    }
}
