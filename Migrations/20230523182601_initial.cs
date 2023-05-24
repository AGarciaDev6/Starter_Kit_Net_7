using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Starter_NET_7.Migrations
{
  /// <inheritdoc />
  public partial class initial : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "PERMISSIONS",
          columns: table => new
          {
            ID_PERMISSION = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            NAME = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
            STATUS = table.Column<bool>(type: "bit", nullable: false),
            CREATED_BY = table.Column<int>(type: "int", nullable: false),
            CREATION_DATE = table.Column<DateTime>(type: "datetime", nullable: false),
            LAST_UPDATE_BY = table.Column<int>(type: "int", nullable: true),
            LAST_UPDATE_DATE = table.Column<DateTime>(type: "datetime", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_PERMISSIONS", x => x.ID_PERMISSION);
          });

      migrationBuilder.CreateTable(
          name: "ROLES",
          columns: table => new
          {
            ID_ROLE = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            NAME = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
            STATUS = table.Column<bool>(type: "bit", nullable: false),
            CREATED_BY = table.Column<int>(type: "int", nullable: false),
            CREATION_DATE = table.Column<DateTime>(type: "datetime", nullable: false),
            LAST_UPDATE_BY = table.Column<int>(type: "int", nullable: true),
            LAST_UPDATE_DATE = table.Column<DateTime>(type: "datetime", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ROLES", x => x.ID_ROLE);
          });

      migrationBuilder.CreateTable(
          name: "UNION_PERMISSIONS_ROLES",
          columns: table => new
          {
            ROLE_ID = table.Column<int>(type: "int", nullable: false),
            PERMISSION_ID = table.Column<int>(type: "int", nullable: false),
            STATUS = table.Column<bool>(type: "bit", nullable: false),
            ASSIGNED_BY = table.Column<int>(type: "int", nullable: false),
            ASSIGNED_DATE = table.Column<DateTime>(type: "datetime", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_PERMISSIONS_ROLES", x => new { x.ROLE_ID, x.PERMISSION_ID });
            table.ForeignKey(
                      name: "FK_PERMISSIONS_ROLES_PERMISSIONS",
                      column: x => x.PERMISSION_ID,
                      principalTable: "PERMISSIONS",
                      principalColumn: "ID_PERMISSION");
            table.ForeignKey(
                      name: "FK_PERMISSIONS_ROLES_ROLES",
                      column: x => x.ROLE_ID,
                      principalTable: "ROLES",
                      principalColumn: "ID_ROLE");
          });

      migrationBuilder.CreateTable(
          name: "USERS",
          columns: table => new
          {
            ID_USER = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            LAST_NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            EMAIL = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
            PASSWORD = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
            STATUS = table.Column<bool>(type: "bit", nullable: false),
            CREATED_BY = table.Column<int>(type: "int", nullable: false),
            CREATION_DATE = table.Column<DateTime>(type: "datetime", nullable: false),
            LAST_UPDATE_BY = table.Column<int>(type: "int", nullable: true),
            LAST_UPDATE_DATE = table.Column<DateTime>(type: "datetime", nullable: true),
            ROLE_ID = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_USERS", x => x.ID_USER);
            table.ForeignKey(
                      name: "FK_USERS_ROLES",
                      column: x => x.ROLE_ID,
                      principalTable: "ROLES",
                      principalColumn: "ID_ROLE");
          });

      migrationBuilder.CreateTable(
          name: "UNION_PERMISSIONS_USERS",
          columns: table => new
          {
            PERMISSION_ID = table.Column<int>(type: "int", nullable: false),
            USER_ID = table.Column<int>(type: "int", nullable: false),
            STATUS = table.Column<bool>(type: "bit", nullable: false),
            ASSIGNED_BY = table.Column<int>(type: "int", nullable: false),
            ASSIGNED_DATE = table.Column<DateTime>(type: "datetime", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_PERMISSIONS_USERS", x => new { x.PERMISSION_ID, x.USER_ID });
            table.ForeignKey(
                      name: "FK_PERMISSIONS_USERS_PERMISSIONS",
                      column: x => x.PERMISSION_ID,
                      principalTable: "PERMISSIONS",
                      principalColumn: "ID_PERMISSION");
            table.ForeignKey(
                      name: "FK_PERMISSIONS_USERS_USERS",
                      column: x => x.USER_ID,
                      principalTable: "USERS",
                      principalColumn: "ID_USER");
          });

      migrationBuilder.CreateTable(
          name: "USER_VALIDATION",
          columns: table => new
          {
            ID_USER_VALIDATION = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            REFRESH_TOKEN = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
            REFRESH_TOKEN_EXPIRY = table.Column<DateTime>(type: "datetime", nullable: true),
            FORGOT_PASSWORD_UUID = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
            FORGOT_PASSWORD_EXPIRY = table.Column<DateTime>(type: "datetime", nullable: true),
            USER_ID = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_USER_VALIDATION", x => x.ID_USER_VALIDATION);
            table.ForeignKey(
                      name: "FK_USER_VALIDATION_USERS_USER_ID",
                      column: x => x.USER_ID,
                      principalTable: "USERS",
                      principalColumn: "ID_USER",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.InsertData(
          table: "PERMISSIONS",
          columns: new[] { "ID_PERMISSION", "CREATED_BY", "CREATION_DATE", "LAST_UPDATE_BY", "LAST_UPDATE_DATE", "NAME", "STATUS" },
          values: new object[,]
          {
                    { 1, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1597), null, null, "Roles", true },
                    { 2, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1611), null, null, "Users", true }
          });

      migrationBuilder.InsertData(
          table: "ROLES",
          columns: new[] { "ID_ROLE", "CREATED_BY", "CREATION_DATE", "LAST_UPDATE_BY", "LAST_UPDATE_DATE", "NAME", "STATUS" },
          values: new object[,]
          {
                    { 1, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1497), null, null, "Root", true },
                    { 2, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1554), null, null, "Admin", true }
          });

      migrationBuilder.InsertData(
          table: "UNION_PERMISSIONS_ROLES",
          columns: new[] { "PERMISSION_ID", "ROLE_ID", "ASSIGNED_BY", "ASSIGNED_DATE", "STATUS" },
          values: new object[,]
          {
                    { 1, 1, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1624), true },
                    { 2, 1, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1635), true },
                    { 1, 2, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1642), true },
                    { 2, 2, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1649), true }
          });

      migrationBuilder.InsertData(
          table: "USERS",
          columns: new[] { "ID_USER", "CREATED_BY", "CREATION_DATE", "EMAIL", "LAST_NAME", "LAST_UPDATE_BY", "LAST_UPDATE_DATE", "NAME", "PASSWORD", "ROLE_ID", "STATUS" },
          values: new object[] { 1, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1566), "superuser@mail.com", "User", null, null, "Super", "$2a$11$aMKwWQmIv5iVvOY/ysJhK.OrZ4mifA0fGw7uELtr2MkOgwkE5Fm22", 1, true });

      migrationBuilder.InsertData(
          table: "UNION_PERMISSIONS_USERS",
          columns: new[] { "PERMISSION_ID", "USER_ID", "ASSIGNED_BY", "ASSIGNED_DATE", "STATUS" },
          values: new object[,]
          {
                    { 1, 1, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1662), true },
                    { 2, 1, 1, new DateTime(2023, 5, 23, 12, 26, 1, 140, DateTimeKind.Local).AddTicks(1672), true }
          });

      migrationBuilder.InsertData(
          table: "USER_VALIDATION",
          columns: new[] { "ID_USER_VALIDATION", "FORGOT_PASSWORD_EXPIRY", "FORGOT_PASSWORD_UUID", "REFRESH_TOKEN", "REFRESH_TOKEN_EXPIRY", "USER_ID" },
          values: new object[] { 1, null, null, null, null, 1 });

      migrationBuilder.CreateIndex(
          name: "IX_UNION_PERMISSIONS_ROLES_PERMISSION_ID",
          table: "UNION_PERMISSIONS_ROLES",
          column: "PERMISSION_ID");

      migrationBuilder.CreateIndex(
          name: "IX_UNION_PERMISSIONS_USERS_USER_ID",
          table: "UNION_PERMISSIONS_USERS",
          column: "USER_ID");

      migrationBuilder.CreateIndex(
          name: "IX_USER_VALIDATION_USER_ID",
          table: "USER_VALIDATION",
          column: "USER_ID",
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_USERS_ROLE_ID",
          table: "USERS",
          column: "ROLE_ID");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "UNION_PERMISSIONS_ROLES");

      migrationBuilder.DropTable(
          name: "UNION_PERMISSIONS_USERS");

      migrationBuilder.DropTable(
          name: "USER_VALIDATION");

      migrationBuilder.DropTable(
          name: "PERMISSIONS");

      migrationBuilder.DropTable(
          name: "USERS");

      migrationBuilder.DropTable(
          name: "ROLES");
    }
  }
}
