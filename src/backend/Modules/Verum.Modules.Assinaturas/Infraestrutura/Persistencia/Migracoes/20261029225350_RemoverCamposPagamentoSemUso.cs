using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Migracoes
{
  /// <inheritdoc />
  public partial class RemoverCamposPagamentoSemUso : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql("""
                DO $$
                BEGIN
                  IF EXISTS (SELECT 1 FROM assinaturas."PAGAMENTO"
                    WHERE "METODO" IS NOT NULL OR "VENCIMENTO_EM" IS NOT NULL) THEN
                    RAISE EXCEPTION 'Há dados nos campos de pagamento obsoletos; preserve-os antes de migrar.';
                  END IF;
                END $$;
                """);

      migrationBuilder.DropCheckConstraint(
          name: "CK_PAGAMENTO_METODO",
          schema: "assinaturas",
          table: "PAGAMENTO");

      migrationBuilder.DropColumn(
          name: "METODO",
          schema: "assinaturas",
          table: "PAGAMENTO");

      migrationBuilder.DropColumn(
          name: "VENCIMENTO_EM",
          schema: "assinaturas",
          table: "PAGAMENTO");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<short>(
          name: "METODO",
          schema: "assinaturas",
          table: "PAGAMENTO",
          type: "smallint",
          nullable: true);

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "VENCIMENTO_EM",
          schema: "assinaturas",
          table: "PAGAMENTO",
          type: "timestamp with time zone",
          nullable: true);

      migrationBuilder.AddCheckConstraint(
          name: "CK_PAGAMENTO_METODO",
          schema: "assinaturas",
          table: "PAGAMENTO",
          sql: "\"METODO\" IN (1, 2, 3)");
    }
  }
}
