using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemBrightSpotBE.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employment_status",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employment_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "genders",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_genders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    last_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    post_code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    region = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    locality = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    street = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    building_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<long>(type: "bigint", nullable: false, comment: "\r\n            1: InReview\r\n            2: Scheduled\r\n            3: Actived\r\n            4: Suspended\r\n            5: Expired\r\n        "),
                    send_mail = table.Column<bool>(type: "boolean", nullable: false),
                    send_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenants_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "certifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_certifications_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    phone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.id);
                    table.ForeignKey(
                        name: "FK_companies_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_awards",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_awards", x => x.id);
                    table.ForeignKey(
                        name: "FK_company_awards_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                    table.ForeignKey(
                        name: "FK_departments_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employment_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employment_types", x => x.id);
                    table.ForeignKey(
                        name: "FK_employment_types_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "experience_jobs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_experience_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_experience_jobs_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "monitoring_systems",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    last_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    email = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    phone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monitoring_systems", x => x.id);
                    table.ForeignKey(
                        name: "FK_monitoring_systems_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "participation_positions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participation_positions", x => x.id);
                    table.ForeignKey(
                        name: "FK_participation_positions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "participation_processes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participation_processes", x => x.id);
                    table.ForeignKey(
                        name: "FK_participation_processes_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.id);
                    table.ForeignKey(
                        name: "FK_positions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_types",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_types", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_types_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "divisions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_divisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_divisions_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_divisions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "experience_fields",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    experience_job_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_experience_fields", x => x.id);
                    table.ForeignKey(
                        name: "FK_experience_fields_experience_jobs_experience_job_id",
                        column: x => x.experience_job_id,
                        principalTable: "experience_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_experience_fields_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    division_id = table.Column<long>(type: "bigint", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_groups_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_groups_divisions_division_id",
                        column: x => x.division_id,
                        principalTable: "divisions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_groups_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "experience_areas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    experience_field_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_experience_areas", x => x.id);
                    table.ForeignKey(
                        name: "FK_experience_areas_experience_fields_experience_field_id",
                        column: x => x.experience_field_id,
                        principalTable: "experience_fields",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_experience_areas_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    first_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    last_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    first_name_kana = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    last_name_kana = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    gender_id = table.Column<long>(type: "bigint", nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    nearest_station = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    role_id = table.Column<long>(type: "bigint", nullable: true, comment: "\r\n            1 = System admin\r\n            2 = Power User\r\n            3 = Senior User\r\n            4 = Contributor\r\n            5 = Member"),
                    department_id = table.Column<long>(type: "bigint", nullable: true),
                    division_id = table.Column<long>(type: "bigint", nullable: true),
                    group_id = table.Column<long>(type: "bigint", nullable: true),
                    position_id = table.Column<long>(type: "bigint", nullable: true),
                    employment_type_id = table.Column<long>(type: "bigint", nullable: true),
                    employment_status_id = table.Column<long>(type: "bigint", nullable: true),
                    is_tenant_created = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0：手動作成\r\n            1：テナントによる自動作成"),
                    active = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0：デフォルト\r\n            1：電子メールが正常に検証されたとき"),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    temp_password_used = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0：アカウントが最初のパスワードを変更し、確認されました\r\n            1: アカウントの作成中に使用される一時的なパスワードまたはアカウントがアクティブ化された後にリセットされたパスワード"),
                    temp_password_expired_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_users_divisions_division_id",
                        column: x => x.division_id,
                        principalTable: "divisions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_users_employment_status_employment_status_id",
                        column: x => x.employment_status_id,
                        principalTable: "employment_status",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_users_employment_types_employment_type_id",
                        column: x => x.employment_type_id,
                        principalTable: "employment_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_users_genders_gender_id",
                        column: x => x.gender_id,
                        principalTable: "genders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_users_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_users_positions_position_id",
                        column: x => x.position_id,
                        principalTable: "positions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "specific_skills",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    experience_area_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    delete_flag = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            0: 許可されていません\r\n            1: 許可されています")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_specific_skills", x => x.id);
                    table.ForeignKey(
                        name: "FK_specific_skills_experience_areas_experience_area_id",
                        column: x => x.experience_area_id,
                        principalTable: "experience_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_specific_skills_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    complete_date = table.Column<DateOnly>(type: "date", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    division_id = table.Column<long>(type: "bigint", nullable: true),
                    group_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<long>(type: "bigint", nullable: true, comment: "\r\n            1: 完了 (completed: all member plan status completed)\r\n            2: 進行中 (in progress: the plan has ≥1 members)\r\n            3: 未着手 (not started: the plan has no members)\r\n        "),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_plans_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_plans_divisions_division_id",
                        column: x => x.division_id,
                        principalTable: "divisions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_plans_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_plans_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_plans_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_projects_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_projects_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_projects_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    report_type_id = table.Column<long>(type: "bigint", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: true, comment: "\r\n            1：公開\r\n            0：非公開"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_reports_report_types_report_type_id",
                        column: x => x.report_type_id,
                        principalTable: "report_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reports_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reports_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: true),
                    file_url = table.Column<string>(type: "text", nullable: true),
                    file_url_thumb = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_settings_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_certification",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    certification_id = table.Column<long>(type: "bigint", nullable: false),
                    certified_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_certification", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_certification_certifications_certification_id",
                        column: x => x.certification_id,
                        principalTable: "certifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_certification_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_certification_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_company_award",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    company_award_id = table.Column<long>(type: "bigint", nullable: false),
                    awarded_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_company_award", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_company_award_company_awards_company_award_id",
                        column: x => x.company_award_id,
                        principalTable: "company_awards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_company_award_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_company_award_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_experience_area",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    experience_area_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_experience_area", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_experience_area_experience_areas_experience_area_id",
                        column: x => x.experience_area_id,
                        principalTable: "experience_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_experience_area_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_experience_area_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_experience_field",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    experience_field_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_experience_field", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_experience_field_experience_fields_experience_field_id",
                        column: x => x.experience_field_id,
                        principalTable: "experience_fields",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_experience_field_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_experience_field_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_experience_job",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    experience_job_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_experience_job", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_experience_job_experience_jobs_experience_job_id",
                        column: x => x.experience_job_id,
                        principalTable: "experience_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_experience_job_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_experience_job_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_status_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<int>(type: "integer", nullable: false, comment: "\r\n            1: 入社\r\n            2: 休職\r\n            3: 復帰\r\n            4: 退職"),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_status_history_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_status_history_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_specific_skill",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    specific_skill_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_specific_skill", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_specific_skill_specific_skills_specific_skill_id",
                        column: x => x.specific_skill_id,
                        principalTable: "specific_skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_specific_skill_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_specific_skill_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_conditions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    overview = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    est_time = table.Column<int>(type: "integer", nullable: true),
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_conditions", x => x.id);
                    table.ForeignKey(
                        name: "FK_plan_conditions_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_plan_conditions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_plan",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<long>(type: "bigint", nullable: false, comment: "\r\n            1: 完了 (completed: Status after the completion request is approved)\r\n            2: 進行中 (in progress: Status after the plan is allocated)\r\n            3: 承認待ち (pending approval: Status after the completion request is submitted)"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    plan_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_plan", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_plan_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_plan_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_experience_area",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    experience_area_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_experience_area", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_experience_area_experience_areas_experience_area_id",
                        column: x => x.experience_area_id,
                        principalTable: "experience_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_experience_area_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_experience_area_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_experience_field",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    experience_field_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_experience_field", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_experience_field_experience_fields_experience_field~",
                        column: x => x.experience_field_id,
                        principalTable: "experience_fields",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_experience_field_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_experience_field_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_experience_job",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    experience_job_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_experience_job", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_experience_job_experience_jobs_experience_job_id",
                        column: x => x.experience_job_id,
                        principalTable: "experience_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_experience_job_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_experience_job_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_participation_position",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    participation_position_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_participation_position", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_participation_position_participation_positions_part~",
                        column: x => x.participation_position_id,
                        principalTable: "participation_positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_participation_position_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_participation_position_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_participation_process",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    participation_process_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_participation_process", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_participation_process_participation_processes_parti~",
                        column: x => x.participation_process_id,
                        principalTable: "participation_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_participation_process_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_participation_process_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_specific_skill",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    specific_skill_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_specific_skill", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_specific_skill_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_specific_skill_specific_skills_specific_skill_id",
                        column: x => x.specific_skill_id,
                        principalTable: "specific_skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_specific_skill_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, comment: "\r\n            ０：未読\r\n            １：既読"),
                    report_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_departments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_departments", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_departments_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_departments_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_departments_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_divisions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_id = table.Column<long>(type: "bigint", nullable: false),
                    division_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_divisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_divisions_divisions_division_id",
                        column: x => x.division_id,
                        principalTable: "divisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_divisions_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_divisions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_groups",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_id = table.Column<long>(type: "bigint", nullable: false),
                    group_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_groups_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_groups_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_groups_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_users_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_users_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_plan_activity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<long>(type: "bigint", nullable: true, comment: "\r\n            1: Accepted\r\n            2: Submitted\r\n            3: Rejected\r\n            4: Revoked"),
                    revoke_flag = table.Column<bool>(type: "boolean", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    user_plan_id = table.Column<long>(type: "bigint", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_plan_activity", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_plan_activity_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_plan_activity_user_plan_user_plan_id",
                        column: x => x.user_plan_id,
                        principalTable: "user_plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_plan_activity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_plan_condition",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<long>(type: "bigint", nullable: true, comment: "\r\n            1: 完了 (completed)\r\n            2: 未完了 (incomplete)\r\n            3: 承認待ち (pendding approval)"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    plan_condition_id = table.Column<long>(type: "bigint", nullable: false),
                    user_plan_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_plan_condition", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_plan_condition_plan_conditions_plan_condition_id",
                        column: x => x.plan_condition_id,
                        principalTable: "plan_conditions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_plan_condition_user_plan_user_plan_id",
                        column: x => x.user_plan_id,
                        principalTable: "user_plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_plan_condition_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_plan_condition_activity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<long>(type: "bigint", nullable: true, comment: "\r\n            1: Accepted\r\n            2: Submitted\r\n            3: Rejected\r\n            4: Revoked"),
                    file_name = table.Column<string>(type: "text", nullable: true),
                    file_url = table.Column<string>(type: "text", nullable: true),
                    revoke_flag = table.Column<bool>(type: "boolean", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    user_plan_condition_id = table.Column<long>(type: "bigint", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_plan_condition_activity", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_plan_condition_activity_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_plan_condition_activity_user_plan_condition_user_plan_~",
                        column: x => x.user_plan_condition_id,
                        principalTable: "user_plan_condition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_plan_condition_activity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_certifications_tenant_id",
                table: "certifications",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_companies_tenant_id",
                table: "companies",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_company_awards_tenant_id",
                table: "company_awards",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_departments_tenant_id",
                table: "departments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_divisions_department_id",
                table: "divisions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_divisions_tenant_id",
                table: "divisions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_employment_types_tenant_id",
                table: "employment_types",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_experience_areas_experience_field_id",
                table: "experience_areas",
                column: "experience_field_id");

            migrationBuilder.CreateIndex(
                name: "IX_experience_areas_tenant_id",
                table: "experience_areas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_experience_fields_experience_job_id",
                table: "experience_fields",
                column: "experience_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_experience_fields_tenant_id",
                table: "experience_fields",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_experience_jobs_tenant_id",
                table: "experience_jobs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_groups_department_id",
                table: "groups",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_groups_division_id",
                table: "groups",
                column: "division_id");

            migrationBuilder.CreateIndex(
                name: "IX_groups_tenant_id",
                table: "groups",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_monitoring_systems_email",
                table: "monitoring_systems",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_monitoring_systems_tenant_id",
                table: "monitoring_systems",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_report_id",
                table: "notifications",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_tenant_id",
                table: "notifications",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_participation_positions_tenant_id",
                table: "participation_positions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_participation_processes_tenant_id",
                table: "participation_processes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_conditions_plan_id",
                table: "plan_conditions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_conditions_tenant_id",
                table: "plan_conditions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_plans_department_id",
                table: "plans",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_plans_division_id",
                table: "plans",
                column: "division_id");

            migrationBuilder.CreateIndex(
                name: "IX_plans_group_id",
                table: "plans",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_plans_tenant_id",
                table: "plans",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_plans_user_id",
                table: "plans",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_positions_tenant_id",
                table: "positions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_experience_area_experience_area_id",
                table: "project_experience_area",
                column: "experience_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_experience_area_project_id",
                table: "project_experience_area",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_experience_area_tenant_id",
                table: "project_experience_area",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_experience_field_experience_field_id",
                table: "project_experience_field",
                column: "experience_field_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_experience_field_project_id",
                table: "project_experience_field",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_experience_field_tenant_id",
                table: "project_experience_field",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_experience_job_experience_job_id",
                table: "project_experience_job",
                column: "experience_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_experience_job_project_id",
                table: "project_experience_job",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_experience_job_tenant_id",
                table: "project_experience_job",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_participation_position_participation_position_id",
                table: "project_participation_position",
                column: "participation_position_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_participation_position_project_id",
                table: "project_participation_position",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_participation_position_tenant_id",
                table: "project_participation_position",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_participation_process_participation_process_id",
                table: "project_participation_process",
                column: "participation_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_participation_process_project_id",
                table: "project_participation_process",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_participation_process_tenant_id",
                table: "project_participation_process",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_specific_skill_project_id",
                table: "project_specific_skill",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_specific_skill_specific_skill_id",
                table: "project_specific_skill",
                column: "specific_skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_specific_skill_tenant_id",
                table: "project_specific_skill",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_company_id",
                table: "projects",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_tenant_id",
                table: "projects",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_user_id",
                table: "projects",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_departments_department_id",
                table: "report_departments",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_departments_report_id",
                table: "report_departments",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_departments_tenant_id",
                table: "report_departments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_divisions_division_id",
                table: "report_divisions",
                column: "division_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_divisions_report_id",
                table: "report_divisions",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_divisions_tenant_id",
                table: "report_divisions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_groups_group_id",
                table: "report_groups",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_groups_report_id",
                table: "report_groups",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_groups_tenant_id",
                table: "report_groups",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_types_tenant_id",
                table: "report_types",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_users_report_id",
                table: "report_users",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_users_tenant_id",
                table: "report_users",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_users_user_id",
                table: "report_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_report_type_id",
                table: "reports",
                column: "report_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_tenant_id",
                table: "reports",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_user_id",
                table: "reports",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_settings_tenant_id",
                table: "settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_settings_user_id",
                table: "settings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_specific_skills_experience_area_id",
                table: "specific_skills",
                column: "experience_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_specific_skills_tenant_id",
                table: "specific_skills",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_tenant_id",
                table: "tenants",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_certification_certification_id",
                table: "user_certification",
                column: "certification_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_certification_tenant_id",
                table: "user_certification",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_certification_user_id",
                table: "user_certification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_company_award_company_award_id",
                table: "user_company_award",
                column: "company_award_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_company_award_tenant_id",
                table: "user_company_award",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_company_award_user_id",
                table: "user_company_award",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_experience_area_experience_area_id",
                table: "user_experience_area",
                column: "experience_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_experience_area_tenant_id",
                table: "user_experience_area",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_experience_area_user_id",
                table: "user_experience_area",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_experience_field_experience_field_id",
                table: "user_experience_field",
                column: "experience_field_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_experience_field_tenant_id",
                table: "user_experience_field",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_experience_field_user_id",
                table: "user_experience_field",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_experience_job_experience_job_id",
                table: "user_experience_job",
                column: "experience_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_experience_job_tenant_id",
                table: "user_experience_job",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_experience_job_user_id",
                table: "user_experience_job",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_plan_id",
                table: "user_plan",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_user_id",
                table: "user_plan",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_activity_tenant_id",
                table: "user_plan_activity",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_activity_user_id",
                table: "user_plan_activity",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_activity_user_plan_id",
                table: "user_plan_activity",
                column: "user_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_condition_plan_condition_id",
                table: "user_plan_condition",
                column: "plan_condition_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_condition_user_id",
                table: "user_plan_condition",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_condition_user_plan_id",
                table: "user_plan_condition",
                column: "user_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_condition_activity_tenant_id",
                table: "user_plan_condition_activity",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_condition_activity_user_id",
                table: "user_plan_condition_activity",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_plan_condition_activity_user_plan_condition_id",
                table: "user_plan_condition_activity",
                column: "user_plan_condition_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_specific_skill_specific_skill_id",
                table: "user_specific_skill",
                column: "specific_skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_specific_skill_tenant_id",
                table: "user_specific_skill",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_specific_skill_user_id",
                table: "user_specific_skill",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_status_history_tenant_id",
                table: "user_status_history",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_status_history_user_id",
                table: "user_status_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_department_id",
                table: "users",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_division_id",
                table: "users",
                column: "division_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_employment_status_id",
                table: "users",
                column: "employment_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_employment_type_id",
                table: "users",
                column: "employment_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_gender_id",
                table: "users",
                column: "gender_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_group_id",
                table: "users",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_position_id",
                table: "users",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_tenant_id",
                table: "users",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "monitoring_systems");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "project_experience_area");

            migrationBuilder.DropTable(
                name: "project_experience_field");

            migrationBuilder.DropTable(
                name: "project_experience_job");

            migrationBuilder.DropTable(
                name: "project_participation_position");

            migrationBuilder.DropTable(
                name: "project_participation_process");

            migrationBuilder.DropTable(
                name: "project_specific_skill");

            migrationBuilder.DropTable(
                name: "report_departments");

            migrationBuilder.DropTable(
                name: "report_divisions");

            migrationBuilder.DropTable(
                name: "report_groups");

            migrationBuilder.DropTable(
                name: "report_users");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "user_certification");

            migrationBuilder.DropTable(
                name: "user_company_award");

            migrationBuilder.DropTable(
                name: "user_experience_area");

            migrationBuilder.DropTable(
                name: "user_experience_field");

            migrationBuilder.DropTable(
                name: "user_experience_job");

            migrationBuilder.DropTable(
                name: "user_plan_activity");

            migrationBuilder.DropTable(
                name: "user_plan_condition_activity");

            migrationBuilder.DropTable(
                name: "user_specific_skill");

            migrationBuilder.DropTable(
                name: "user_status_history");

            migrationBuilder.DropTable(
                name: "participation_positions");

            migrationBuilder.DropTable(
                name: "participation_processes");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "certifications");

            migrationBuilder.DropTable(
                name: "company_awards");

            migrationBuilder.DropTable(
                name: "user_plan_condition");

            migrationBuilder.DropTable(
                name: "specific_skills");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "report_types");

            migrationBuilder.DropTable(
                name: "plan_conditions");

            migrationBuilder.DropTable(
                name: "user_plan");

            migrationBuilder.DropTable(
                name: "experience_areas");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropTable(
                name: "experience_fields");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "experience_jobs");

            migrationBuilder.DropTable(
                name: "employment_status");

            migrationBuilder.DropTable(
                name: "employment_types");

            migrationBuilder.DropTable(
                name: "genders");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "positions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "divisions");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
