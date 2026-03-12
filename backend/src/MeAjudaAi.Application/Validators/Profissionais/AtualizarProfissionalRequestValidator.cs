using FluentValidation;
using MeAjudaAi.Application.DTOs.Profissionais;

namespace MeAjudaAi.Application.Validators.Profissionais;

public class AtualizarProfissionalRequestValidator : AbstractValidator<AtualizarProfissionalRequest>
{
    public AtualizarProfissionalRequestValidator()
    {
        RuleFor(x => x.NomeExibicao)
            .NotEmpty().WithMessage("Nome de exibição é obrigatório.")
            .MaximumLength(150).WithMessage("Nome de exibição deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(2000).WithMessage("Descrição deve ter no máximo 2000 caracteres.");

        RuleFor(x => x.WhatsApp)
            .MaximumLength(20).WithMessage("WhatsApp deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Instagram)
            .MaximumLength(100).WithMessage("Instagram deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Facebook)
            .MaximumLength(100).WithMessage("Facebook deve ter no máximo 100 caracteres.");

        RuleFor(x => x.OutraFormaContato)
            .MaximumLength(200).WithMessage("Outra forma de contato deve ter no máximo 200 caracteres.");
    }
}