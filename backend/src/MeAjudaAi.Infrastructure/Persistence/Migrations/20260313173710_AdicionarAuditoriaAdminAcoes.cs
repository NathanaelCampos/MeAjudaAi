using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarAuditoriaAdminAcoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auditorias_admin_acoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntidadeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PayloadJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditorias_admin_acoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auditorias_admin_acoes_usuarios_AdminUsuarioId",
                        column: x => x.AdminUsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_admin_acoes_Acao_DataCriacao",
                table: "auditorias_admin_acoes",
                columns: new[] { "Acao", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_admin_acoes_AdminUsuarioId_DataCriacao",
                table: "auditorias_admin_acoes",
                columns: new[] { "AdminUsuarioId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_admin_acoes_Entidade_EntidadeId_DataCriacao",
                table: "auditorias_admin_acoes",
                columns: new[] { "Entidade", "EntidadeId", "DataCriacao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditorias_admin_acoes");
        }
    }
}
