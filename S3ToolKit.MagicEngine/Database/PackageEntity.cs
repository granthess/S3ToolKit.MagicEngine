using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace S3ToolKit.MagicEngine.Database
{
    public class PackageEntity
    {
        #region Data Properties
        [Key]
        public int Id { get; set; }

        // Data Properties
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsEnabled { get; set; }
        
        // Navigation Properties
        public virtual DatafileEntity ParentDatafile { get; set; }
        public virtual List<ResourceEntity> Resources { get; set; }
        #endregion
    }
}
