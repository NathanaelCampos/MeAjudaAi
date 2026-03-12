using FluentValidation;
using MeAjudaAi.Application.DTOs.Servicos;

namespace MeAjudaAi.Application.Validators.Servicos;

public class CriarServicoRequestValidator : AbstractValidator<CriarServicoRequest>
{
    public CriarServicoRequestValidator()
    {
        RuleFor(x => x.ProfissionalId)
            .NotEmpty().WithMessage("Profissional é obrigatório.");

        RuleFor(x => x.CidadeId)
            .NotEmpty().WithMessage("Cidade é obrigatória.");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(2000).WithMessage("Descrição deve ter no máximo 2000 caracteres.");

        RuleFor(x => x.ValorCombinado)
            .GreaterThanOrEqualTo(0).When(x => x.ValorCombinado.HasValue)
            .WithMessage("Valor combinado deve ser zero ou maior.");
    }
}