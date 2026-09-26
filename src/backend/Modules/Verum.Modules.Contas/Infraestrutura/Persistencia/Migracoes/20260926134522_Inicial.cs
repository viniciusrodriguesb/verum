using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Contas.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.EnsureSchema(
                name: "contas");

            migrationBuilder.CreateTable(
                name: "CONTA",
                schema: "contas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    SUJEITO_IDENTIDADE = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: false),
                    NOME_EXIBICAO = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    STATUS = table.Column<short>(type: "smallint", nullable: false),
                    CRIADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ATUALIZADA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EXCLUIDA_EM = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                  table.PrimaryKey("PK_CONTA", x => x.ID);

                    table.CheckConstraint("CK_CONTA_EMAIL", "length(btrim(\"EMAIL\")) > 0");

                    table.CheckConstraint("CK_CONTA_ID", "\"ID\" <> '00000000-0000-0000-0000-000000000000'::uuid");

                    table.CheckConstraint("CK_CONTA_NOME_EXIBICAO", "length(btrim(\"NOME_EXIBICAO\")) > 0");

                    table.CheckConstraint("CK_CONTA_STATUS", "\"STATUS\" IN (1, 2, 3)");

                    table.CheckConstraint("CK_CONTA_SUJEITO_IDENTIDADE", "length(btrim(\"SUJEITO_IDENTIDADE\")) > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONTA_SUJEITO_IDENTIDADE",
                schema: "contas",
                table: "CONTA",
                column: "SUJEITO_IDENTIDADE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.DropTable(
                name: "CONTA",
                schema: "contas");
        }
    }
}
