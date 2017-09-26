using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace GalleryServer.Data.Migrations
{
    public partial class v500 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // We want to create everything for new DB's but only a subset when we're upgrading from 4.X. For now let's focus on brand new installations.
            // We may never have to worry about upgrades.
            EnsureIdentitySchema(migrationBuilder);

            //EnsureGallerySchema(migrationBuilder);
        }

        private static void EnsureIdentitySchema(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_AspNetRoles", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_AspNetUsers", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                    table.PrimaryKey("PK_AspNetUserLogins", x => new {x.LoginProvider, x.ProviderKey});
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
                    table.PrimaryKey("PK_AspNetUserRoles", x => new {x.UserId, x.RoleId});
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
                    table.PrimaryKey("PK_AspNetUserTokens", x => new {x.UserId, x.LoginProvider, x.Name});
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
        }

        private static void EnsureGallerySchema(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gsp");

            migrationBuilder.CreateTable(
                name: "AppSetting",
                schema: "gsp",
                columns: table => new
                {
                    AppSettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SettingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_AppSetting", x => x.AppSettingId); });

            migrationBuilder.CreateTable(
                name: "Gallery",
                schema: "gsp",
                columns: table => new
                {
                    GalleryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsTemplate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Gallery", x => x.GalleryId); });

            migrationBuilder.CreateTable(
                name: "GalleryControlSetting",
                schema: "gsp",
                columns: table => new
                {
                    GalleryControlSettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ControlId = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    SettingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_GalleryControlSetting", x => x.GalleryControlSettingId); });

            migrationBuilder.CreateTable(
                name: "MediaTemplate",
                schema: "gsp",
                columns: table => new
                {
                    MediaTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BrowserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HtmlTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ScriptTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_MediaTemplate", x => x.MediaTemplateId); });

            migrationBuilder.CreateTable(
                name: "MimeType",
                schema: "gsp",
                columns: table => new
                {
                    MimeTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BrowserMimeTypeValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MimeTypeValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_MimeType", x => x.MimeTypeId); });

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "gsp",
                columns: table => new
                {
                    RoleName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AllowAddChildAlbum = table.Column<bool>(type: "bit", nullable: false),
                    AllowAddMediaObject = table.Column<bool>(type: "bit", nullable: false),
                    AllowAdministerGallery = table.Column<bool>(type: "bit", nullable: false),
                    AllowAdministerSite = table.Column<bool>(type: "bit", nullable: false),
                    AllowDeleteChildAlbum = table.Column<bool>(type: "bit", nullable: false),
                    AllowDeleteMediaObject = table.Column<bool>(type: "bit", nullable: false),
                    AllowEditAlbum = table.Column<bool>(type: "bit", nullable: false),
                    AllowEditMediaObject = table.Column<bool>(type: "bit", nullable: false),
                    AllowSynchronize = table.Column<bool>(type: "bit", nullable: false),
                    AllowViewAlbumsAndObjects = table.Column<bool>(type: "bit", nullable: false),
                    AllowViewOriginalImage = table.Column<bool>(type: "bit", nullable: false),
                    HideWatermark = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Role", x => x.RoleName); });

            migrationBuilder.CreateTable(
                name: "Synchronize",
                schema: "gsp",
                columns: table => new
                {
                    FKGalleryId = table.Column<int>(type: "int", nullable: false),
                    CurrentFileIndex = table.Column<int>(type: "int", nullable: false),
                    SynchId = table.Column<string>(type: "nvarchar(46)", maxLength: 46, nullable: false),
                    SynchState = table.Column<int>(type: "int", nullable: false),
                    TotalFiles = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Synchronize", x => x.FKGalleryId); });

            migrationBuilder.CreateTable(
                name: "Tag",
                schema: "gsp",
                columns: table => new
                {
                    TagName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Tag", x => x.TagName); });

            migrationBuilder.CreateTable(
                name: "UiTemplate",
                schema: "gsp",
                columns: table => new
                {
                    UiTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FKGalleryId = table.Column<int>(type: "int", nullable: false),
                    HtmlTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ScriptTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_UiTemplate", x => x.UiTemplateId); });

            migrationBuilder.CreateTable(
                name: "Album",
                schema: "gsp",
                columns: table => new
                {
                    AlbumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false),
                    DateLastModified = table.Column<DateTime>(type: "datetime", nullable: false),
                    DirectoryName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FKAlbumParentId = table.Column<int>(type: "int", nullable: true),
                    FKGalleryId = table.Column<int>(type: "int", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OwnedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OwnerRoleName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    SortAscending = table.Column<bool>(type: "bit", nullable: false),
                    SortByMetaName = table.Column<int>(type: "int", nullable: false),
                    ThumbnailMediaObjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Album", x => x.AlbumId);
                    table.ForeignKey(
                        name: "FK_Album_Album_FKAlbumParentId",
                        column: x => x.FKAlbumParentId,
                        principalSchema: "gsp",
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Album_Gallery_FKGalleryId",
                        column: x => x.FKGalleryId,
                        principalSchema: "gsp",
                        principalTable: "Gallery",
                        principalColumn: "GalleryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Event",
                schema: "gsp",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Cookies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    ExSource = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ExStackTrace = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExTargetSite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExType = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FKGalleryId = table.Column<int>(type: "int", nullable: false),
                    FormVariables = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InnerExData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InnerExMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    InnerExSource = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    InnerExStackTrace = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InnerExTargetSite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InnerExType = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ServerVariables = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionVariables = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeStampUtc = table.Column<DateTime>(type: "datetime", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Event_Gallery_FKGalleryId",
                        column: x => x.FKGalleryId,
                        principalSchema: "gsp",
                        principalTable: "Gallery",
                        principalColumn: "GalleryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GallerySetting",
                schema: "gsp",
                columns: table => new
                {
                    GallerySettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FKGalleryId = table.Column<int>(type: "int", nullable: false),
                    SettingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GallerySetting", x => x.GallerySettingId);
                    table.ForeignKey(
                        name: "FK_GallerySetting_Gallery_FKGalleryId",
                        column: x => x.FKGalleryId,
                        principalSchema: "gsp",
                        principalTable: "Gallery",
                        principalColumn: "GalleryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGalleryProfile",
                schema: "gsp",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FKGalleryId = table.Column<int>(type: "int", nullable: false),
                    SettingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGalleryProfile", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_UserGalleryProfile_Gallery_FKGalleryId",
                        column: x => x.FKGalleryId,
                        principalSchema: "gsp",
                        principalTable: "Gallery",
                        principalColumn: "GalleryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MimeTypeGallery",
                schema: "gsp",
                columns: table => new
                {
                    MimeTypeGalleryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FKGalleryId = table.Column<int>(type: "int", nullable: false),
                    FKMimeTypeId = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MimeTypeGallery", x => x.MimeTypeGalleryId);
                    table.ForeignKey(
                        name: "FK_MimeTypeGallery_Gallery_FKGalleryId",
                        column: x => x.FKGalleryId,
                        principalSchema: "gsp",
                        principalTable: "Gallery",
                        principalColumn: "GalleryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MimeTypeGallery_MimeType_FKMimeTypeId",
                        column: x => x.FKMimeTypeId,
                        principalSchema: "gsp",
                        principalTable: "MimeType",
                        principalColumn: "MimeTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaObject",
                schema: "gsp",
                columns: table => new
                {
                    MediaObjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false),
                    DateLastModified = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExternalHtmlSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalType = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    FKAlbumId = table.Column<int>(type: "int", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OptimizedFilename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OptimizedHeight = table.Column<int>(type: "int", nullable: false),
                    OptimizedSizeKB = table.Column<int>(type: "int", nullable: false),
                    OptimizedWidth = table.Column<int>(type: "int", nullable: false),
                    OriginalFilename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OriginalHeight = table.Column<int>(type: "int", nullable: false),
                    OriginalSizeKB = table.Column<int>(type: "int", nullable: false),
                    OriginalWidth = table.Column<int>(type: "int", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    ThumbnailFilename = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThumbnailHeight = table.Column<int>(type: "int", nullable: false),
                    ThumbnailSizeKB = table.Column<int>(type: "int", nullable: false),
                    ThumbnailWidth = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaObject", x => x.MediaObjectId);
                    table.ForeignKey(
                        name: "FK_MediaObject_Album_FKAlbumId",
                        column: x => x.FKAlbumId,
                        principalSchema: "gsp",
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleAlbum",
                schema: "gsp",
                columns: table => new
                {
                    FKRoleName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FKAlbumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAlbum", x => new {x.FKRoleName, x.FKAlbumId});
                    table.UniqueConstraint("AK_RoleAlbum_FKAlbumId_FKRoleName", x => new {x.FKAlbumId, x.FKRoleName});
                    table.ForeignKey(
                        name: "FK_RoleAlbum_Album_FKAlbumId",
                        column: x => x.FKAlbumId,
                        principalSchema: "gsp",
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleAlbum_Role_FKRoleName",
                        column: x => x.FKRoleName,
                        principalSchema: "gsp",
                        principalTable: "Role",
                        principalColumn: "RoleName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UiTemplateAlbum",
                schema: "gsp",
                columns: table => new
                {
                    FKUiTemplateId = table.Column<int>(type: "int", nullable: false),
                    FKAlbumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UiTemplateAlbum", x => new {x.FKUiTemplateId, x.FKAlbumId});
                    table.UniqueConstraint("AK_UiTemplateAlbum_FKAlbumId_FKUiTemplateId", x => new {x.FKAlbumId, x.FKUiTemplateId});
                    table.ForeignKey(
                        name: "FK_UiTemplateAlbum_Album_FKAlbumId",
                        column: x => x.FKAlbumId,
                        principalSchema: "gsp",
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UiTemplateAlbum_UiTemplate_FKUiTemplateId",
                        column: x => x.FKUiTemplateId,
                        principalSchema: "gsp",
                        principalTable: "UiTemplate",
                        principalColumn: "UiTemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaQueue",
                schema: "gsp",
                columns: table => new
                {
                    MediaQueueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ConversionType = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((0))"),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false),
                    DateConversionCompleted = table.Column<DateTime>(type: "datetime", nullable: true),
                    DateConversionStarted = table.Column<DateTime>(type: "datetime", nullable: true),
                    FKMediaObjectId = table.Column<int>(type: "int", nullable: false),
                    RotationAmount = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((0))"),
                    Status = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StatusDetail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaQueue", x => x.MediaQueueId);
                    table.ForeignKey(
                        name: "FK_MediaQueue_MediaObject_FKMediaObjectId",
                        column: x => x.FKMediaObjectId,
                        principalSchema: "gsp",
                        principalTable: "MediaObject",
                        principalColumn: "MediaObjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Metadata",
                schema: "gsp",
                columns: table => new
                {
                    MetadataId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FKAlbumId = table.Column<int>(type: "int", nullable: true),
                    FKMediaObjectId = table.Column<int>(type: "int", nullable: true),
                    MetaName = table.Column<int>(type: "int", nullable: false),
                    RawValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metadata", x => x.MetadataId);
                    table.ForeignKey(
                        name: "FK_Metadata_Album_FKAlbumId",
                        column: x => x.FKAlbumId,
                        principalSchema: "gsp",
                        principalTable: "Album",
                        principalColumn: "AlbumId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gsp.Metadata_gsp.MediaObject_FKMediaObjectId",
                        column: x => x.FKMediaObjectId,
                        principalSchema: "gsp",
                        principalTable: "MediaObject",
                        principalColumn: "MediaObjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetadataTag",
                schema: "gsp",
                columns: table => new
                {
                    FKMetadataId = table.Column<int>(type: "int", nullable: false),
                    FKTagName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FKGalleryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataTag", x => new {x.FKMetadataId, x.FKTagName});
                    table.ForeignKey(
                        name: "FK_MetadataTag_Metadata_FKMetadataId",
                        column: x => x.FKMetadataId,
                        principalSchema: "gsp",
                        principalTable: "Metadata",
                        principalColumn: "MetadataId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetadataTag_Tag_FKTagName",
                        column: x => x.FKTagName,
                        principalSchema: "gsp",
                        principalTable: "Tag",
                        principalColumn: "TagName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FKAlbumParentId",
                schema: "gsp",
                table: "Album",
                column: "FKAlbumParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FKGalleryId",
                schema: "gsp",
                table: "Album",
                column: "FKGalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_FKGalleryId",
                schema: "gsp",
                table: "Event",
                column: "FKGalleryId");

            migrationBuilder.CreateIndex(
                name: "UC_GalleryControlSetting_ControlId_SettingName",
                schema: "gsp",
                table: "GalleryControlSetting",
                columns: new[] {"ControlId", "SettingName"},
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FKGalleryId",
                schema: "gsp",
                table: "GallerySetting",
                column: "FKGalleryId");

            migrationBuilder.CreateIndex(
                name: "UC_GallerySetting_FKGalleryId_SettingName",
                schema: "gsp",
                table: "GallerySetting",
                columns: new[] {"FKGalleryId", "SettingName"},
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FKAlbumId",
                schema: "gsp",
                table: "MediaObject",
                column: "FKAlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_FKMediaObjectId",
                schema: "gsp",
                table: "MediaQueue",
                column: "FKMediaObjectId");

            migrationBuilder.CreateIndex(
                name: "UC_MediaTemplate_MimeType_BrowserId",
                schema: "gsp",
                table: "MediaTemplate",
                columns: new[] {"MimeType", "BrowserId"},
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FKAlbumId",
                schema: "gsp",
                table: "Metadata",
                column: "FKAlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_FKMediaObjectId",
                schema: "gsp",
                table: "Metadata",
                column: "FKMediaObjectId");

            migrationBuilder.CreateIndex(
                name: "UC_Metadata_MetaName_MetadataId",
                schema: "gsp",
                table: "Metadata",
                columns: new[] {"MetaName", "MetadataId"},
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FKMetadataId",
                schema: "gsp",
                table: "MetadataTag",
                column: "FKMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_FKTagName",
                schema: "gsp",
                table: "MetadataTag",
                column: "FKTagName");

            migrationBuilder.CreateIndex(
                name: "UC_MimeType_FileExtension",
                schema: "gsp",
                table: "MimeType",
                column: "FileExtension",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FKGalleryId",
                schema: "gsp",
                table: "MimeTypeGallery",
                column: "FKGalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_FKMimeTypeId",
                schema: "gsp",
                table: "MimeTypeGallery",
                column: "FKMimeTypeId");

            migrationBuilder.CreateIndex(
                name: "UC_MimeTypeGallery_FKGalleryId_FKMimeTypeId",
                schema: "gsp",
                table: "MimeTypeGallery",
                columns: new[] {"FKGalleryId", "FKMimeTypeId"},
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FKAlbumId",
                schema: "gsp",
                table: "RoleAlbum",
                column: "FKAlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_FKRoleName",
                schema: "gsp",
                table: "RoleAlbum",
                column: "FKRoleName");

            migrationBuilder.CreateIndex(
                name: "UC_UiTemplate_TemplateType_Name",
                schema: "gsp",
                table: "UiTemplate",
                columns: new[] {"TemplateType", "FKGalleryId", "Name"},
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FKAlbumId",
                schema: "gsp",
                table: "UiTemplateAlbum",
                column: "FKAlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_FKUiTemplateId",
                schema: "gsp",
                table: "UiTemplateAlbum",
                column: "FKUiTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FKGalleryId",
                schema: "gsp",
                table: "UserGalleryProfile",
                column: "FKGalleryId");

            migrationBuilder.CreateIndex(
                name: "UC_UserGalleryProfile_UserName_FKGalleryId_SettingName",
                schema: "gsp",
                table: "UserGalleryProfile",
                columns: new[] {"UserName", "FKGalleryId", "SettingName"},
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "AppSetting",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "Event",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "GalleryControlSetting",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "GallerySetting",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "MediaQueue",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "MediaTemplate",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "MetadataTag",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "MimeTypeGallery",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "RoleAlbum",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "Synchronize",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "UiTemplateAlbum",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "UserGalleryProfile",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Metadata",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "Tag",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "MimeType",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "UiTemplate",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "MediaObject",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "Album",
                schema: "gsp");

            migrationBuilder.DropTable(
                name: "Gallery",
                schema: "gsp");
        }
    }
}
