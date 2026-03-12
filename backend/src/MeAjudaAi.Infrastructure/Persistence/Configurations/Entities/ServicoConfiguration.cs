using MeAjudaAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeAjudaAi.Infrastructure.Persistence.Configurations.Entities;

public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("servicos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Descricao)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.ValorCombinado)
            .HasPrecision(18, 2);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.HasIndex(x => new { x.ClienteId, x.Status });
        builder.HasIndex(x => new { x.ProfissionalId, x.Status });

        builder.HasOne(x => x.Cliente)
            .WithMany(x => x.Servicos)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Profissional)
            .WithMany(x => x.Servicos)
            .HasForeignKey(x => x.ProfissionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Profissao)
            .WithMany()
            .HasForeignKey(x => x.ProfissaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Especialidade)
            .WithMany()
            .HasForeignKey(x => x.EspecialidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cidade)
            .WithMany()
            .HasForeignKey(x => x.CidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Bairro)
            .WithMany()
            .HasForeignKey(x => x.BairroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Avaliacao)
            .WithOne(x => x.Servico)
            .HasForeignKey<Avaliacao>(x => x.ServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}