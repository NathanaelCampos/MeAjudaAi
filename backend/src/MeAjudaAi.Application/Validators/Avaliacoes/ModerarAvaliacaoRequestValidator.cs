using FluentValidation;
using MeAjudaAi.Application.DTOs.Avaliacoes;

namespace MeAjudaAi.Application.Validators.Avaliacoes;

public class ModerarAvaliacaoRequestValidator : AbstractValidator<ModerarAvaliacaoRequest>
{
    public ModerarAvaliacaoRequestValidator()
    {
        RuleFor(x => x.Acao)
            .IsInEnum()
            .WithMessage("A ação de moderação é inválida.");
    }
}