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
}
