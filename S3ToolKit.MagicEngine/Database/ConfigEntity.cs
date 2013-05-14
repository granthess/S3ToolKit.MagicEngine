using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace S3ToolKit.MagicEngine.Database
{
    public class ConfigEntity
    {
        #region Data Properties
        [Key]
        public int Id { get; set; }

        // Data Properties
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }

        // Navigation Properties
        public virtual List<SetEntity> Sets { get; set; }
        #endregion

        #region Helpers
        public override string ToString()
        {
            string S1 = IsDefault ? "[D]" : "[ ]";
            return string.Format("{0} {1} (Has {2} Sets) -- {3}", S1, Name, Sets.Count, Description);
        }
        #endregion
    }
}
