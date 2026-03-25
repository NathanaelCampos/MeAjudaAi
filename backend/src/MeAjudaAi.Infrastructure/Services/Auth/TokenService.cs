using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MeAjudaAi.Application.Common;
using MeAjudaAi.Application.DTOs.Auth;
using MeAjudaAi.Application.Interfaces.Auth;
using MeAjudaAi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MeAjudaAi.Infrastructure.Services.Auth;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthResponse GerarToken(Usuario usuario)
    {
        var jwtSection = _configuration.GetSection("Jwt");

        var issuer = jwtSection["Issuer"] ?? "MeAjudaAi.Api";
        var audience = jwtSection["Audience"] ?? "MeAjudaAi.Mobile";
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
        var expiracaoMinutos = int.TryParse(jwtSection["ExpiracaoEmMinutos"], out var minutos) ? minutos : 120;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiraEmUtc = DateTime.UtcNow.AddMinutes(expiracaoMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.UniqueName, usuario.Nome),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, AccessRoles.FromTipoPerfil(usuario.TipoPerfil))
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiraEmUtc,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return new AuthResponse
        {
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Token = token,
            ExpiraEmUtc = expiraEmUtc
        };
    }
}
