using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class EspecialidadeConfiguration : IEntityTypeConfiguration<Especialidade>
{
    public void Configure(EntityTypeBuilder<Especialidade> builder)
    {
        builder.ToTable("especialidades");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.ProfissaoId, x.Nome });

        builder.HasOne(x => x.Profissao)
            .WithMany(x => x.Especialidades)
            .HasForeignKey(x => x.ProfissaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}