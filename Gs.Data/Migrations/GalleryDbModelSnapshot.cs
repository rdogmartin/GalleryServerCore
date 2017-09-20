﻿// <auto-generated />
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace GalleryServer.Data.Migrations
{
    [DbContext(typeof(GalleryDb))]
    partial class GalleryDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("GalleryServer.Data.AlbumDto", b =>
                {
                    b.Property<int>("AlbumId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("DateLastModified")
                        .HasColumnType("datetime");

                    b.Property<string>("DirectoryName")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int?>("FKAlbumParentId");

                    b.Property<int>("FKGalleryId");

                    b.Property<bool>("IsPrivate");

                    b.Property<string>("LastModifiedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("OwnedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("OwnerRoleName")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<int>("Seq");

                    b.Property<bool>("SortAscending");

                    b.Property<int>("SortByMetaName");

                    b.Property<int>("ThumbnailMediaObjectId");

                    b.HasKey("AlbumId");

                    b.HasIndex("FKAlbumParentId")
                        .HasName("IX_FKAlbumParentId");

                    b.HasIndex("FKGalleryId")
                        .HasName("IX_FKGalleryId");

                    b.ToTable("Album","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("GalleryServer.Data.AppSettingDto", b =>
                {
                    b.Property<int>("AppSettingId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("SettingName")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("SettingValue")
                        .IsRequired();

                    b.HasKey("AppSettingId");

                    b.ToTable("AppSetting","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.EventDto", b =>
                {
                    b.Property<int>("EventId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Cookies")
                        .IsRequired();

                    b.Property<string>("EventData")
                        .IsRequired();

                    b.Property<int>("EventType");

                    b.Property<string>("ExSource")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<string>("ExStackTrace")
                        .IsRequired();

                    b.Property<string>("ExTargetSite")
                        .IsRequired();

                    b.Property<string>("ExType")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<int>("FKGalleryId");

                    b.Property<string>("FormVariables")
                        .IsRequired();

                    b.Property<string>("InnerExData")
                        .IsRequired();

                    b.Property<string>("InnerExMessage")
                        .IsRequired()
                        .HasMaxLength(4000);

                    b.Property<string>("InnerExSource")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<string>("InnerExStackTrace")
                        .IsRequired();

                    b.Property<string>("InnerExTargetSite")
                        .IsRequired();

                    b.Property<string>("InnerExType")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(4000);

                    b.Property<string>("ServerVariables")
                        .IsRequired();

                    b.Property<string>("SessionVariables")
                        .IsRequired();

                    b.Property<DateTime>("TimeStampUtc")
                        .HasColumnType("datetime");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.HasKey("EventId");

                    b.HasIndex("FKGalleryId")
                        .HasName("IX_FKGalleryId");

                    b.ToTable("Event","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.GalleryControlSettingDto", b =>
                {
                    b.Property<int>("GalleryControlSettingId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ControlId")
                        .IsRequired()
                        .HasMaxLength(350);

                    b.Property<string>("SettingName")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("SettingValue")
                        .IsRequired();

                    b.HasKey("GalleryControlSettingId");

                    b.HasIndex("ControlId", "SettingName")
                        .IsUnique()
                        .HasName("UC_GalleryControlSetting_ControlId_SettingName");

                    b.ToTable("GalleryControlSetting","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.GalleryDto", b =>
                {
                    b.Property<int>("GalleryId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000);

                    b.Property<bool>("IsTemplate");

                    b.HasKey("GalleryId");

                    b.ToTable("Gallery","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.GallerySettingDto", b =>
                {
                    b.Property<int>("GallerySettingId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("FKGalleryId");

                    b.Property<string>("SettingName")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("SettingValue")
                        .IsRequired();

                    b.HasKey("GallerySettingId");

                    b.HasIndex("FKGalleryId")
                        .HasName("IX_FKGalleryId");

                    b.HasIndex("FKGalleryId", "SettingName")
                        .IsUnique()
                        .HasName("UC_GallerySetting_FKGalleryId_SettingName");

                    b.ToTable("GallerySetting","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.MediaObjectDto", b =>
                {
                    b.Property<int>("MediaObjectId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("DateLastModified")
                        .HasColumnType("datetime");

                    b.Property<string>("ExternalHtmlSource")
                        .IsRequired();

                    b.Property<string>("ExternalType")
                        .IsRequired()
                        .HasMaxLength(15);

                    b.Property<int>("FKAlbumId");

                    b.Property<bool>("IsPrivate");

                    b.Property<string>("LastModifiedBy")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("OptimizedFilename")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("OptimizedHeight");

                    b.Property<int>("OptimizedSizeKB");

                    b.Property<int>("OptimizedWidth");

                    b.Property<string>("OriginalFilename")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("OriginalHeight");

                    b.Property<int>("OriginalSizeKB");

                    b.Property<int>("OriginalWidth");

                    b.Property<int>("Seq");

                    b.Property<string>("ThumbnailFilename")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("ThumbnailHeight");

                    b.Property<int>("ThumbnailSizeKB");

                    b.Property<int>("ThumbnailWidth");

                    b.HasKey("MediaObjectId");

                    b.HasIndex("FKAlbumId")
                        .HasName("IX_FKAlbumId");

                    b.ToTable("MediaObject","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.MediaQueueDto", b =>
                {
                    b.Property<int>("MediaQueueId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ConversionType")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("((0))");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("DateConversionCompleted")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("DateConversionStarted")
                        .HasColumnType("datetime");

                    b.Property<int>("FKMediaObjectId");

                    b.Property<int>("RotationAmount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("((0))");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("StatusDetail")
                        .IsRequired();

                    b.HasKey("MediaQueueId");

                    b.HasIndex("FKMediaObjectId")
                        .HasName("IX_FKMediaObjectId");

                    b.ToTable("MediaQueue","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.MediaTemplateDto", b =>
                {
                    b.Property<int>("MediaTemplateId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BrowserId")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("HtmlTemplate")
                        .IsRequired();

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("ScriptTemplate")
                        .IsRequired();

                    b.HasKey("MediaTemplateId");

                    b.HasIndex("MimeType", "BrowserId")
                        .IsUnique()
                        .HasName("UC_MediaTemplate_MimeType_BrowserId");

                    b.ToTable("MediaTemplate","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.MetadataDto", b =>
                {
                    b.Property<int>("MetadataId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("FKAlbumId");

                    b.Property<int?>("FKMediaObjectId");

                    b.Property<int>("MetaName");

                    b.Property<string>("RawValue");

                    b.Property<string>("Value")
                        .IsRequired();

                    b.HasKey("MetadataId");

                    b.HasIndex("FKAlbumId")
                        .HasName("IX_FKAlbumId");

                    b.HasIndex("FKMediaObjectId")
                        .HasName("IX_FKMediaObjectId");

                    b.HasIndex("MetaName", "MetadataId")
                        .IsUnique()
                        .HasName("UC_Metadata_MetaName_MetadataId");

                    b.ToTable("Metadata","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.MetadataTagDto", b =>
                {
                    b.Property<int>("FKMetadataId");

                    b.Property<string>("FKTagName")
                        .HasMaxLength(100);

                    b.Property<int>("FKGalleryId");

                    b.HasKey("FKMetadataId", "FKTagName");

                    b.HasIndex("FKMetadataId")
                        .HasName("IX_FKMetadataId");

                    b.HasIndex("FKTagName")
                        .HasName("IX_FKTagName");

                    b.ToTable("MetadataTag","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.MimeTypeDto", b =>
                {
                    b.Property<int>("MimeTypeId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BrowserMimeTypeValue")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("FileExtension")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<string>("MimeTypeValue")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.HasKey("MimeTypeId");

                    b.HasIndex("FileExtension")
                        .IsUnique()
                        .HasName("UC_MimeType_FileExtension");

                    b.ToTable("MimeType","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.MimeTypeGalleryDto", b =>
                {
                    b.Property<int>("MimeTypeGalleryId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("FKGalleryId");

                    b.Property<int>("FKMimeTypeId");

                    b.Property<bool>("IsEnabled");

                    b.HasKey("MimeTypeGalleryId");

                    b.HasIndex("FKGalleryId")
                        .HasName("IX_FKGalleryId");

                    b.HasIndex("FKMimeTypeId")
                        .HasName("IX_FKMimeTypeId");

                    b.HasIndex("FKGalleryId", "FKMimeTypeId")
                        .IsUnique()
                        .HasName("UC_MimeTypeGallery_FKGalleryId_FKMimeTypeId");

                    b.ToTable("MimeTypeGallery","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.RoleAlbumDto", b =>
                {
                    b.Property<string>("FKRoleName")
                        .HasMaxLength(256);

                    b.Property<int>("FKAlbumId");

                    b.HasKey("FKRoleName", "FKAlbumId");

                    b.HasAlternateKey("FKAlbumId", "FKRoleName");

                    b.HasIndex("FKAlbumId")
                        .HasName("IX_FKAlbumId");

                    b.HasIndex("FKRoleName")
                        .HasName("IX_FKRoleName");

                    b.ToTable("RoleAlbum","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.RoleDto", b =>
                {
                    b.Property<string>("RoleName")
                        .HasMaxLength(256);

                    b.Property<bool>("AllowAddChildAlbum");

                    b.Property<bool>("AllowAddMediaObject");

                    b.Property<bool>("AllowAdministerGallery");

                    b.Property<bool>("AllowAdministerSite");

                    b.Property<bool>("AllowDeleteChildAlbum");

                    b.Property<bool>("AllowDeleteMediaObject");

                    b.Property<bool>("AllowEditAlbum");

                    b.Property<bool>("AllowEditMediaObject");

                    b.Property<bool>("AllowSynchronize");

                    b.Property<bool>("AllowViewAlbumsAndObjects");

                    b.Property<bool>("AllowViewOriginalImage");

                    b.Property<bool>("HideWatermark");

                    b.HasKey("RoleName");

                    b.ToTable("Role","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.SynchronizeDto", b =>
                {
                    b.Property<int>("FKGalleryId");

                    b.Property<int>("CurrentFileIndex");

                    b.Property<string>("SynchId")
                        .IsRequired()
                        .HasMaxLength(46);

                    b.Property<int>("SynchState");

                    b.Property<int>("TotalFiles");

                    b.HasKey("FKGalleryId");

                    b.ToTable("Synchronize","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.TagDto", b =>
                {
                    b.Property<string>("TagName")
                        .HasMaxLength(100);

                    b.HasKey("TagName");

                    b.ToTable("Tag","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.UiTemplateAlbumDto", b =>
                {
                    b.Property<int>("FKUiTemplateId");

                    b.Property<int>("FKAlbumId");

                    b.HasKey("FKUiTemplateId", "FKAlbumId");

                    b.HasAlternateKey("FKAlbumId", "FKUiTemplateId");

                    b.HasIndex("FKAlbumId")
                        .HasName("IX_FKAlbumId");

                    b.HasIndex("FKUiTemplateId")
                        .HasName("IX_FKUiTemplateId");

                    b.ToTable("UiTemplateAlbum","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.UiTemplateDto", b =>
                {
                    b.Property<int>("UiTemplateId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<int>("FKGalleryId");

                    b.Property<string>("HtmlTemplate")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("ScriptTemplate")
                        .IsRequired();

                    b.Property<int>("TemplateType");

                    b.HasKey("UiTemplateId");

                    b.HasIndex("TemplateType", "FKGalleryId", "Name")
                        .IsUnique()
                        .HasName("UC_UiTemplate_TemplateType_Name");

                    b.ToTable("UiTemplate","gsp");
                });

            modelBuilder.Entity("GalleryServer.Data.UserGalleryProfileDto", b =>
                {
                    b.Property<int>("ProfileId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("FKGalleryId");

                    b.Property<string>("SettingName")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("SettingValue")
                        .IsRequired();

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.HasKey("ProfileId");

                    b.HasIndex("FKGalleryId")
                        .HasName("IX_FKGalleryId");

                    b.HasIndex("UserName", "FKGalleryId", "SettingName")
                        .IsUnique()
                        .HasName("UC_UserGalleryProfile_UserName_FKGalleryId_SettingName");

                    b.ToTable("UserGalleryProfile","gsp");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("GalleryServer.Data.AlbumDto", b =>
                {
                    b.HasOne("GalleryServer.Data.AlbumDto", "AlbumParent")
                        .WithMany()
                        .HasForeignKey("FKAlbumParentId");

                    b.HasOne("GalleryServer.Data.GalleryDto", "Gallery")
                        .WithMany()
                        .HasForeignKey("FKGalleryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GalleryServer.Data.EventDto", b =>
                {
                    b.HasOne("GalleryServer.Data.GalleryDto", "Gallery")
                        .WithMany()
                        .HasForeignKey("FKGalleryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GalleryServer.Data.GallerySettingDto", b =>
                {
                    b.HasOne("GalleryServer.Data.GalleryDto", "Gallery")
                        .WithMany()
                        .HasForeignKey("FKGalleryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GalleryServer.Data.MediaObjectDto", b =>
                {
                    b.HasOne("GalleryServer.Data.AlbumDto", "Album")
                        .WithMany()
                        .HasForeignKey("FKAlbumId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GalleryServer.Data.MediaQueueDto", b =>
                {
                    b.HasOne("GalleryServer.Data.MediaObjectDto", "MediaObject")
                        .WithMany()
                        .HasForeignKey("FKMediaObjectId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GalleryServer.Data.MetadataDto", b =>
                {
                    b.HasOne("GalleryServer.Data.AlbumDto", "Album")
                        .WithMany("Metadata")
                        .HasForeignKey("FKAlbumId");

                    b.HasOne("GalleryServer.Data.MediaObjectDto", "MediaObject")
                        .WithMany("Metadata")
                        .HasForeignKey("FKMediaObjectId")
                        .HasConstraintName("FK_gsp.Metadata_gsp.MediaObject_FKMediaObjectId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GalleryServer.Data.MetadataTagDto", b =>
                {
                    b.HasOne("GalleryServer.Data.MetadataDto", "Metadata")
                        .WithMany("MetadataTags")
                        .HasForeignKey("FKMetadataId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GalleryServer.Data.TagDto", "Tag")
                        .WithMany("MetadataTags")
                        .HasForeignKey("FKTagName")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GalleryServer.Data.MimeTypeGalleryDto", b =>
                {
                    b.HasOne("GalleryServer.Data.GalleryDto", "Gallery")
                        .WithMany()
                        .HasForeignKey("FKGalleryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GalleryServer.Data.MimeTypeDto", "MimeType")
                        .WithMany("MimeTypeGalleries")
                        .HasForeignKey("FKMimeTypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GalleryServer.Data.RoleAlbumDto", b =>
                {
                    b.HasOne("GalleryServer.Data.AlbumDto", "Album")
                        .WithMany()
                        .HasForeignKey("FKAlbumId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GalleryServer.Data.RoleDto", "Role")
                        .WithMany("RoleAlbums")
                        .HasForeignKey("FKRoleName")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GalleryServer.Data.UiTemplateAlbumDto", b =>
                {
                    b.HasOne("GalleryServer.Data.AlbumDto", "Album")
                        .WithMany("UiTemplates")
                        .HasForeignKey("FKAlbumId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GalleryServer.Data.UiTemplateDto", "UiTemplate")
                        .WithMany("TemplateAlbums")
                        .HasForeignKey("FKUiTemplateId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GalleryServer.Data.UserGalleryProfileDto", b =>
                {
                    b.HasOne("GalleryServer.Data.GalleryDto", "Gallery")
                        .WithMany()
                        .HasForeignKey("FKGalleryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("GalleryServer.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("GalleryServer.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GalleryServer.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("GalleryServer.Data.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
