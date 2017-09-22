using Microsoft.EntityFrameworkCore;

namespace GalleryServer.Data
{
    public static class DbInitializer
    {
        public static void Initialize(GalleryDb ctx)
        {
            ctx.Database.Migrate();

            //    if (ctx.AppSettings.Any())
            //    {
            //        return;   // DB has been seeded
            //    }

            //    var appSettings = new AppSettingDto[]
            //    {
            //        new AppSettingDto {SettingName = "InstallDateEncrypted", SettingValue = "Wevx2hoamX7tBGzyCbCNk+xwVFSizuhnJbTNRdmIOPc="},
            //        new AppSettingDto {SettingName = "MediaObjectDownloadBufferSize", SettingValue = "32768"}
            //    };

            //    ctx.AppSettings.AddRange(appSettings);
            //    ctx.SaveChanges();
        }
    }
}
