using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Radar.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class FluxoRadar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AVISO_ENCAMINHADO_EM",
                schema: "radar",
                table: "OPORTUNIDADE",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CHAVE_IDEMPOTENCIA",
                schema: "radar",
                table: "MONITORAMENTO",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CONDICAO",
                schema: "radar",
                table: "MONITORAMENTO",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ESTADO_VERIFICACAO",
                schema: "radar",
                table: "MONITORAMENTO",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OFERTA_ID",
                schema: "radar",
                table: "MONITORAMENTO",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "REARMADO",
                schema: "radar",
                table: "MONITORAMENTO",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MONITORAMENTO_CONTA_ID_CHAVE_IDEMPOTENCIA",
                schema: "radar",
                table: "MONITORAMENTO",
                columns: new[] { "CONTA_ID", "CHAVE_IDEMPOTENCIA" },
                unique: true,
                filter: "\"CHAVE_IDEMPOTENCIA\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_MONITORAMENTO_OFERTA_ID_PROXIMA_VERIFICACAO_EM",
                schema: "radar",
                table: "MONITORAMENTO",
                columns: new[] { "OFERTA_ID", "PROXIMA_VERIFICACAO_EM" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MONITORAMENTO_CONTA_ID_CHAVE_IDEMPOTENCIA",
                schema: "radar",
                table: "MONITORAMENTO");

            migrationBuilder.DropIndex(
                name: "IX_MONITORAMENTO_OFERTA_ID_PROXIMA_VERIFICACAO_EM",
                schema: "radar",
                table: "MONITORAMENTO");

            migrationBuilder.DropColumn(
                name: "AVISO_ENCAMINHADO_EM",
                schema: "radar",
                table: "OPORTUNIDADE");

            migrationBuilder.DropColumn(
                name: "CHAVE_IDEMPOTENCIA",
                schema: "radar",
                table: "MONITORAMENTO");

            migrationBuilder.DropColumn(
                name: "CONDICAO",
                schema: "radar",
                table: "MONITORAMENTO");

            migrationBuilder.DropColumn(
                name: "ESTADO_VERIFICACAO",
                schema: "radar",
                table: "MONITORAMENTO");

            migrationBuilder.DropColumn(
                name: "OFERTA_ID",
                schema: "radar",
                table: "MONITORAMENTO");

            migrationBuilder.DropColumn(
                name: "REARMADO",
                schema: "radar",
                table: "MONITORAMENTO");
        }
    }
}
