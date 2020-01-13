﻿using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace TeamProject.Migrations
{
    public partial class entrance_form : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntranceFormFields",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    IdField = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntranceFormFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SelectFieldOptions",
                columns: table => new
                {
                    idOption = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    idField = table.Column<int>(nullable: false),
                    option = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectFieldOptions", x => x.idOption);
                });

            migrationBuilder.CreateTable(
                name: "Validations",
                columns: table => new
                {
                    idValidation = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    idField = table.Column<int>(nullable: false),
                    type = table.Column<string>(nullable: true),
                    value = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Validations", x => x.idValidation);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntranceFormFields");

            migrationBuilder.DropTable(
                name: "SelectFieldOptions");

            migrationBuilder.DropTable(
                name: "Validations");
        }
    }
}
