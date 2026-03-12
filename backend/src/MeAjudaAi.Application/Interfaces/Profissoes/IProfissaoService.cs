using MeAjudaAi.Application.DTOs.Profissoes;
using MeAjudaAi.Application.DTOs.Profissionais;

namespace MeAjudaAi.Application.Interfaces.Profissoes;

public interface IProfissaoService
{
    Task<IReadOnlyList<ProfissaoResponse>> ListarAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EspecialidadeResponse>> ListarEspecialidadesPorProfissaoAsync(
        Guid profissaoId,
        CancellationToken cancellationToken = default);
}