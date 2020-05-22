using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TeamProject.Migrations
{
    public partial class _0503Migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerID",
                table: "Logs");

            migrationBuilder.AddColumn<string>(
                name: "AnswerValue",
                table: "Logs",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FieldFieldDependencyIdDependency",
                table: "Field",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Dependencies",
                columns: table => new
                {
                    IdDependency = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Id = table.Column<int>(nullable: false),
                    DependencyType = table.Column<int>(nullable: false),
                    ActivationValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dependencies", x => x.IdDependency);
                    table.ForeignKey(
                        name: "FK_Dependencies_Field_Id",
                        column: x => x.Id,
                        principalTable: "Field",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Field_FieldFieldDependencyIdDependency",
                table: "Field",
                column: "FieldFieldDependencyIdDependency");

            migrationBuilder.CreateIndex(
                name: "IX_Dependencies_Id",
                table: "Dependencies",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_Dependencies_FieldFieldDependencyIdDependency",
                table: "Field",
                column: "FieldFieldDependencyIdDependency",
                principalTable: "Dependencies",
                principalColumn: "IdDependency",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_Dependencies_FieldFieldDependencyIdDependency",
                table: "Field");

            migrationBuilder.DropTable(
                name: "Dependencies");

            migrationBuilder.DropIndex(
                name: "IX_Field_FieldFieldDependencyIdDependency",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "AnswerValue",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "FieldFieldDependencyIdDependency",
                table: "Field");

            migrationBuilder.AddColumn<int>(
                name: "AnswerID",
                table: "Logs",
                nullable: true);
        }
    }
}
