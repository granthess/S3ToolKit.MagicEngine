using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.IO;

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
        #endregion

        #region Helpers
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}
