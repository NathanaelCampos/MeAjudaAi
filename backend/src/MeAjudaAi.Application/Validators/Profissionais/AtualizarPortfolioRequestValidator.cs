using FluentValidation;
using MeAjudaAi.Application.DTOs.Profissionais;

namespace MeAjudaAi.Application.Validators.Profissionais;

public class AtualizarPortfolioRequestValidator : AbstractValidator<AtualizarPortfolioRequest>
{
    public AtualizarPortfolioRequestValidator()
    {
        RuleFor(x => x.Fotos)
            .NotNull().WithMessage("A lista de fotos é obrigatória.");

        RuleForEach(x => x.Fotos).ChildRules(foto =>
        {
            foto.RuleFor(f => f.UrlArquivo)
                .NotEmpty().WithMessage("URL da foto é obrigatória.")
                .MaximumLength(500).WithMessage("URL da foto deve ter no máximo 500 caracteres.");

            foto.RuleFor(f => f.Legenda)
                .MaximumLength(300).WithMessage("Legenda deve ter no máximo 300 caracteres.");

            foto.RuleFor(f => f.Ordem)
                .GreaterThanOrEqualTo(0).WithMessage("Ordem deve ser zero ou maior.");
        });
    }
}