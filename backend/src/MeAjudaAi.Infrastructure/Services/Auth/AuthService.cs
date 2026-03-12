using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.Interfaces.Auth;
using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Domain.Enums;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IHashSenhaService _hashSenhaService;
    private readonly ITokenService _tokenService;

    public AuthService(
        AppDbContext context,
        IHashSenhaService hashSenhaService,
        ITokenService tokenService)
    {
        _context = context;
        _hashSenhaService = hashSenhaService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegistrarAsync(
    RegistrarUsuarioRequest request,
    CancellationToken cancellationToken = default)
    {
        if (request.TipoPerfil == MeAjudaAi.Domain.Enums.TipoPerfil.Administrador)
            throw new InvalidOperationException("Não é permitido criar administrador por este endpoint.");

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        var usuarioExistente = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == emailNormalizado, cancellationToken);

        if (usuarioExistente is not null)
            throw new InvalidOperationException("Já existe um usuário com este e-mail.");

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = emailNormalizado,
            Telefone = request.Telefone?.Trim() ?? string.Empty,
            TipoPerfil = request.TipoPerfil
        };

        usuario.SenhaHash = _hashSenhaService.GerarHash(usuario, request.Senha);

        if (request.TipoPerfil == TipoPerfil.Cliente)
        {
            usuario.Cliente = new Cliente
            {
                NomeExibicao = usuario.Nome
            };
        }
        else if (request.TipoPerfil == TipoPerfil.Profissional)
        {
            usuario.Profissional = new Profissional
            {
                NomeExibicao = usuario.Nome,
                AceitaContatoPeloApp = true
            };
        }

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(cancellationToken);

        return _tokenService.GerarToken(usuario);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Email == emailNormalizado, cancellationToken);

        if (usuario is null)
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        var senhaValida = _hashSenhaService.VerificarSenha(usuario, request.Senha, usuario.SenhaHash);

        if (!senhaValida)
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        usuario.DataUltimoLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return _tokenService.GerarToken(usuario);
    }
}