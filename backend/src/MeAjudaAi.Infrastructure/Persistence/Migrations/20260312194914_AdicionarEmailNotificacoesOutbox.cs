using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEmailNotificacoesOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AtivoEmail",
                table: "preferencias_notificacoes_usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "emails_notificacoes_outbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoNotificacao = table.Column<int>(type: "integer", nullable: false),
                    EmailDestino = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Assunto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Corpo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReferenciaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMensagemErro = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emails_notificacoes_outbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_emails_notificacoes_outbox_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_emails_notificacoes_outbox_Status_DataCriacao",
                table: "emails_notificacoes_outbox",
                columns: new[] { "Status", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_emails_notificacoes_outbox_UsuarioId",
                table: "emails_notificacoes_outbox",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emails_notificacoes_outbox");

            migrationBuilder.DropColumn(
                name: "AtivoEmail",
                table: "preferencias_notificacoes_usuarios");
        }
    }
}
