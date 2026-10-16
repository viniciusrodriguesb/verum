using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verum.Modules.Assinaturas.Infraestrutura.Persistencia.Migracoes
{
  /// <inheritdoc />
  public partial class PagamentosRecorrentes : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "ATUALIZADO_GATEWAY_EM",
          schema: "assinaturas",
          table: "PAGAMENTO",
          type: "timestamp with time zone",
          nullable: true);

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "PERIODO_FIM",
          schema: "assinaturas",
          table: "PAGAMENTO",
          type: "timestamp with time zone",
          nullable: true);

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "PERIODO_INICIO",
          schema: "assinaturas",
          table: "PAGAMENTO",
          type: "timestamp with time zone",
          nullable: true);

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "PROXIMA_TENTATIVA_EM",
          schema: "assinaturas",
          table: "EVENTO_GATEWAY",
          type: "timestamp with time zone",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "CHAVE_IDEMPOTENCIA",
          schema: "assinaturas",
          table: "ASSINATURA",
          type: "varchar(100)",
          maxLength: 100,
          nullable: true);

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "RECONCILIACAO_AGENDADA_EM",
          schema: "assinaturas",
          table: "ASSINATURA",
          type: "timestamp with time zone",
          nullable: true);

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "SINCRONIZADA_EM",
          schema: "assinaturas",
          table: "ASSINATURA",
          type: "timestamp with time zone",
          nullable: true);

      migrationBuilder.AddColumn<string>(
          name: "URL_CHECKOUT",
          schema: "assinaturas",
          table: "ASSINATURA",
          type: "varchar(2000)",
          maxLength: 2000,
          nullable: true);

      migrationBuilder.CreateIndex(
          name: "IX_EVENTO_GATEWAY_STATUS_PROXIMA_TENTATIVA_EM_ID",
          schema: "assinaturas",
          table: "EVENTO_GATEWAY",
          columns: new[] { "STATUS", "PROXIMA_TENTATIVA_EM", "ID" });

      migrationBuilder.CreateIndex(
          name: "IX_ASSINATURA_CONTA_ID_CHAVE_IDEMPOTENCIA",
          schema: "assinaturas",
          table: "ASSINATURA",
          columns: new[] { "CONTA_ID", "CHAVE_IDEMPOTENCIA" },
          unique: true,
          filter: "\"CHAVE_IDEMPOTENCIA\" IS NOT NULL");

      migrationBuilder.CreateIndex(
          name: "IX_ASSINATURA_RECONCILIACAO_AGENDADA_EM",
          schema: "assinaturas",
          table: "ASSINATURA",
          column: "RECONCILIACAO_AGENDADA_EM");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
          name: "IX_EVENTO_GATEWAY_STATUS_PROXIMA_TENTATIVA_EM_ID",
          schema: "assinaturas",
          table: "EVENTO_GATEWAY");

      migrationBuilder.DropIndex(
          name: "IX_ASSINATURA_CONTA_ID_CHAVE_IDEMPOTENCIA",
          schema: "assinaturas",
          table: "ASSINATURA");

      migrationBuilder.DropIndex(
          name: "IX_ASSINATURA_RECONCILIACAO_AGENDADA_EM",
          schema: "assinaturas",
          table: "ASSINATURA");

      migrationBuilder.DropColumn(
          name: "ATUALIZADO_GATEWAY_EM",
          schema: "assinaturas",
          table: "PAGAMENTO");

      migrationBuilder.DropColumn(
          name: "PERIODO_FIM",
          schema: "assinaturas",
          table: "PAGAMENTO");

      migrationBuilder.DropColumn(
          name: "PERIODO_INICIO",
          schema: "assinaturas",
          table: "PAGAMENTO");

      migrationBuilder.DropColumn(
          name: "PROXIMA_TENTATIVA_EM",
          schema: "assinaturas",
          table: "EVENTO_GATEWAY");

      migrationBuilder.DropColumn(
          name: "CHAVE_IDEMPOTENCIA",
          schema: "assinaturas",
          table: "ASSINATURA");

      migrationBuilder.DropColumn(
          name: "RECONCILIACAO_AGENDADA_EM",
          schema: "assinaturas",
          table: "ASSINATURA");

      migrationBuilder.DropColumn(
          name: "SINCRONIZADA_EM",
          schema: "assinaturas",
          table: "ASSINATURA");

      migrationBuilder.DropColumn(
          name: "URL_CHECKOUT",
          schema: "assinaturas",
          table: "ASSINATURA");
    }
  }
}
