using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GalleryServer.Data.Classes
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GalleryDb>
    {
        public GalleryDb CreateDbContext(string[] args)
        {
            //IConfigurationRoot configuration = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json")
            //    .Build();

            var builder = new DbContextOptionsBuilder<GalleryDb>();
            //var connectionString = configuration.GetConnectionString("GalleryCore");
           // builder.UseSqlServer(connectionString);

            return new GalleryDb(builder.Options);
        }
    }
}
