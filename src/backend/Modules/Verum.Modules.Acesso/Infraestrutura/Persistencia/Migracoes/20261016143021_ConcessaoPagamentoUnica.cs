using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Acesso.Infraestrutura.Persistencia.Migracoes
{
  /// <inheritdoc />
  public partial class ConcessaoPagamentoUnica : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateIndex(
          name: "IX_CONCESSAO_PACOTE_ORIGEM_REFERENCIA_ORIGEM_ID",
          schema: "acesso",
          table: "CONCESSAO_PACOTE",
          columns: new[] { "ORIGEM", "REFERENCIA_ORIGEM_ID" },
          unique: true,
          filter: "\"ORIGEM\" = 2 AND \"REFERENCIA_ORIGEM_ID\" IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
          name: "IX_CONCESSAO_PACOTE_ORIGEM_REFERENCIA_ORIGEM_ID",
          schema: "acesso",
          table: "CONCESSAO_PACOTE");
    }
  }
}
