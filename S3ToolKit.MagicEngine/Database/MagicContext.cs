using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.IO;
using System.Data.SqlServerCe;
using System.Data.Entity.Infrastructure;

namespace S3ToolKit.MagicEngine.Database
{
    public class MagicContext : DbContext
    {
        #region Properties
        #endregion

        #region Data Table Properties
        public DbSet<CollectionEntity> Collections { get; set; }
        public DbSet<CollectionItemEntity> CollectionItems { get; set; }
        public DbSet<ConfigEntity> Configurations {get; set;}
        public DbSet<DatafileEntity> Datafiles { get; set; }
        public DbSet<PackageEntity> Packages { get; set; }
        public DbSet<ResourceEntity> Resources { get; set; }
        public DbSet<SetEntity> Sets { get; set; }
        #endregion

        #region Constructors
        
        // Info on how to set the connection string from
        // http://stackoverflow.com/questions/12490190/ef-5-sql-ce-4-how-to-specify-custom-location-for-database-file
        public static MagicContext CreateInstance(string Location)
        {
            // Set connection string
            var connectionString = string.Format("Data Source={0}", Location);
            System.Data.Entity.Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0", "", connectionString);
                        
            // Ensure that the destination directory actually exists
            Directory.CreateDirectory(Path.GetDirectoryName(Location));
            return new MagicContext();
        }
        #endregion

        #region Helpers
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}
