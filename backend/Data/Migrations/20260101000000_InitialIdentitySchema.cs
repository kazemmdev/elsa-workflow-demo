using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElsaWorkflow.Data.Migrations;

public partial class InitialIdentitySchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "roles",
            columns: table => new
            {
                Id               = table.Column<string>(nullable: false),
                Name             = table.Column<string>(maxLength: 256, nullable: true),
                NormalizedName   = table.Column<string>(maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(nullable: true),
                Description      = table.Column<string>(nullable: true),
            },
            constraints: table => table.PrimaryKey("PK_roles", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "roles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateTable(
            name: "role_claims",
            columns: table => new
            {
                Id         = table.Column<int>(nullable: false)
                                  .Annotation("Npgsql:ValueGenerationStrategy",
                                      Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
                                            .NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RoleId     = table.Column<string>(nullable: false),
                ClaimType  = table.Column<string>(nullable: true),
                ClaimValue = table.Column<string>(nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_role_claims", x => x.Id);
                table.ForeignKey("FK_role_claims_roles_RoleId",
                    x => x.RoleId, "roles", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_role_claims_RoleId", "role_claims", "RoleId");

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                Id                   = table.Column<string>(nullable: false),
                UserName             = table.Column<string>(maxLength: 256, nullable: true),
                NormalizedUserName   = table.Column<string>(maxLength: 256, nullable: true),
                Email                = table.Column<string>(maxLength: 256, nullable: true),
                NormalizedEmail      = table.Column<string>(maxLength: 256, nullable: true),
                EmailConfirmed       = table.Column<bool>(nullable: false),
                PasswordHash         = table.Column<string>(nullable: true),
                SecurityStamp        = table.Column<string>(nullable: true),
                ConcurrencyStamp     = table.Column<string>(nullable: true),
                PhoneNumber          = table.Column<string>(nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                TwoFactorEnabled     = table.Column<bool>(nullable: false),
                LockoutEnd           = table.Column<DateTimeOffset>(nullable: true),
                LockoutEnabled       = table.Column<bool>(nullable: false),
                AccessFailedCount    = table.Column<int>(nullable: false),
                FirstName            = table.Column<string>(nullable: true),
                LastName             = table.Column<string>(nullable: true),
                CreatedAt            = table.Column<DateTimeOffset>(nullable: false),
                LastLoginAt          = table.Column<DateTimeOffset>(nullable: true),
                IsActive             = table.Column<bool>(nullable: false, defaultValue: true),
            },
            constraints: table => table.PrimaryKey("PK_users", x => x.Id));

        migrationBuilder.CreateIndex("UserNameIndex",   "users", "NormalizedUserName", unique: true);
        migrationBuilder.CreateIndex("EmailIndex",      "users", "NormalizedEmail");

        migrationBuilder.CreateTable(
            name: "user_claims",
            columns: table => new
            {
                Id         = table.Column<int>(nullable: false)
                                  .Annotation("Npgsql:ValueGenerationStrategy",
                                      Npgsql.EntityFrameworkCore.PostgreSQL.Metadata
                                            .NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId     = table.Column<string>(nullable: false),
                ClaimType  = table.Column<string>(nullable: true),
                ClaimValue = table.Column<string>(nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_claims", x => x.Id);
                table.ForeignKey("FK_user_claims_users_UserId",
                    x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_user_claims_UserId", "user_claims", "UserId");

        migrationBuilder.CreateTable(
            name: "user_logins",
            columns: table => new
            {
                LoginProvider       = table.Column<string>(nullable: false),
                ProviderKey         = table.Column<string>(nullable: false),
                ProviderDisplayName = table.Column<string>(nullable: true),
                UserId              = table.Column<string>(nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_logins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey("FK_user_logins_users_UserId",
                    x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_user_logins_UserId", "user_logins", "UserId");

        migrationBuilder.CreateTable(
            name: "user_roles",
            columns: table => new
            {
                UserId = table.Column<string>(nullable: false),
                RoleId = table.Column<string>(nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_roles", x => new { x.UserId, x.RoleId });
                table.ForeignKey("FK_user_roles_roles_RoleId",
                    x => x.RoleId, "roles", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_user_roles_users_UserId",
                    x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_user_roles_RoleId", "user_roles", "RoleId");

        migrationBuilder.CreateTable(
            name: "user_tokens",
            columns: table => new
            {
                UserId        = table.Column<string>(nullable: false),
                LoginProvider = table.Column<string>(nullable: false),
                Name          = table.Column<string>(nullable: false),
                Value         = table.Column<string>(nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey("FK_user_tokens_users_UserId",
                    x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("user_tokens");
        migrationBuilder.DropTable("user_roles");
        migrationBuilder.DropTable("user_logins");
        migrationBuilder.DropTable("user_claims");
        migrationBuilder.DropTable("users");
        migrationBuilder.DropTable("role_claims");
        migrationBuilder.DropTable("roles");
    }
}
