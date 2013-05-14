using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace S3ToolKit.MagicEngine.Database
{
    public class SetEntity
    {
        #region Data Properties
        [Key]
        public int Id { get; set; }

        // Data Properties
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDirty { get; set; }
        public bool IsVirtual { get; set; }

        // Navigation Properties
        // Sets
        public virtual List<SetEntity> ChildSets { get; set; }
        public virtual SetEntity ParentSet { get; set; }

        // Configuration
        public virtual List<ConfigEntity> Configuations { get; set; }

        // Collection
        public virtual CollectionEntity Collection { get; set; }

        // Datafile (.sims3pack or .package)
        public virtual List<DatafileEntity> Datafiles { get; set; }
        #endregion

        #region Helpers
        public override string ToString()
        {
            string S1 = IsDefault ? "[D]" : "[ ]";
            return string.Format("{0} {1} (In {2} Configs) -- {3}", S1, Name, Configuations.Count, Description);
        }
        #endregion
    }
}
