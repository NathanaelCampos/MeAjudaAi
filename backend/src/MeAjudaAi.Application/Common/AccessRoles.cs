using MeAjudaAi.Domain.Enums;

namespace MeAjudaAi.Application.Common;

public static class AccessRoles
{
    public const string Administrador = "Administrador";
    public const string Cliente = "Cliente";
    public const string Profissional = "Profissional";

    public static string FromTipoPerfil(TipoPerfil tipoPerfil) => tipoPerfil switch
    {
        TipoPerfil.Administrador => Administrador,
        TipoPerfil.Profissional => Profissional,
        _ => Cliente
    };
}
