using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace S3ToolKit.MagicEngine.Database
{
    public class DatafileEntity
    {
        #region Data Properties
        [Key]
        public int Id { get; set; } 

        // Data Properties
        public double Rating { get; set; }
        public string Category { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public DateTime InstallDate { get; set; }

        public bool IsEnabled { get; set; }
        public bool IsTS3Pack { get; set; }

        // Navigation Properties
        public virtual SetEntity ParentSet { get; set; }
        public virtual List<PackageEntity> Packages { get; set; }
        #endregion
    }
}
