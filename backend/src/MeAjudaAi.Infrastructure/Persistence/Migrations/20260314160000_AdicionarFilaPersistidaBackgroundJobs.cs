using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    public partial class AdicionarFilaPersistidaBackgroundJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "background_jobs_execucoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NomeJob = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Origem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SolicitadoPorAdminUsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TentativasProcessamento = table.Column<int>(type: "integer", nullable: false),
                    RegistrosProcessados = table.Column<int>(type: "integer", nullable: false),
                    ProcessarAposUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataInicioProcessamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataFinalizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MensagemResultado = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_background_jobs_execucoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_background_jobs_execucoes_usuarios_SolicitadoPorAdminUsuarioId",
                        column: x => x.SolicitadoPorAdminUsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_background_jobs_execucoes_JobId_DataCriacao",
                table: "background_jobs_execucoes",
                columns: new[] { "JobId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_background_jobs_execucoes_SolicitadoPorAdminUsuarioId",
                table: "background_jobs_execucoes",
                column: "SolicitadoPorAdminUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_background_jobs_execucoes_Status_ProcessarAposUtc",
                table: "background_jobs_execucoes",
                columns: new[] { "Status", "ProcessarAposUtc" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "background_jobs_execucoes");
        }
    }
}
