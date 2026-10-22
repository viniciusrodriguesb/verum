using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Radar.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class NomeAnuncioCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NOME",
                schema: "radar",
                table: "MONITORAMENTO",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldMaxLength: 250);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NOME",
                schema: "radar",
                table: "MONITORAMENTO",
                type: "varchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(300)",
                oldMaxLength: 300);
        }
    }
}
