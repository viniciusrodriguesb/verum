using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Notificacoes.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class IndicesEnvio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACAO_CONTA_ID_CRIADA_EM",
                schema: "notificacoes",
                table: "NOTIFICACAO",
                columns: new[] { "CONTA_ID", "CRIADA_EM" });

            migrationBuilder.CreateIndex(
                name: "IX_ENTREGA_NOTIFICACAO_STATUS_PROXIMA_TENTATIVA_EM",
                schema: "notificacoes",
                table: "ENTREGA_NOTIFICACAO",
                columns: new[] { "STATUS", "PROXIMA_TENTATIVA_EM" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NOTIFICACAO_CONTA_ID_CRIADA_EM",
                schema: "notificacoes",
                table: "NOTIFICACAO");

            migrationBuilder.DropIndex(
                name: "IX_ENTREGA_NOTIFICACAO_STATUS_PROXIMA_TENTATIVA_EM",
                schema: "notificacoes",
                table: "ENTREGA_NOTIFICACAO");
        }
    }
}
