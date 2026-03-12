using MeAjudaAi.Application.DTOs.Impulsionamentos;
using MeAjudaAi.Application.Validators.Impulsionamentos;

namespace MeAjudaAi.UnitTests.Impulsionamentos;

public class ContratarPlanoImpulsionamentoRequestValidatorTests
{
    [Fact]
    public void Validate_DeveRetornarErroQuandoPlanoImpulsionamentoIdForVazio()
    {
        var validator = new ContratarPlanoImpulsionamentoRequestValidator();

        var result = validator.Validate(new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = Guid.Empty
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ContratarPlanoImpulsionamentoRequest.PlanoImpulsionamentoId));
    }

    [Fact]
    public void Validate_DeveRetornarErroQuandoCodigoReferenciaPagamentoExcederLimite()
    {
        var validator = new ContratarPlanoImpulsionamentoRequestValidator();

        var result = validator.Validate(new ContratarPlanoImpulsionamentoRequest
        {
            PlanoImpulsionamentoId = Guid.NewGuid(),
            CodigoReferenciaPagamento = new string('a', 151)
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ContratarPlanoImpulsionamentoRequest.CodigoReferenciaPagamento));
    }

    [Fact]
    public void ConfirmarPagamentoValidator_DeveRetornarErroQuandoCodigoNaoForInformado()
    {
        var validator = new ConfirmarPagamentoImpulsionamentoRequestValidator();

        var result = validator.Validate(new ConfirmarPagamentoImpulsionamentoRequest
        {
            CodigoReferenciaPagamento = string.Empty
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(ConfirmarPagamentoImpulsionamentoRequest.CodigoReferenciaPagamento));
    }

    [Fact]
    public void WebhookPagamentoValidator_DeveRetornarErroQuandoStatusForInvalido()
    {
        var validator = new WebhookPagamentoImpulsionamentoRequestValidator();

        var result = validator.Validate(new WebhookPagamentoImpulsionamentoRequest
        {
            CodigoReferenciaPagamento = "pag-001",
            StatusPagamento = "desconhecido"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(WebhookPagamentoImpulsionamentoRequest.StatusPagamento));
    }
}
