using MeAjudaAi.Domain.Entities;

namespace MeAjudaAi.Application.Interfaces.Auth;

public interface IHashSenhaService
{
    string GerarHash(Usuario usuario, string senha);
    bool VerificarSenha(Usuario usuario, string senhaInformada, string senhaHash);
}