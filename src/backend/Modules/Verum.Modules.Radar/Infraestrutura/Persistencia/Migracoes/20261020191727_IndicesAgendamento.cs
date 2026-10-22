using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Radar.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class IndicesAgendamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OPORTUNIDADE_DETECTADA_EM",
                schema: "radar",
                table: "OPORTUNIDADE",
                column: "DETECTADA_EM",
                filter: "\"AVISO_ENCAMINHADO_EM\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MONITORAMENTO_STATUS_PROXIMA_VERIFICACAO_EM",
                schema: "radar",
                table: "MONITORAMENTO",
                columns: new[] { "STATUS", "PROXIMA_VERIFICACAO_EM" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OPORTUNIDADE_DETECTADA_EM",
                schema: "radar",
                table: "OPORTUNIDADE");

            migrationBuilder.DropIndex(
                name: "IX_MONITORAMENTO_STATUS_PROXIMA_VERIFICACAO_EM",
                schema: "radar",
                table: "MONITORAMENTO");
        }
    }
}
