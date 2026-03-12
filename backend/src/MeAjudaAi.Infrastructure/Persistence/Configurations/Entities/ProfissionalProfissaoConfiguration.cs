using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class ProfissionalProfissaoConfiguration : IEntityTypeConfiguration<ProfissionalProfissao>
{
    public void Configure(EntityTypeBuilder<ProfissionalProfissao> builder)
    {
        builder.ToTable("profissionais_profissoes");

        builder.HasKey(x => new { x.ProfissionalId, x.ProfissaoId });

        builder.HasOne(x => x.Profissional)
            .WithMany(x => x.Profissoes)
            .HasForeignKey(x => x.ProfissionalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Profissao)
            .WithMany(x => x.Profissionais)
            .HasForeignKey(x => x.ProfissaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}