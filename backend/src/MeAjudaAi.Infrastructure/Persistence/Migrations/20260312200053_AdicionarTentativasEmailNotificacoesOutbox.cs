using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeAjudaAi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTentativasEmailNotificacoesOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TentativasProcessamento",
                table: "emails_notificacoes_outbox",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TentativasProcessamento",
                table: "emails_notificacoes_outbox");
        }
    }
}
