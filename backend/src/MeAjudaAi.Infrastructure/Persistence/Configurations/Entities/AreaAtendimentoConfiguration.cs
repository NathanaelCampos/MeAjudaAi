using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class AreaAtendimentoConfiguration : IEntityTypeConfiguration<AreaAtendimento>
{
    public void Configure(EntityTypeBuilder<AreaAtendimento> builder)
    {
        builder.ToTable("areas_atendimento");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CidadeInteira)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.ProfissionalId, x.CidadeId, x.BairroId });

        builder.HasOne(x => x.Profissional)
            .WithMany(x => x.AreasAtendimento)
            .HasForeignKey(x => x.ProfissionalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Cidade)
            .WithMany(x => x.AreasAtendimento)
            .HasForeignKey(x => x.CidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Bairro)
            .WithMany(x => x.AreasAtendimento)
            .HasForeignKey(x => x.BairroId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}