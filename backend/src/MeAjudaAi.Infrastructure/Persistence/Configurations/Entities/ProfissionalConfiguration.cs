using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class ProfissionalConfiguration : IEntityTypeConfiguration<Profissional>
{
    public void Configure(EntityTypeBuilder<Profissional> builder)
    {
        builder.ToTable("profissionais");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NomeExibicao)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Descricao)
            .HasMaxLength(2000);

        builder.Property(x => x.WhatsApp)
            .HasMaxLength(20);

        builder.Property(x => x.Instagram)
            .HasMaxLength(100);

        builder.Property(x => x.Facebook)
            .HasMaxLength(100);

        builder.Property(x => x.OutraFormaContato)
            .HasMaxLength(200);

        builder.Property(x => x.AceitaContatoPeloApp)
            .IsRequired();

        builder.Property(x => x.PerfilVerificado)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();
    }
}