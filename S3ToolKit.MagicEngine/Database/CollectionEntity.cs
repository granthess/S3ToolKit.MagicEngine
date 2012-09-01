using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace S3ToolKit.MagicEngine.Database
{    
    public class CollectionEntity
    {
        #region Data Properties
        [Key]
        public int Id { get; set; }

        // Data Properties
        public string Name { get; set; }
        public string ThumbnailKey { get; set; }

        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }

        // Navigation Properties
        public virtual List<CollectionItemEntity> Items { get; set; }
        #endregion
    }
}
