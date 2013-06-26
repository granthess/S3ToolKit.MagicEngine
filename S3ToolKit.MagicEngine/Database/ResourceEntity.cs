using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using S3ToolKit.GameFiles;
using S3ToolKit.GameFiles.Package;

namespace S3ToolKit.MagicEngine.Database
{
    public class ResourceEntity
    {
        #region Data Properties
        [Key]
        public int Id { get; set; }

        // Data Properties
        public string Key { get; set; }
        
        public bool IsActive { get; set; }        

        // Navigation Properties
        public virtual PackageEntity ParentPackage { get; set; }
        #endregion
    }
}
