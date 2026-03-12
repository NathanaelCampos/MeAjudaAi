using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class ProfissionalEspecialidadeConfiguration : IEntityTypeConfiguration<ProfissionalEspecialidade>
{
    public void Configure(EntityTypeBuilder<ProfissionalEspecialidade> builder)
    {
        builder.ToTable("profissionais_especialidades");

        builder.HasKey(x => new { x.ProfissionalId, x.EspecialidadeId });

        builder.HasOne(x => x.Profissional)
            .WithMany(x => x.Especialidades)
            .HasForeignKey(x => x.ProfissionalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Especialidade)
            .WithMany(x => x.Profissionais)
            .HasForeignKey(x => x.EspecialidadeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}