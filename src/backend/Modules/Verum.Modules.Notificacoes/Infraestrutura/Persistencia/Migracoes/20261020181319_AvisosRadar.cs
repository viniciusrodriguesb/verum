using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class AvisosRadar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FUSO",
                schema: "notificacoes",
                table: "PREFERENCIA_NOTIFICACAO",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "America/Sao_Paulo");

            migrationBuilder.AddColumn<int>(
                name: "SILENCIO_FIM",
                schema: "notificacoes",
                table: "PREFERENCIA_NOTIFICACAO",
                type: "integer",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<int>(
                name: "SILENCIO_INICIO",
                schema: "notificacoes",
                table: "PREFERENCIA_NOTIFICACAO",
                type: "integer",
                nullable: false,
                defaultValue: 22);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FUSO",
                schema: "notificacoes",
                table: "PREFERENCIA_NOTIFICACAO");

            migrationBuilder.DropColumn(
                name: "SILENCIO_FIM",
                schema: "notificacoes",
                table: "PREFERENCIA_NOTIFICACAO");

            migrationBuilder.DropColumn(
                name: "SILENCIO_INICIO",
                schema: "notificacoes",
                table: "PREFERENCIA_NOTIFICACAO");
        }
    }
}
