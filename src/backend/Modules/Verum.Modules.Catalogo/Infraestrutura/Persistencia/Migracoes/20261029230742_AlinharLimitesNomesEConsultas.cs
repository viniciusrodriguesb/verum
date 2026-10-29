using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Catalogo.Infraestrutura.Persistencia.Migracoes
{
  /// <inheritdoc />
  public partial class AlinharLimitesNomesEConsultas : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
          name: "TERMO_ORIGINAL",
          schema: "catalogo",
          table: "PRODUTO_TERMO_BUSCA",
          type: "varchar(500)",
          maxLength: 500,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "varchar(300)",
          oldMaxLength: 300);

      migrationBuilder.AlterColumn<string>(
          name: "TERMO_NORMALIZADO",
          schema: "catalogo",
          table: "PRODUTO_TERMO_BUSCA",
          type: "varchar(500)",
          maxLength: 500,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "varchar(300)",
          oldMaxLength: 300);

      migrationBuilder.AlterColumn<string>(
          name: "NOME_NORMALIZADO",
          schema: "catalogo",
          table: "PRODUTO",
          type: "varchar(300)",
          maxLength: 300,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "varchar(250)",
          oldMaxLength: 250);

      migrationBuilder.AlterColumn<string>(
          name: "NOME",
          schema: "catalogo",
          table: "PRODUTO",
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
          name: "TERMO_ORIGINAL",
          schema: "catalogo",
          table: "PRODUTO_TERMO_BUSCA",
          type: "varchar(300)",
          maxLength: 300,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "varchar(500)",
          oldMaxLength: 500);

      migrationBuilder.AlterColumn<string>(
          name: "TERMO_NORMALIZADO",
          schema: "catalogo",
          table: "PRODUTO_TERMO_BUSCA",
          type: "varchar(300)",
          maxLength: 300,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "varchar(500)",
          oldMaxLength: 500);

      migrationBuilder.AlterColumn<string>(
          name: "NOME_NORMALIZADO",
          schema: "catalogo",
          table: "PRODUTO",
          type: "varchar(250)",
          maxLength: 250,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "varchar(300)",
          oldMaxLength: 300);

      migrationBuilder.AlterColumn<string>(
          name: "NOME",
          schema: "catalogo",
          table: "PRODUTO",
          type: "varchar(250)",
          maxLength: 250,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "varchar(300)",
          oldMaxLength: 300);
    }
  }
}
