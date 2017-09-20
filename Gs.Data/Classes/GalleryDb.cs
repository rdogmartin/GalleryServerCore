using Microsoft.EntityFrameworkCore;
using GalleryServer.Business;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GalleryServer.Data
{
    /// <inheritdoc />
    public class GalleryDb : IdentityDbContext<ApplicationUser>
    {
        public GalleryDb(DbContextOptions<GalleryDb> options) : base(options)
        {
        }

        public virtual DbSet<AlbumDto> Albums { get; set; }
        public virtual DbSet<EventDto> Events { get; set; }
        public virtual DbSet<AppSettingDto> AppSettings { get; set; }
        public virtual DbSet<MediaTemplateDto> MediaTemplates { get; set; }
        public virtual DbSet<GalleryControlSettingDto> GalleryControlSettings { get; set; }
        public virtual DbSet<GalleryDto> Galleries { get; set; }
        public virtual DbSet<GallerySettingDto> GallerySettings { get; set; }
        public virtual DbSet<MediaObjectDto> MediaObjects { get; set; }
        public virtual DbSet<MetadataDto> Metadatas { get; set; }
        public virtual DbSet<MimeTypeDto> MimeTypes { get; set; }
        public virtual DbSet<MimeTypeGalleryDto> MimeTypeGalleries { get; set; }
        public virtual DbSet<RoleDto> GalleryRoles { get; set; } // Was Roles in 4.X, but this conflicted with inherited Roles property from IdentityDbContext
        public virtual DbSet<SynchronizeDto> Synchronizes { get; set; }
        public virtual DbSet<RoleAlbumDto> RoleAlbums { get; set; }
        public virtual DbSet<UserGalleryProfileDto> UserGalleryProfiles { get; set; }
        public virtual DbSet<MediaQueueDto> MediaQueues { get; set; }
        public virtual DbSet<UiTemplateDto> UiTemplates { get; set; }
        public virtual DbSet<UiTemplateAlbumDto> UiTemplateAlbums { get; set; }
        public virtual DbSet<TagDto> Tags { get; set; }
        public virtual DbSet<MetadataTagDto> MetadataTags { get; set; }

        /// <summary>
        /// Gets the version of the current data schema.
        /// </summary>
        /// <value>The data schema version.</value>
        public static GalleryDataSchemaVersion DataSchemaVersion
        {
            get
            {
                return GalleryDataSchemaVersion.V4_4_0;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=(local);Database=GalleryCoreDb;Trusted_Connection=True;");
            }
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // Set up relationship to enforce cascade delete between media objects and their metadata (by default it is set to NO ACTION)
            modelBuilder.Entity<MetadataDto>()
                .HasOne(t => t.MediaObject)
                .WithMany(t => t.Metadata)
                .HasForeignKey(t => t.FKMediaObjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_gsp.Metadata_gsp.MediaObject_FKMediaObjectId");

            // Can't create a cascade delete between albums and their metadata, as we get this error when we try:
            // "Introducing FOREIGN KEY constraint 'FK_dbo.gsp_Metadata_dbo.gsp_Album_FKAlbumId' on table 'gsp_Metadata' may cause cycles or multiple cascade paths."
            // We just have to make sure the app deletes 
            //modelBuilder.Entity<MetadataDto>()
            //  .HasOptional(t => t.Album)
            //  .WithMany(t => t.Metadata)
            //  .HasForeignKey(t => t.FKAlbumId)
            //  .WillCascadeOnDelete(true);

            modelBuilder.Entity<AlbumDto>(entity =>
            {
                entity.HasIndex(e => e.FKAlbumParentId)
                    .HasName("IX_FKAlbumParentId");

                entity.HasIndex(e => e.FKGalleryId)
                    .HasName("IX_FKGalleryId");

                //entity.HasOne(d => d.FkalbumParent)
                //    .WithMany(p => p.InverseFkalbumParent)
                //    .HasForeignKey(d => d.FkalbumParentId)
                //    .HasConstraintName("FK_gsp.Album_gsp.Album_FKAlbumParentId");

                //entity.HasOne(d => d.Fkgallery)
                //    .WithMany(p => p.Album)
                //    .HasForeignKey(d => d.FkgalleryId)
                //    .HasConstraintName("FK_gsp.Album_gsp.Gallery_FKGalleryId");
            });

            modelBuilder.Entity<EventDto>(entity =>
            {
                entity.HasIndex(e => e.FKGalleryId)
                    .HasName("IX_FKGalleryId");

                //entity.HasOne(d => d.Fkgallery)
                //    .WithMany(p => p.Event)
                //    .HasForeignKey(d => d.FkgalleryId)
                //    .HasConstraintName("FK_gsp.Event_gsp.Gallery_FKGalleryId");
            });

            modelBuilder.Entity<GalleryControlSettingDto>(entity =>
            {
                entity.HasIndex(e => new { e.ControlId, e.SettingName })
                    .HasName("UC_GalleryControlSetting_ControlId_SettingName")
                    .IsUnique();
            });

            modelBuilder.Entity<GallerySettingDto>(entity =>
            {
                entity.HasIndex(e => e.FKGalleryId)
                    .HasName("IX_FKGalleryId");

                entity.HasIndex(e => new { e.FKGalleryId, e.SettingName })
                    .HasName("UC_GallerySetting_FKGalleryId_SettingName")
                    .IsUnique();

                //entity.HasOne(d => d.Fkgallery)
                //    .WithMany(p => p.GallerySetting)
                //    .HasForeignKey(d => d.FkgalleryId)
                //    .HasConstraintName("FK_gsp.GallerySetting_gsp.Gallery_FKGalleryId");
            });

            modelBuilder.Entity<MediaObjectDto>(entity =>
            {
                entity.HasIndex(e => e.FKAlbumId)
                    .HasName("IX_FKAlbumId");

                //entity.HasOne(d => d.Fkalbum)
                //    .WithMany(p => p.MediaObject)
                //    .HasForeignKey(d => d.FkalbumId)
                //    .HasConstraintName("FK_gsp.MediaObject_gsp.Album_FKAlbumId");
            });

            modelBuilder.Entity<MediaQueueDto>(entity =>
            {
                entity.HasIndex(e => e.FKMediaObjectId)
                    .HasName("IX_FKMediaObjectId");

                entity.Property(e => e.ConversionType).HasDefaultValueSql("((0))");

                entity.Property(e => e.RotationAmount).HasDefaultValueSql("((0))");

                //entity.HasOne(d => d.FkmediaObject)
                //    .WithMany(p => p.MediaQueue)
                //    .HasForeignKey(d => d.FkmediaObjectId)
                //    .HasConstraintName("FK_gsp.MediaQueue_gsp.MediaObject_FKMediaObjectId");
            });

            modelBuilder.Entity<MediaTemplateDto>(entity =>
            {
                entity.HasIndex(e => new { e.MimeType, e.BrowserId })
                    .HasName("UC_MediaTemplate_MimeType_BrowserId")
                    .IsUnique();
            });

            modelBuilder.Entity<MetadataDto>(entity =>
            {
                entity.HasIndex(e => e.FKAlbumId)
                    .HasName("IX_FKAlbumId");

                entity.HasIndex(e => e.FKMediaObjectId)
                    .HasName("IX_FKMediaObjectId");

                entity.HasIndex(e => new { e.MetaName, e.MetadataId })
                    .HasName("UC_Metadata_MetaName_MetadataId")
                    .IsUnique();

                //entity.HasOne(d => d.Fkalbum)
                //    .WithMany(p => p.Metadata)
                //    .HasForeignKey(d => d.FkalbumId)
                //    .HasConstraintName("FK_gsp.Metadata_gsp.Album_FKAlbumId");

                //entity.HasOne(d => d.MediaObject)
                //    .WithMany(p => p.Metadata)
                //    .HasForeignKey(d => d.FKMediaObjectId)
                //    .OnDelete(DeleteBehavior.Cascade)
                //    .HasConstraintName("FK_gsp.Metadata_gsp.MediaObject_FKMediaObjectId");
            });

            modelBuilder.Entity<MetadataTagDto>(entity =>
            {
                entity.HasKey(e => new { e.FKMetadataId, e.FKTagName });

                entity.HasIndex(e => e.FKMetadataId)
                    .HasName("IX_FKMetadataId");

                entity.HasIndex(e => e.FKTagName)
                    .HasName("IX_FKTagName");

                //entity.HasOne(d => d.Fkmetadata)
                //    .WithMany(p => p.MetadataTag)
                //    .HasForeignKey(d => d.FkmetadataId)
                //    .HasConstraintName("FK_gsp.MetadataTag_gsp.Metadata_FKMetadataId");

                //entity.HasOne(d => d.FktagNameNavigation)
                //    .WithMany(p => p.MetadataTag)
                //    .HasForeignKey(d => d.FktagName)
                //    .HasConstraintName("FK_gsp.MetadataTag_gsp.Tag_FKTagName");
            });

            modelBuilder.Entity<MimeTypeDto>(entity =>
            {
                entity.HasIndex(e => e.FileExtension)
                    .HasName("UC_MimeType_FileExtension")
                    .IsUnique();
            });

            modelBuilder.Entity<MimeTypeGalleryDto>(entity =>
            {
                entity.HasIndex(e => e.FKGalleryId)
                    .HasName("IX_FKGalleryId");

                entity.HasIndex(e => e.FKMimeTypeId)
                    .HasName("IX_FKMimeTypeId");

                entity.HasIndex(e => new { e.FKGalleryId, e.FKMimeTypeId })
                    .HasName("UC_MimeTypeGallery_FKGalleryId_FKMimeTypeId")
                    .IsUnique();

                //entity.HasOne(d => d.Fkgallery)
                //    .WithMany(p => p.MimeTypeGallery)
                //    .HasForeignKey(d => d.FkgalleryId)
                //    .HasConstraintName("FK_gsp.MimeTypeGallery_gsp.Gallery_FKGalleryId");

                //entity.HasOne(d => d.FkmimeType)
                //    .WithMany(p => p.MimeTypeGallery)
                //    .HasForeignKey(d => d.FkmimeTypeId)
                //    .HasConstraintName("FK_gsp.MimeTypeGallery_gsp.MimeType_FKMimeTypeId");
            });

            modelBuilder.Entity<RoleDto>(entity =>
            {
                entity.Property(e => e.RoleName).ValueGeneratedNever();
            });

            modelBuilder.Entity<RoleAlbumDto>(entity =>
            {
                entity.HasKey(e => new { e.FKRoleName, e.FKAlbumId });

                entity.HasIndex(e => e.FKAlbumId)
                    .HasName("IX_FKAlbumId");

                entity.HasIndex(e => e.FKRoleName)
                    .HasName("IX_FKRoleName");

                //entity.HasOne(d => d.Fkalbum)
                //    .WithMany(p => p.RoleAlbum)
                //    .HasForeignKey(d => d.FkalbumId)
                //    .HasConstraintName("FK_gsp.RoleAlbum_gsp.Album_FKAlbumId");

                //entity.HasOne(d => d.FkroleNameNavigation)
                //    .WithMany(p => p.RoleAlbum)
                //    .HasForeignKey(d => d.FkroleName)
                //    .HasConstraintName("FK_gsp.RoleAlbum_gsp.Role_FKRoleName");
            });

            modelBuilder.Entity<UiTemplateDto>(entity =>
            {
                entity.HasIndex(e => new { e.TemplateType, e.FKGalleryId, e.Name })
                    .HasName("UC_UiTemplate_TemplateType_Name")
                    .IsUnique();
            });

            modelBuilder.Entity<UiTemplateAlbumDto>(entity =>
            {
                entity.HasKey(e => new { e.FKUiTemplateId, e.FKAlbumId });

                entity.HasIndex(e => e.FKAlbumId)
                    .HasName("IX_FKAlbumId");

                entity.HasIndex(e => e.FKUiTemplateId)
                    .HasName("IX_FKUiTemplateId");

                //entity.HasOne(d => d.Fkalbum)
                //    .WithMany(p => p.UiTemplateAlbum)
                //    .HasForeignKey(d => d.FkalbumId)
                //    .HasConstraintName("FK_gsp.UiTemplateAlbum_gsp.Album_FKAlbumId");

                //entity.HasOne(d => d.FkuiTemplate)
                //    .WithMany(p => p.UiTemplateAlbum)
                //    .HasForeignKey(d => d.FkuiTemplateId)
                //    .HasConstraintName("FK_gsp.UiTemplateAlbum_gsp.UiTemplate_FKUiTemplateId");
            });

            modelBuilder.Entity<UserGalleryProfileDto>(entity =>
            {
                entity.HasIndex(e => e.FKGalleryId)
                    .HasName("IX_FKGalleryId");

                entity.HasIndex(e => new { e.UserName, e.FKGalleryId, e.SettingName })
                    .HasName("UC_UserGalleryProfile_UserName_FKGalleryId_SettingName")
                    .IsUnique();

                //entity.HasOne(d => d.Fkgallery)
                //    .WithMany(p => p.UserGalleryProfile)
                //    .HasForeignKey(d => d.FkgalleryId)
                //    .HasConstraintName("FK_gsp.UserGalleryProfile_gsp.Gallery_FKGalleryId");
            });
        }
    }
}
