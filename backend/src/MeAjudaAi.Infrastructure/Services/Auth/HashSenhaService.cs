using MeAjudaAi.Application.Interfaces.Auth;
using MeAjudaAi.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeAjudaAi.Infrastructure.Services.Auth;

public class HashSenhaService : IHashSenhaService
{
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public string GerarHash(Usuario usuario, string senha)
    {
        return _passwordHasher.HashPassword(usuario, senha);
    }

    public bool VerificarSenha(Usuario usuario, string senhaInformada, string senhaHash)
    {
        var resultado = _passwordHasher.VerifyHashedPassword(usuario, senhaHash, senhaInformada);

        return resultado == PasswordVerificationResult.Success ||
               resultado == PasswordVerificationResult.SuccessRehashNeeded;
    }
}