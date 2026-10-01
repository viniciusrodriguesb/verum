using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Ofertas.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class EstadosDescoberta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_OFERTA_CONDICAO_PRODUTO",
                schema: "ofertas",
                table: "OFERTA");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EXECUCAO_CONSULTA_FONTE_STATUS",
                schema: "ofertas",
                table: "EXECUCAO_CONSULTA_FONTE");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OFERTA_CONDICAO_PRODUTO",
                schema: "ofertas",
                table: "OFERTA",
                sql: "\"CONDICAO_PRODUTO\" IN (1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EXECUCAO_CONSULTA_FONTE_STATUS",
                schema: "ofertas",
                table: "EXECUCAO_CONSULTA_FONTE",
                sql: "\"STATUS\" IN (1, 2, 3, 4, 5, 6, 7, 8)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_OFERTA_CONDICAO_PRODUTO",
                schema: "ofertas",
                table: "OFERTA");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EXECUCAO_CONSULTA_FONTE_STATUS",
                schema: "ofertas",
                table: "EXECUCAO_CONSULTA_FONTE");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OFERTA_CONDICAO_PRODUTO",
                schema: "ofertas",
                table: "OFERTA",
                sql: "\"CONDICAO_PRODUTO\" IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EXECUCAO_CONSULTA_FONTE_STATUS",
                schema: "ofertas",
                table: "EXECUCAO_CONSULTA_FONTE",
                sql: "\"STATUS\" IN (1, 2, 3, 4, 5, 6, 7)");
        }
    }
}
