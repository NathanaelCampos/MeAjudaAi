using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    public partial class AdicionarRestricaoSobreposicaoImpulsionamento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE EXTENSION IF NOT EXISTS btree_gist;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE impulsionamentos_profissionais
                ADD CONSTRAINT ck_impulsionamentos_profissionais_periodo_valido
                CHECK ("DataInicio" < "DataFim");
                """);

            migrationBuilder.Sql("""
                ALTER TABLE impulsionamentos_profissionais
                ADD CONSTRAINT ex_impulsionamentos_profissionais_sem_sobreposicao
                EXCLUDE USING GIST
                (
                    "ProfissionalId" WITH =,
                    tstzrange("DataInicio", "DataFim", '[)') WITH &&
                )
                WHERE ("Status" IN (1, 2));
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE impulsionamentos_profissionais
                DROP CONSTRAINT IF EXISTS ex_impulsionamentos_profissionais_sem_sobreposicao;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE impulsionamentos_profissionais
                DROP CONSTRAINT IF EXISTS ck_impulsionamentos_profissionais_periodo_valido;
                """);
        }
    }
}
