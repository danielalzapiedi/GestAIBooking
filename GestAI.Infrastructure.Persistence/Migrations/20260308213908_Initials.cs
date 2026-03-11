using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OwnerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DefaultPropertyId = table.Column<int>(type: "int", nullable: true),
                    DefaultAccountId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaasPlanDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MaxProperties = table.Column<int>(type: "int", nullable: false),
                    MaxUnits = table.Column<int>(type: "int", nullable: false),
                    MaxUsers = table.Column<int>(type: "int", nullable: false),
                    IncludesReports = table.Column<bool>(type: "bit", nullable: false),
                    IncludesPublicPortal = table.Column<bool>(type: "bit", nullable: false),
                    IncludesOperations = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaasPlanDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CommercialName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DefaultCheckInTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    DefaultCheckOutTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "ARS"),
                    DepositPolicy = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DefaultDepositPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    CancellationPolicy = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TermsAndConditions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CheckInInstructions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PropertyRules = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CommercialContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CommercialContactPhone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommercialContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PublicSlug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PublicDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Properties_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CanManageBookings = table.Column<bool>(type: "bit", nullable: false),
                    CanManageGuests = table.Column<bool>(type: "bit", nullable: false),
                    CanManagePayments = table.Column<bool>(type: "bit", nullable: false),
                    CanViewReports = table.Column<bool>(type: "bit", nullable: false),
                    CanManageConfiguration = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    InvitedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountUsers_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountSubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    PlanDefinitionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSubscriptionPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountSubscriptionPlans_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountSubscriptionPlans_SaasPlanDefinitions_PlanDefinitionId",
                        column: x => x.PlanDefinitionId,
                        principalTable: "SaasPlanDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    PropertyId = table.Column<int>(type: "int", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Guests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DocumentType = table.Column<int>(type: "int", nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guests_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "General"),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageTemplates_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CapacityAdults = table.Column<int>(type: "int", nullable: false),
                    CapacityChildren = table.Column<int>(type: "int", nullable: false),
                    BaseRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    TotalCapacity = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OperationalStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Units_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlockedDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    DateTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedDates_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlockedDates_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExternalChannelConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ChannelType = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImportCalendarUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ExportToken = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AutoSyncEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSyncStatus = table.Column<int>(type: "int", nullable: false),
                    LastSyncMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalChannelConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalChannelConnections_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalChannelConnections_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsCumulative = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    DateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    DateTo = table.Column<DateOnly>(type: "date", nullable: false),
                    ValueType = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinNights = table.Column<int>(type: "int", nullable: true),
                    MaxNights = table.Column<int>(type: "int", nullable: true),
                    BookingWindowDaysMin = table.Column<int>(type: "int", nullable: true),
                    BookingWindowDaysMax = table.Column<int>(type: "int", nullable: true),
                    AllowedCheckInDays = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    AllowedCheckOutDays = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promotions_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Promotions_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RatePlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BaseNightlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeekendAdjustmentEnabled = table.Column<bool>(type: "bit", nullable: false),
                    WeekendAdjustmentType = table.Column<int>(type: "int", nullable: false),
                    WeekendAdjustmentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RatePlans_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RatePlans_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExternalSyncLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalChannelConnectionId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProcessedEvents = table.Column<int>(type: "int", nullable: false),
                    ImportedEvents = table.Column<int>(type: "int", nullable: false),
                    UpdatedEvents = table.Column<int>(type: "int", nullable: false),
                    CancelledEvents = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalSyncLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalSyncLogs_ExternalChannelConnections_ExternalChannelConnectionId",
                        column: x => x.ExternalChannelConnectionId,
                        principalTable: "ExternalChannelConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExternalSyncLogs_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalSyncLogs_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DateRangeRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RatePlanId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    DateTo = table.Column<DateOnly>(type: "date", nullable: false),
                    AdjustmentType = table.Column<int>(type: "int", nullable: false),
                    AdjustmentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateRangeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DateRangeRates_RatePlans_RatePlanId",
                        column: x => x.RatePlanId,
                        principalTable: "RatePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonalRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RatePlanId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    StartMonth = table.Column<int>(type: "int", nullable: false),
                    StartDay = table.Column<int>(type: "int", nullable: false),
                    EndMonth = table.Column<int>(type: "int", nullable: false),
                    EndDay = table.Column<int>(type: "int", nullable: false),
                    AdjustmentType = table.Column<int>(type: "int", nullable: false),
                    AdjustmentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonalRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonalRates_RatePlans_RatePlanId",
                        column: x => x.RatePlanId,
                        principalTable: "RatePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ChangedByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingEvents_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    BookingCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Source = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OperationalStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Adults = table.Column<int>(type: "int", nullable: false),
                    Children = table.Column<int>(type: "int", nullable: false),
                    FinalGuestsCount = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    SuggestedNightlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    PromotionsAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    ExpectedDepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    DepositDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DepositVerified = table.Column<bool>(type: "bit", nullable: false),
                    DocumentationVerified = table.Column<bool>(type: "bit", nullable: false),
                    ManualPriceOverride = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedFromQuote = table.Column<bool>(type: "bit", nullable: false),
                    SavedQuoteId = table.Column<int>(type: "int", nullable: true),
                    AppliedPromotionNames = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GuestVisibleNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CancellationPolicyApplied = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ActualCheckInTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ActualCheckOutTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    CheckInNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CheckOutNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bookings_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExternalCalendarEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalChannelConnectionId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ExternalEventUid = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceChannel = table.Column<int>(type: "int", nullable: false),
                    RawData = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    SyncHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalCalendarEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalCalendarEvents_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ExternalCalendarEvents_ExternalChannelConnections_ExternalChannelConnectionId",
                        column: x => x.ExternalChannelConnectionId,
                        principalTable: "ExternalChannelConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExternalCalendarEvents_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalCalendarEvents_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperationalTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ResponsibleName = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationalTasks_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationalTasks_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperationalTasks_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SavedQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    PublicToken = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Adults = table.Column<int>(type: "int", nullable: false),
                    Children = table.Column<int>(type: "int", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PromotionsAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SuggestedDepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AppliedPromotionNames = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GuestEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GuestPhone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBookingId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedQuotes_Bookings_CreatedBookingId",
                        column: x => x.CreatedBookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavedQuotes_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedQuotes_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_OwnerUserId_Name",
                table: "Accounts",
                columns: new[] { "OwnerUserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountSubscriptionPlans_AccountId_IsActive",
                table: "AccountSubscriptionPlans",
                columns: new[] { "AccountId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountSubscriptionPlans_PlanDefinitionId",
                table: "AccountSubscriptionPlans",
                column: "PlanDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountUsers_AccountId_UserId",
                table: "AccountUsers",
                columns: new[] { "AccountId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountUsers_UserId",
                table: "AccountUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AccountId",
                table: "AuditLogs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_PropertyId_CreatedAtUtc",
                table: "AuditLogs",
                columns: new[] { "PropertyId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedDates_PropertyId_UnitId_DateFrom_DateTo",
                table: "BlockedDates",
                columns: new[] { "PropertyId", "UnitId", "DateFrom", "DateTo" });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedDates_UnitId",
                table: "BlockedDates",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingEvents_BookingId_ChangedAtUtc",
                table: "BookingEvents",
                columns: new[] { "BookingId", "ChangedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingEvents_PropertyId",
                table: "BookingEvents",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingCode",
                table: "Bookings",
                column: "BookingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_GuestId",
                table: "Bookings",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PropertyId_UnitId_CheckInDate_CheckOutDate",
                table: "Bookings",
                columns: new[] { "PropertyId", "UnitId", "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SavedQuoteId",
                table: "Bookings",
                column: "SavedQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UnitId",
                table: "Bookings",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DateRangeRates_RatePlanId_DateFrom_DateTo",
                table: "DateRangeRates",
                columns: new[] { "RatePlanId", "DateFrom", "DateTo" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCalendarEvents_BookingId",
                table: "ExternalCalendarEvents",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCalendarEvents_ExternalChannelConnectionId_ExternalEventUid",
                table: "ExternalCalendarEvents",
                columns: new[] { "ExternalChannelConnectionId", "ExternalEventUid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCalendarEvents_PropertyId_UnitId_StartDate_EndDate",
                table: "ExternalCalendarEvents",
                columns: new[] { "PropertyId", "UnitId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCalendarEvents_UnitId",
                table: "ExternalCalendarEvents",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalChannelConnections_ExportToken",
                table: "ExternalChannelConnections",
                column: "ExportToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalChannelConnections_PropertyId_UnitId_ChannelType_ImportCalendarUrl",
                table: "ExternalChannelConnections",
                columns: new[] { "PropertyId", "UnitId", "ChannelType", "ImportCalendarUrl" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalChannelConnections_UnitId",
                table: "ExternalChannelConnections",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSyncLogs_ExternalChannelConnectionId_CreatedAtUtc",
                table: "ExternalSyncLogs",
                columns: new[] { "ExternalChannelConnectionId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSyncLogs_PropertyId",
                table: "ExternalSyncLogs",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSyncLogs_UnitId",
                table: "ExternalSyncLogs",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_PropertyId_Email",
                table: "Guests",
                columns: new[] { "PropertyId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Guests_PropertyId_FullName",
                table: "Guests",
                columns: new[] { "PropertyId", "FullName" });

            migrationBuilder.CreateIndex(
                name: "IX_Guests_PropertyId_Phone",
                table: "Guests",
                columns: new[] { "PropertyId", "Phone" });

            migrationBuilder.CreateIndex(
                name: "IX_MessageTemplates_PropertyId_Type_Name",
                table: "MessageTemplates",
                columns: new[] { "PropertyId", "Type", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_OperationalTasks_BookingId",
                table: "OperationalTasks",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalTasks_PropertyId_ScheduledDate_Status",
                table: "OperationalTasks",
                columns: new[] { "PropertyId", "ScheduledDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OperationalTasks_UnitId",
                table: "OperationalTasks",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PropertyId_BookingId_Date",
                table: "Payments",
                columns: new[] { "PropertyId", "BookingId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_PropertyId_UnitId_IsActive_IsDeleted_DateFrom_DateTo",
                table: "Promotions",
                columns: new[] { "PropertyId", "UnitId", "IsActive", "IsDeleted", "DateFrom", "DateTo" });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_UnitId",
                table: "Promotions",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_AccountId_Name",
                table: "Properties",
                columns: new[] { "AccountId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_PublicSlug",
                table: "Properties",
                column: "PublicSlug",
                unique: true,
                filter: "[PublicSlug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RatePlans_PropertyId_UnitId_Name",
                table: "RatePlans",
                columns: new[] { "PropertyId", "UnitId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RatePlans_UnitId",
                table: "RatePlans",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SaasPlanDefinitions_Code",
                table: "SaasPlanDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedQuotes_CreatedBookingId",
                table: "SavedQuotes",
                column: "CreatedBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedQuotes_PropertyId_Status_CreatedAtUtc",
                table: "SavedQuotes",
                columns: new[] { "PropertyId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SavedQuotes_PublicToken",
                table: "SavedQuotes",
                column: "PublicToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedQuotes_UnitId",
                table: "SavedQuotes",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalRates_RatePlanId",
                table: "SeasonalRates",
                column: "RatePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_PropertyId_Name",
                table: "Units",
                columns: new[] { "PropertyId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingEvents_Bookings_BookingId",
                table: "BookingEvents",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_SavedQuotes_SavedQuoteId",
                table: "Bookings",
                column: "SavedQuoteId",
                principalTable: "SavedQuotes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Accounts_AccountId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Properties_PropertyId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Guests_Properties_PropertyId",
                table: "Guests");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedQuotes_Properties_PropertyId",
                table: "SavedQuotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Properties_PropertyId",
                table: "Units");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Units_UnitId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedQuotes_Units_UnitId",
                table: "SavedQuotes");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedQuotes_Bookings_CreatedBookingId",
                table: "SavedQuotes");

            migrationBuilder.DropTable(
                name: "AccountSubscriptionPlans");

            migrationBuilder.DropTable(
                name: "AccountUsers");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BlockedDates");

            migrationBuilder.DropTable(
                name: "BookingEvents");

            migrationBuilder.DropTable(
                name: "DateRangeRates");

            migrationBuilder.DropTable(
                name: "ExternalCalendarEvents");

            migrationBuilder.DropTable(
                name: "ExternalSyncLogs");

            migrationBuilder.DropTable(
                name: "MessageTemplates");

            migrationBuilder.DropTable(
                name: "OperationalTasks");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "SeasonalRates");

            migrationBuilder.DropTable(
                name: "SaasPlanDefinitions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ExternalChannelConnections");

            migrationBuilder.DropTable(
                name: "RatePlans");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Properties");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Guests");

            migrationBuilder.DropTable(
                name: "SavedQuotes");
        }
    }
}
