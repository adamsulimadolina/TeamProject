using Microsoft.EntityFrameworkCore.Migrations;

namespace TeamProject.Migrations
{
    public partial class _0404 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdTest",
                table: "PatientForms",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdTest",
                table: "PatientForms");
        }
    }
}
