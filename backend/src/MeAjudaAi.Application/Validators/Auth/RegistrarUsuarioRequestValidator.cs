using FluentValidation;
using MeAjudaAi.Application.DTOs.Auth;

namespace MeAjudaAi.Application.Validators.Auth;

public class RegistrarUsuarioRequestValidator : AbstractValidator<RegistrarUsuarioRequest>
{
    public RegistrarUsuarioRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(200).WithMessage("E-mail deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Telefone)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres.")
            .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres.");
    }
}