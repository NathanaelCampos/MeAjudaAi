using FluentValidation;
using MeAjudaAi.Application.DTOs.Avaliacoes;

namespace MeAjudaAi.Application.Validators.Avaliacoes;

public class CriarAvaliacaoRequestValidator : AbstractValidator<CriarAvaliacaoRequest>
{
    public CriarAvaliacaoRequestValidator()
    {
        RuleFor(x => x.ServicoId)
            .NotEmpty().WithMessage("Serviço é obrigatório.");

        RuleFor(x => x.Comentario)
            .MaximumLength(1000).WithMessage("Comentário deve ter no máximo 1000 caracteres.");
    }
}