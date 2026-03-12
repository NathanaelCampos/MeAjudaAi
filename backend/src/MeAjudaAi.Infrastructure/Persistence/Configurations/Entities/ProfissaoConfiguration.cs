using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class ProfissaoConfiguration : IEntityTypeConfiguration<Profissao>
{
    public void Configure(EntityTypeBuilder<Profissao> builder)
    {
        builder.ToTable("profissoes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => x.Nome);
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}