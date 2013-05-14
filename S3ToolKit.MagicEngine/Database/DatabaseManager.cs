/*
    Copyright 2012, Grant Hess

    This file is part of S3ToolKit.MagicEngine.

    S3ToolKit.Utils is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Foobar is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with CC Magic.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using S3ToolKit.Utils.Logging;
using S3ToolKit.MagicEngine.Core;
using System.IO;

namespace S3ToolKit.MagicEngine.Database
{
    public class DatabaseManager : INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton
        private static readonly Lazy<DatabaseManager> _instance = new Lazy<DatabaseManager>(() => new DatabaseManager());

        public static DatabaseManager Instance { get { return _instance.Value; } }
        #endregion

        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        // ActiveSharp property change notification implementation
        // use this layout to set properties:
        //     public int Foo
        //     {
        //       get { return _foo; }
        //       set { SetValue(ref _foo, value); }   // assigns value and does prop change notification, all in one line
        //     }
        protected void SetValue<T>(ref T field, T value)
        {
            field = value;   //Actually assign the new value
            PropertyInfo changedProperty = ActiveSharp.PropertyMapping.PropertyMap.GetProperty(this, ref field);

            OnPropertyChanged(changedProperty.Name);
        }
        #endregion

        #region Property Fields
        private SetEntity _DefaultSet ;
        private SetEntity _LegacySet ; 
        private ConfigEntity _DefaultConfig;
        #endregion

        #region Properties
        public SetEntity DefaultSet { get { return _DefaultSet; } set { SetValue<SetEntity>(ref _DefaultSet, value); } }
        public SetEntity LegacySet { get { return _LegacySet; } set { SetValue<SetEntity>(ref _LegacySet, value); } }
        public ConfigEntity DefaultConfig { get { return _DefaultConfig; } set { SetValue<ConfigEntity>(ref _DefaultConfig, value); } }
        #endregion

        #region Manager Properties
        public SettingsManager mgrSettings { get; private set; }
        #endregion

        #region Contructors
        private DatabaseManager()
        { 
            mgrSettings = SettingsManager.Instance;
        }
        #endregion

        #region Helpers
        public MagicContext GetNewContext()
        {
            string DatabaseFile = Path.Combine(mgrSettings["dir_database"],"database", "CCM.sdf");
            return MagicContext.CreateInstance(DatabaseFile);
        }
        #endregion


        #region Database Validation
        public void ValidateDatabase()
        {
            log.Info("ValidateDatabase()");
            ValidateDefaults();
        }

        private void ValidateDefaults()
        {
            MagicContext Context = GetNewContext();

            try
            {
                // Ensure that the Default Config exists
                var Configs = from i in Context.Configurations
                              where i.IsDefault == true
                              orderby i.Id
                              select i;

                if (Configs.Count() < 1)
                {
                    DefaultConfig = new ConfigEntity();
                    DefaultConfig.IsDefault = true;
                    DefaultConfig.Name = "Default";
                    DefaultConfig.Description = "Default pre-installed configuration";
                    DefaultConfig.IsActive = true;
                    Context.Configurations.Add(DefaultConfig);
                    Context.SaveChanges();
                }
                else
                {
                    DefaultConfig = Configs.First<ConfigEntity>();
                }


                // Ensure that the Default Set exists
                var Sets = from i in Context.Sets
                           where i.IsDefault == true & i.Name == "Default"
                           orderby i.Id
                           select i;

                if (Sets.Count() < 1)
                {
                    DefaultSet = new SetEntity();
                    DefaultSet.IsDefault = true;
                    DefaultSet.IsVirtual = false;
                    DefaultSet.Name = "Default";
                    DefaultSet.Description = "Default pre-installed set.  All newly installed items will be placed here.";
                    DefaultSet.IsActive = true;
                    Context.Sets.Add(DefaultSet);
                    DefaultSet.Configuations = new List<ConfigEntity>();
                    DefaultSet.Configuations.Add(DefaultConfig);
                    Context.SaveChanges();
                }
                else
                {
                    DefaultSet = Sets.First<SetEntity>();
                }

                // Ensure that the Legacy Set exists
                Sets = from i in Context.Sets
                       where i.IsDefault == true & i.Name == "Legacy"
                       orderby i.Id
                       select i;

                if (Sets.Count() < 1)
                {
                    LegacySet = new SetEntity();
                    LegacySet.IsDefault = true;
                    LegacySet.IsVirtual = false;
                    LegacySet.Name = "Legacy";
                    LegacySet.Description = "Preinstalled items from the existing Mod Framework are listed here.";
                    LegacySet.IsActive = true;
                    Context.Sets.Add(LegacySet);
                    LegacySet.Configuations = new List<ConfigEntity>();
                    LegacySet.Configuations.Add(DefaultConfig);
                    Context.SaveChanges();
                }
                else
                {
                    LegacySet = Sets.First<SetEntity>();
                }
            }
            finally
            {
                Context.Dispose();
            }
        }
        #endregion
    }
}
