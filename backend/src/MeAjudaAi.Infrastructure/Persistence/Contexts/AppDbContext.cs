using MeAjudaAi.Domain.Entities;
using MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Infrastructure.Persistence.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Profissional> Profissionais => Set<Profissional>();
    public DbSet<Profissao> Profissoes => Set<Profissao>();
    public DbSet<Especialidade> Especialidades => Set<Especialidade>();
    public DbSet<Cidade> Cidades => Set<Cidade>();
    public DbSet<Bairro> Bairros => Set<Bairro>();
    public DbSet<AreaAtendimento> AreasAtendimento => Set<AreaAtendimento>();
    public DbSet<ProfissionalProfissao> ProfissionalProfissoes => Set<ProfissionalProfissao>();
    public DbSet<ProfissionalEspecialidade> ProfissionalEspecialidades => Set<ProfissionalEspecialidade>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
    public DbSet<PortfolioFoto> PortfolioFotos => Set<PortfolioFoto>();
    public DbSet<FormaRecebimento> FormasRecebimento => Set<FormaRecebimento>();
    public DbSet<PlanoImpulsionamento> PlanosImpulsionamento => Set<PlanoImpulsionamento>();
    public DbSet<ImpulsionamentoProfissional> ImpulsionamentosProfissionais => Set<ImpulsionamentoProfissional>();
    public DbSet<WebhookPagamentoImpulsionamentoEvento> WebhookPagamentoImpulsionamentoEventos => Set<WebhookPagamentoImpulsionamentoEvento>();
    public DbSet<AuditoriaAdminAcao> AuditoriasAdminAcoes => Set<AuditoriaAdminAcao>();
    public DbSet<BackgroundJobExecucao> BackgroundJobsExecucoes => Set<BackgroundJobExecucao>();
    public DbSet<BackgroundJobFilaAlertaHistorico> BackgroundJobFilaAlertasHistorico => Set<BackgroundJobFilaAlertaHistorico>();
    public DbSet<NotificacaoUsuario> NotificacoesUsuarios => Set<NotificacaoUsuario>();
    public DbSet<PreferenciaNotificacaoUsuario> PreferenciasNotificacoesUsuarios => Set<PreferenciaNotificacaoUsuario>();
    public DbSet<EmailNotificacaoOutbox> EmailsNotificacoesOutbox => Set<EmailNotificacaoOutbox>();
    public DbSet<Estado> Estados => Set<Estado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
