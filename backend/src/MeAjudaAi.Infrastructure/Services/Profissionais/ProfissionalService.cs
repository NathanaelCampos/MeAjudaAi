using MeAjudaAi.Application.DTOs.Profissionais;
using MeAjudaAi.Application.Interfaces.Profissionais;
using MeAjudaAi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using MeAjudaAi.Application.DTOs.Common;
using MeAjudaAi.Application.Interfaces.Storage;

namespace MeAjudaAi.Infrastructure.Services.Profissionais;

public class ProfissionalService : IProfissionalService
{
    private readonly AppDbContext _context;
    private readonly IArquivoStorageService _arquivoStorageService;

    public ProfissionalService(
     AppDbContext context,
     IArquivoStorageService arquivoStorageService)
    {
        _context = context;
        _arquivoStorageService = arquivoStorageService;
    }

    public async Task<IReadOnlyList<ProfissionalResponse>> ListarAsync(
        ListarProfissionaisRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Profissionais
            .AsNoTracking()
            .AsQueryable();

        if (request.SomenteAtivos)
            query = query.Where(x => x.Ativo);

        if (!string.IsNullOrWhiteSpace(request.Nome))
        {
            var nome = request.Nome.Trim().ToLower();
            query = query.Where(x => x.NomeExibicao.ToLower().Contains(nome));
        }

        return await query
            .OrderBy(x => x.NomeExibicao)
            .Select(x => new ProfissionalResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeExibicao = x.NomeExibicao,
                Descricao = x.Descricao,
                WhatsApp = x.WhatsApp,
                Instagram = x.Instagram,
                Facebook = x.Facebook,
                OutraFormaContato = x.OutraFormaContato,
                AceitaContatoPeloApp = x.AceitaContatoPeloApp,
                PerfilVerificado = x.PerfilVerificado,
                NotaMediaAtendimento = x.NotaMediaAtendimento,
                NotaMediaServico = x.NotaMediaServico,
                NotaMediaPreco = x.NotaMediaPreco
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginacaoResponse<ProfissionalResumoResponse>> BuscarAsync(
    BuscarProfissionaisRequest request,
    CancellationToken cancellationToken = default)
    {
        var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
        var tamanhoPagina = request.TamanhoPagina <= 0 ? 10 : request.TamanhoPagina;
        var ordenacao = Enum.IsDefined(typeof(OrdenacaoProfissionais), request.Ordenacao)
            ? request.Ordenacao
            : OrdenacaoProfissionais.Relevancia;

        if (tamanhoPagina > 50)
            tamanhoPagina = 50;

        var query = _context.Profissionais
            .AsNoTracking()
            .AsQueryable();

        if (request.SomenteAtivos)
            query = query.Where(x => x.Ativo);

        if (!string.IsNullOrWhiteSpace(request.Nome))
        {
            var nome = request.Nome.Trim().ToLower();
            query = query.Where(x => x.NomeExibicao.ToLower().Contains(nome));
        }

        if (request.ProfissaoId.HasValue)
        {
            var profissaoId = request.ProfissaoId.Value;
            query = query.Where(x => x.Profissoes.Any(pp => pp.ProfissaoId == profissaoId));
        }

        if (request.EspecialidadeId.HasValue)
        {
            var especialidadeId = request.EspecialidadeId.Value;
            query = query.Where(x => x.Especialidades.Any(pe => pe.EspecialidadeId == especialidadeId));
        }

        if (request.CidadeId.HasValue)
        {
            var cidadeId = request.CidadeId.Value;
            query = query.Where(x => x.AreasAtendimento.Any(a => a.CidadeId == cidadeId));
        }

        if (request.BairroId.HasValue)
        {
            var bairroId = request.BairroId.Value;
            query = query.Where(x => x.AreasAtendimento.Any(a =>
                a.BairroId == bairroId || (a.BairroId == null && a.CidadeInteira)));
        }

        if (request.NotaMinimaServico.HasValue)
        {
            var notaMinimaServico = request.NotaMinimaServico.Value;
            query = query.Where(x => x.NotaMediaServico.HasValue && x.NotaMediaServico.Value >= notaMinimaServico);
        }

        if (request.NotaMinimaAtendimento.HasValue)
        {
            var notaMinimaAtendimento = request.NotaMinimaAtendimento.Value;
            query = query.Where(x => x.NotaMediaAtendimento.HasValue && x.NotaMediaAtendimento.Value >= notaMinimaAtendimento);
        }

        if (request.NotaMinimaPreco.HasValue)
        {
            var notaMinimaPreco = request.NotaMinimaPreco.Value;
            query = query.Where(x => x.NotaMediaPreco.HasValue && x.NotaMediaPreco.Value >= notaMinimaPreco);
        }

        var totalRegistros = await query.CountAsync(cancellationToken);
        var usaOrdenacaoEmMemoria = _context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
        var ordenacaoRequerNota = ordenacao is OrdenacaoProfissionais.NotaServicoDesc
            or OrdenacaoProfissionais.NotaAtendimentoDesc
            or OrdenacaoProfissionais.NotaPrecoDesc
            or OrdenacaoProfissionais.Relevancia;

        if (!(usaOrdenacaoEmMemoria && ordenacaoRequerNota))
        {
            query = ordenacao switch
            {
                OrdenacaoProfissionais.NomeAsc =>
                    query.OrderBy(x => x.NomeExibicao),

                OrdenacaoProfissionais.NomeDesc =>
                    query.OrderByDescending(x => x.NomeExibicao),

                OrdenacaoProfissionais.NotaServicoDesc =>
                    query.OrderByDescending(x => x.Impulsionamentos.Any(i =>
            i.Status == MeAjudaAi.Domain.Enums.StatusImpulsionamento.Ativo &&
            i.DataInicio <= DateTime.UtcNow &&
            i.DataFim > DateTime.UtcNow))
         .ThenByDescending(x => x.NotaMediaServico)
         .ThenByDescending(x => x.PerfilVerificado)
         .ThenBy(x => x.NomeExibicao),

                OrdenacaoProfissionais.NotaAtendimentoDesc =>
                    query.OrderByDescending(x => x.Impulsionamentos.Any(i =>
            i.Status == MeAjudaAi.Domain.Enums.StatusImpulsionamento.Ativo &&
            i.DataInicio <= DateTime.UtcNow &&
            i.DataFim > DateTime.UtcNow))
         .ThenByDescending(x => x.NotaMediaAtendimento)
         .ThenByDescending(x => x.PerfilVerificado)
         .ThenBy(x => x.NomeExibicao),

                OrdenacaoProfissionais.NotaPrecoDesc =>
                    query.OrderByDescending(x => x.Impulsionamentos.Any(i =>
            i.Status == MeAjudaAi.Domain.Enums.StatusImpulsionamento.Ativo &&
            i.DataInicio <= DateTime.UtcNow &&
            i.DataFim > DateTime.UtcNow))
         .ThenByDescending(x => x.NotaMediaPreco)
         .ThenByDescending(x => x.PerfilVerificado)
         .ThenBy(x => x.NomeExibicao),

                _ => query
        .OrderByDescending(x => x.Impulsionamentos.Any(i =>
            i.Status == MeAjudaAi.Domain.Enums.StatusImpulsionamento.Ativo &&
            i.DataInicio <= DateTime.UtcNow &&
            i.DataFim > DateTime.UtcNow))
        .ThenByDescending(x => x.PerfilVerificado)
        .ThenByDescending(x => x.NotaMediaServico)
        .ThenByDescending(x => x.NotaMediaAtendimento)
        .ThenBy(x => x.NomeExibicao)
            };
        }

        var queryProjetada = query.Select(x => new ProfissionalResumoResponse
        {
            Id = x.Id,
            UsuarioId = x.UsuarioId,
            NomeExibicao = x.NomeExibicao,
            Descricao = x.Descricao,
            AceitaContatoPeloApp = x.AceitaContatoPeloApp,
            PerfilVerificado = x.PerfilVerificado,
            EstaImpulsionado = x.Impulsionamentos.Any(i =>
                i.Status == MeAjudaAi.Domain.Enums.StatusImpulsionamento.Ativo &&
                i.DataInicio <= DateTime.UtcNow &&
                i.DataFim > DateTime.UtcNow),
            NotaMediaAtendimento = x.NotaMediaAtendimento,
            NotaMediaServico = x.NotaMediaServico,
            NotaMediaPreco = x.NotaMediaPreco,
            Profissoes = x.Profissoes
                .Select(pp => new ProfissaoResumoResponse
                {
                    Id = pp.Profissao.Id,
                    Nome = pp.Profissao.Nome
                })
                .ToList(),
            Especialidades = x.Especialidades
                .Select(pe => new EspecialidadeResumoResponse
                {
                    Id = pe.Especialidade.Id,
                    Nome = pe.Especialidade.Nome
                })
                .ToList(),
            AreasAtendimento = x.AreasAtendimento
                .Select(a => new AreaAtendimentoResumoResponse
                {
                    CidadeId = a.CidadeId,
                    CidadeNome = a.Cidade.Nome,
                    UF = a.Cidade.Estado.UF,
                    BairroId = a.BairroId,
                    BairroNome = a.Bairro != null ? a.Bairro.Nome : null,
                    CidadeInteira = a.CidadeInteira
                })
                .ToList()
        });

        List<ProfissionalResumoResponse> itens;

        if (usaOrdenacaoEmMemoria && ordenacaoRequerNota)
        {
            var itensOrdenados = await queryProjetada.ToListAsync(cancellationToken);

            itensOrdenados = ordenacao switch
            {
                OrdenacaoProfissionais.NotaServicoDesc => itensOrdenados
                    .OrderByDescending(x => x.EstaImpulsionado)
                    .ThenByDescending(x => x.NotaMediaServico)
                    .ThenByDescending(x => x.PerfilVerificado)
                    .ThenBy(x => x.NomeExibicao)
                    .ToList(),

                OrdenacaoProfissionais.NotaAtendimentoDesc => itensOrdenados
                    .OrderByDescending(x => x.EstaImpulsionado)
                    .ThenByDescending(x => x.NotaMediaAtendimento)
                    .ThenByDescending(x => x.PerfilVerificado)
                    .ThenBy(x => x.NomeExibicao)
                    .ToList(),

                OrdenacaoProfissionais.NotaPrecoDesc => itensOrdenados
                    .OrderByDescending(x => x.EstaImpulsionado)
                    .ThenByDescending(x => x.NotaMediaPreco)
                    .ThenByDescending(x => x.PerfilVerificado)
                    .ThenBy(x => x.NomeExibicao)
                    .ToList(),

                _ => itensOrdenados
                    .OrderByDescending(x => x.EstaImpulsionado)
                    .ThenByDescending(x => x.PerfilVerificado)
                    .ThenByDescending(x => x.NotaMediaServico)
                    .ThenByDescending(x => x.NotaMediaAtendimento)
                    .ThenBy(x => x.NomeExibicao)
                    .ToList()
            };

            itens = itensOrdenados
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToList();
        }
        else
        {
            itens = await queryProjetada
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync(cancellationToken);
        }

        var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina);

        return new PaginacaoResponse<ProfissionalResumoResponse>
        {
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalPaginas,
            Itens = itens
        };
    }

    public async Task<ProfissionalResponse?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Profissionais
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProfissionalResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeExibicao = x.NomeExibicao,
                Descricao = x.Descricao,
                WhatsApp = x.WhatsApp,
                Instagram = x.Instagram,
                Facebook = x.Facebook,
                OutraFormaContato = x.OutraFormaContato,
                AceitaContatoPeloApp = x.AceitaContatoPeloApp,
                PerfilVerificado = x.PerfilVerificado,
                NotaMediaAtendimento = x.NotaMediaAtendimento,
                NotaMediaServico = x.NotaMediaServico,
                NotaMediaPreco = x.NotaMediaPreco
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProfissionalResponse?> AtualizarAsync(
        Guid id,
        AtualizarProfissionalRequest request,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (profissional is null)
            return null;

        profissional.NomeExibicao = request.NomeExibicao.Trim();
        profissional.Descricao = request.Descricao.Trim();
        profissional.WhatsApp = request.WhatsApp.Trim();
        profissional.Instagram = request.Instagram.Trim();
        profissional.Facebook = request.Facebook.Trim();
        profissional.OutraFormaContato = request.OutraFormaContato.Trim();
        profissional.AceitaContatoPeloApp = request.AceitaContatoPeloApp;
        profissional.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ProfissionalResponse
        {
            Id = profissional.Id,
            UsuarioId = profissional.UsuarioId,
            NomeExibicao = profissional.NomeExibicao,
            Descricao = profissional.Descricao,
            WhatsApp = profissional.WhatsApp,
            Instagram = profissional.Instagram,
            Facebook = profissional.Facebook,
            OutraFormaContato = profissional.OutraFormaContato,
            AceitaContatoPeloApp = profissional.AceitaContatoPeloApp,
            PerfilVerificado = profissional.PerfilVerificado,
            NotaMediaAtendimento = profissional.NotaMediaAtendimento,
            NotaMediaServico = profissional.NotaMediaServico,
            NotaMediaPreco = profissional.NotaMediaPreco
        };
    }

    public async Task<ProfissionalResponse?> AtualizarPorUsuarioIdAsync(
        Guid usuarioId,
        AtualizarProfissionalRequest request,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            return null;

        profissional.NomeExibicao = request.NomeExibicao.Trim();
        profissional.Descricao = request.Descricao.Trim();
        profissional.WhatsApp = request.WhatsApp.Trim();
        profissional.Instagram = request.Instagram.Trim();
        profissional.Facebook = request.Facebook.Trim();
        profissional.OutraFormaContato = request.OutraFormaContato.Trim();
        profissional.AceitaContatoPeloApp = request.AceitaContatoPeloApp;
        profissional.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ProfissionalResponse
        {
            Id = profissional.Id,
            UsuarioId = profissional.UsuarioId,
            NomeExibicao = profissional.NomeExibicao,
            Descricao = profissional.Descricao,
            WhatsApp = profissional.WhatsApp,
            Instagram = profissional.Instagram,
            Facebook = profissional.Facebook,
            OutraFormaContato = profissional.OutraFormaContato,
            AceitaContatoPeloApp = profissional.AceitaContatoPeloApp,
            PerfilVerificado = profissional.PerfilVerificado,
            NotaMediaAtendimento = profissional.NotaMediaAtendimento,
            NotaMediaServico = profissional.NotaMediaServico,
            NotaMediaPreco = profissional.NotaMediaPreco
        };
    }

    public async Task AtualizarProfissoesAsync(
        Guid usuarioId,
        AtualizarProfissoesProfissionalRequest request,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Perfil profissional não encontrado.");

        var profissaoIds = request.ProfissaoIds
            .Distinct()
            .ToList();

        var profissoesValidas = await _context.Profissoes
            .Where(x => x.Ativo && profissaoIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var profissoesAtuais = await _context.ProfissionalProfissoes
            .Where(x => x.ProfissionalId == profissional.Id)
            .ToListAsync(cancellationToken);

        _context.ProfissionalProfissoes.RemoveRange(profissoesAtuais);

        foreach (var profissaoId in profissoesValidas)
        {
            _context.ProfissionalProfissoes.Add(new MeAjudaAi.Domain.Entities.ProfissionalProfissao
            {
                ProfissionalId = profissional.Id,
                ProfissaoId = profissaoId
            });
        }

        profissional.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarEspecialidadesAsync(
        Guid usuarioId,
        AtualizarEspecialidadesProfissionalRequest request,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Perfil profissional não encontrado.");

        var especialidadeIds = request.EspecialidadeIds
            .Distinct()
            .ToList();

        var especialidadesValidas = await _context.Especialidades
            .Where(x => x.Ativo && especialidadeIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var especialidadesAtuais = await _context.ProfissionalEspecialidades
            .Where(x => x.ProfissionalId == profissional.Id)
            .ToListAsync(cancellationToken);

        _context.ProfissionalEspecialidades.RemoveRange(especialidadesAtuais);

        foreach (var especialidadeId in especialidadesValidas)
        {
            _context.ProfissionalEspecialidades.Add(new MeAjudaAi.Domain.Entities.ProfissionalEspecialidade
            {
                ProfissionalId = profissional.Id,
                EspecialidadeId = especialidadeId
            });
        }

        profissional.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAreasAtendimentoAsync(
        Guid usuarioId,
        AtualizarAreasAtendimentoRequest request,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Perfil profissional não encontrado.");

        var cidadeIds = request.Areas
            .Select(x => x.CidadeId)
            .Distinct()
            .ToList();

        var bairroIds = request.Areas
            .Where(x => x.BairroId.HasValue)
            .Select(x => x.BairroId!.Value)
            .Distinct()
            .ToList();

        var cidadesValidas = await _context.Cidades
            .Where(x => x.Ativo && cidadeIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var bairrosValidos = await _context.Bairros
            .Where(x => x.Ativo && bairroIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var areasAtuais = await _context.AreasAtendimento
            .Where(x => x.ProfissionalId == profissional.Id)
            .ToListAsync(cancellationToken);

        _context.AreasAtendimento.RemoveRange(areasAtuais);

        foreach (var area in request.Areas)
        {
            if (!cidadesValidas.Contains(area.CidadeId))
                continue;

            if (area.BairroId.HasValue && !bairrosValidos.Contains(area.BairroId.Value))
                continue;

            _context.AreasAtendimento.Add(new MeAjudaAi.Domain.Entities.AreaAtendimento
            {
                ProfissionalId = profissional.Id,
                CidadeId = area.CidadeId,
                BairroId = area.BairroId,
                CidadeInteira = area.CidadeInteira
            });
        }

        profissional.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PortfolioFotoResponse>> ListarPortfolioAsync(
    Guid profissionalId,
    CancellationToken cancellationToken = default)
    {
        return await _context.PortfolioFotos
            .AsNoTracking()
            .Where(x => x.Ativo && x.ProfissionalId == profissionalId)
            .OrderBy(x => x.Ordem)
            .ThenBy(x => x.DataCriacao)
            .Select(x => new PortfolioFotoResponse
            {
                Id = x.Id,
                UrlArquivo = x.UrlArquivo,
                Legenda = x.Legenda,
                Ordem = x.Ordem
            })
            .ToListAsync(cancellationToken);
    }

    public async Task AtualizarPortfolioAsync(
    Guid usuarioId,
    AtualizarPortfolioRequest request,
    CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Perfil profissional não encontrado.");

        var urlsNovas = request.Fotos
            .Where(x => !string.IsNullOrWhiteSpace(x.UrlArquivo))
            .Select(x => x.UrlArquivo.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var fotosAntigas = await _context.PortfolioFotos
            .Where(x => x.ProfissionalId == profissional.Id)
            .ToListAsync(cancellationToken);

        var urlsRemovidas = fotosAntigas
            .Where(x => !urlsNovas.Contains(x.UrlArquivo))
            .Select(x => x.UrlArquivo)
            .ToList();

        _context.PortfolioFotos.RemoveRange(fotosAntigas);

        foreach (var foto in request.Fotos.OrderBy(x => x.Ordem))
        {
            if (string.IsNullOrWhiteSpace(foto.UrlArquivo))
                continue;

            _context.PortfolioFotos.Add(new MeAjudaAi.Domain.Entities.PortfolioFoto
            {
                ProfissionalId = profissional.Id,
                UrlArquivo = foto.UrlArquivo.Trim(),
                Legenda = foto.Legenda?.Trim() ?? string.Empty,
                Ordem = foto.Ordem
            });
        }

        profissional.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var urlRemovida in urlsRemovidas)
        {
            await _arquivoStorageService.ExcluirArquivoAsync(urlRemovida, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<FormaRecebimentoResponse>> ListarFormasRecebimentoAsync(
    Guid profissionalId,
    CancellationToken cancellationToken = default)
    {
        return await _context.FormasRecebimento
            .AsNoTracking()
            .Where(x => x.Ativo && x.ProfissionalId == profissionalId)
            .OrderBy(x => x.TipoFormaRecebimento)
            .Select(x => new FormaRecebimentoResponse
            {
                Id = x.Id,
                TipoFormaRecebimento = x.TipoFormaRecebimento,
                Descricao = x.Descricao
            })
            .ToListAsync(cancellationToken);
    }

    public async Task AtualizarFormasRecebimentoAsync(
        Guid usuarioId,
        AtualizarFormasRecebimentoRequest request,
        CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Perfil profissional não encontrado.");

        var formasAtuais = await _context.FormasRecebimento
            .Where(x => x.ProfissionalId == profissional.Id)
            .ToListAsync(cancellationToken);

        _context.FormasRecebimento.RemoveRange(formasAtuais);

        var itens = request.Itens
            .GroupBy(x => x.TipoFormaRecebimento)
            .Select(x => x.First())
            .ToList();

        foreach (var item in itens)
        {
            _context.FormasRecebimento.Add(new MeAjudaAi.Domain.Entities.FormaRecebimento
            {
                ProfissionalId = profissional.Id,
                TipoFormaRecebimento = item.TipoFormaRecebimento,
                Descricao = item.Descricao?.Trim() ?? string.Empty
            });
        }

        profissional.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<ProfissionalDetalhesResponse?> ObterDetalhesPorIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _context.Profissionais
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProfissionalDetalhesResponse
            {
                Id = x.Id,
                UsuarioId = x.UsuarioId,
                NomeExibicao = x.NomeExibicao,
                Descricao = x.Descricao,
                WhatsApp = x.WhatsApp,
                Instagram = x.Instagram,
                Facebook = x.Facebook,
                OutraFormaContato = x.OutraFormaContato,
                AceitaContatoPeloApp = x.AceitaContatoPeloApp,
                PerfilVerificado = x.PerfilVerificado,
                NotaMediaAtendimento = x.NotaMediaAtendimento,
                NotaMediaServico = x.NotaMediaServico,
                NotaMediaPreco = x.NotaMediaPreco,
                EstaImpulsionado = x.Impulsionamentos.Any(i =>
    i.Status == MeAjudaAi.Domain.Enums.StatusImpulsionamento.Ativo &&
    i.DataInicio <= DateTime.UtcNow &&
    i.DataFim > DateTime.UtcNow),

                Profissoes = x.Profissoes
                    .Select(pp => new ProfissaoResumoResponse
                    {
                        Id = pp.Profissao.Id,
                        Nome = pp.Profissao.Nome
                    })
                    .ToList(),

                Especialidades = x.Especialidades
                    .Select(pe => new EspecialidadeResumoResponse
                    {
                        Id = pe.Especialidade.Id,
                        Nome = pe.Especialidade.Nome
                    })
                    .ToList(),

                AreasAtendimento = x.AreasAtendimento
                    .Select(a => new AreaAtendimentoResumoResponse
                    {
                        CidadeId = a.CidadeId,
                        CidadeNome = a.Cidade.Nome,
                        UF = a.Cidade.Estado.UF,
                        BairroId = a.BairroId,
                        BairroNome = a.Bairro != null ? a.Bairro.Nome : null,
                        CidadeInteira = a.CidadeInteira
                    })
                    .ToList(),

                Portfolio = x.PortfolioFotos
                    .Where(f => f.Ativo)
                    .OrderBy(f => f.Ordem)
                    .Select(f => new PortfolioFotoResponse
                    {
                        Id = f.Id,
                        UrlArquivo = f.UrlArquivo,
                        Legenda = f.Legenda,
                        Ordem = f.Ordem
                    })
                    .ToList(),

                FormasRecebimento = x.FormasRecebimento
                    .Where(fr => fr.Ativo)
                    .OrderBy(fr => fr.TipoFormaRecebimento)
                    .Select(fr => new FormaRecebimentoResponse
                    {
                        Id = fr.Id,
                        TipoFormaRecebimento = fr.TipoFormaRecebimento,
                        Descricao = fr.Descricao
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UploadPortfolioResponse> UploadPortfolioAsync(
    Guid usuarioId,
    Stream stream,
    string nomeArquivoOriginal,
    string contentType,
    long tamanhoArquivo,
    CancellationToken cancellationToken = default)
    {
        var profissional = await _context.Profissionais
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (profissional is null)
            throw new InvalidOperationException("Perfil profissional não encontrado.");

        if (string.IsNullOrWhiteSpace(nomeArquivoOriginal))
            throw new InvalidOperationException("Nome do arquivo é obrigatório.");

        if (tamanhoArquivo <= 0)
            throw new InvalidOperationException("Arquivo é obrigatório.");

        var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extensao = Path.GetExtension(nomeArquivoOriginal).ToLowerInvariant();

        if (!extensoesPermitidas.Contains(extensao))
            throw new InvalidOperationException("Formato de arquivo não permitido. Use jpg, jpeg, png ou webp.");

        var contentTypesPermitidos = new[] { "image/jpeg", "image/png", "image/webp" };
        if (string.IsNullOrWhiteSpace(contentType) || !contentTypesPermitidos.Contains(contentType.ToLowerInvariant()))
            throw new InvalidOperationException("Content-Type do arquivo não permitido.");

        if (tamanhoArquivo > 10_000_000)
            throw new InvalidOperationException("O arquivo deve ter no máximo 10 MB.");

        var (nomeArquivo, caminhoRelativo) = await _arquivoStorageService.SalvarArquivoProfissionalAsync(
            stream,
            nomeArquivoOriginal,
            profissional.Id,
            "portfolio",
            cancellationToken);

        return new UploadPortfolioResponse
        {
            NomeArquivo = nomeArquivo,
            UrlArquivo = caminhoRelativo
        };
    }
}
