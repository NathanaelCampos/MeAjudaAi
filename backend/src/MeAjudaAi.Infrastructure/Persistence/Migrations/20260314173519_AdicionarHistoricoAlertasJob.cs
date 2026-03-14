using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarHistoricoAlertasJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "background_job_fila_alerta_historico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NivelAlerta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Mensagem = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Cor = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    TempoMedioFilaSegundos = table.Column<double>(type: "double precision", nullable: false),
                    TempoMedioProcessamentoSegundos = table.Column<double>(type: "double precision", nullable: false),
                    TotalPendentes = table.Column<int>(type: "integer", nullable: false),
                    TotalFalhas = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_background_job_fila_alerta_historico", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "background_job_fila_alerta_historico");
        }
    }
}
