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
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using S3ToolKit.MagicEngine.Core;
using System.Collections.ObjectModel;
using S3ToolKit.Utils.Registry;
using S3ToolKit.Utils.Logging;
using System.IO;
using S3ToolKit.MagicEngine.Database;
using System.Data;
using System.Data.Common;
using S3ToolKit.MagicEngine.Datafile;
using S3ToolKit.MagicEngine.Processes;
using System.Data.Entity;


namespace S3ToolKit.MagicEngine
{

    // Singleton using Lazy<T> from http://geekswithblogs.net/BlackRabbitCoder/archive/2010/05/19/c-system.lazylttgt-and-the-singleton-design-pattern.aspx
    public class Engine : INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton
        private static readonly Lazy<Engine> _instance = new Lazy<Engine>(() => new Engine());

        public static Engine Instance { get { return _instance.Value; } }
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

        #region Fields
        private ObservableCollection<GameVersionEntry> _GameInfo;
        public MagicContext ViewContext;

        private ConfigEntity _CurrentConfig;
        #endregion

        #region Manager Properties
        public SettingsManager mgrSettings { get; private set; }
        public DatabaseManager mgrDatabase { get; private set; }
        public FileManager mgrFiles { get; private set; }
        public ProcessManager mgrProcess { get; private set; } 
        #endregion

        #region Properties
        public ObservableCollection<GameVersionEntry> GameInfo { get { return GetGameInfo(); } }
        #endregion

        #region Database Visualizers 
        public DbSet<CollectionEntity> Collections { get; private set; }
        public DbSet<CollectionItemEntity> CollectionItems { get; private set; }
        public DbSet<ConfigEntity> Configurations { get; private set; }
        public DbSet<DatafileEntity> Datafiles { get; private set; }
        public DbSet<PackageEntity> Packages { get; private set; }
        public DbSet<ResourceEntity> Resources { get; private set; }
        public DbSet<SetEntity> Sets { get; private set; }

        public List<SetEntity> RootSets { get { return GetRootSets(); } }
        private List<SetEntity> GetRootSets()
        {
            return (from i in Sets
                    where i.ParentSet == null
                    select i).ToList<SetEntity>();
        }

        public ConfigEntity CurrentConfig { get { return _CurrentConfig; } set { SetCurrentConfig (value); } }
        private void SetCurrentConfig(ConfigEntity value)
        {
            if (_CurrentConfig != null & _CurrentConfig != value)
            {
                _CurrentConfig.IsActive = false;
            }
            _CurrentConfig = value;
            _CurrentConfig.IsActive = true;
            OnPropertyChanged("CurrentConfig");
            UpdateSetLists();
        }

        private SetEntity _CurrentSet;
        public SetEntity CurrentSet { get { return _CurrentSet; } set { SetCurrentSet(value); } }
        private void SetCurrentSet(SetEntity value)
        {
            _CurrentSet = value;
            OnPropertyChanged("CurrentSet");
        }

        public List<SetEntity> CFGSetsToDisable { get; private set; }
        public List<SetEntity> CFGSetsToEnable { get; private set; }

        public ObservableCollection<SetEntity> EnabledSets { get { return GetEnabledSetList(); } }
        public ObservableCollection<SetEntity> DisabledSets { get { return GetDisabledSetList(); } }
        #endregion

        #region Private Helpers
        private ObservableCollection<GameVersionEntry> GetGameInfo()
        {
            if (_GameInfo == null)
            {
                _GameInfo = GenerateGameInfo();
            }

            return _GameInfo;
        }

        private ObservableCollection<GameVersionEntry> GenerateGameInfo()
        {
            ObservableCollection<GameVersionEntry> temp = new ObservableCollection<GameVersionEntry>();

            SortedDictionary<int, InstalledGameEntry> List = new SortedDictionary<int, InstalledGameEntry>();

            foreach (var entry in InstallationInfo.Instance.Packs)
            {
                if (entry.IsGame)
                {
                    List.Add(entry.ProductID, entry);
                }
            }

            foreach (var entry in List)
            {
                if (entry.Value.IsGame)
                {
                    temp.Add(new GameVersionEntry(entry.Value));
                }
            }

            return temp;
        }

        private string GetLauncherVersion()
        {
            Version Ver = Assembly.GetEntryAssembly().GetName().Version;
            return string.Format("CC Magic [{0}.{1}r{2} Build: {3}]", Ver.Major, Ver.Minor, Ver.Revision, Ver.Build);
        }
        private string GetWindowsVersion()
        {
            return System.Environment.OSVersion.ToString();
        }
        private string GetWindowsBits()
        {
            string temp;
            if (System.Environment.Is64BitProcess)
            {
                temp = "64-bit Application on ";
            }
            else
            {
                temp = "32-bit Application on ";
            }
            if (System.Environment.Is64BitOperatingSystem)
            {
                temp += "64-bit Windows";
            }
            else
            {
                temp += "32-bit Windows";
            }

            return temp;
        }        
        #endregion

        #region Contructors
        private Engine()
        {
            mgrSettings = SettingsManager.Instance;
            mgrDatabase = DatabaseManager.Instance;
            mgrFiles = FileManager.Instance;

            CFGSetsToEnable = new List<SetEntity>();
            CFGSetsToDisable = new List<SetEntity>();
        }
        #endregion

        #region Initialization
        public void Initialize()
        {
            log.Debug("Initialize()");
            // Start by ensuring that the configuration files are created and 
            // contain sane data
            ValidateSettings();

            // Enable logging to the directory specified in the settings
            EnableLogging();

            // Create a database context and validate the database
            mgrDatabase.ValidateDatabase();

            // Get local db Context -- separate from the commanding context
            ViewContext = mgrDatabase.GetNewContext();
            Collections = ViewContext.Collections;
            CollectionItems = ViewContext.CollectionItems;
            Configurations = ViewContext.Configurations; Configurations.Load();
            Datafiles = ViewContext.Datafiles;
            Packages = ViewContext.Packages;
            Resources = ViewContext.Resources;
            Sets = ViewContext.Sets;

            CurrentConfig = (from i in Configurations
                             where i.IsActive == true
                             select i).First<ConfigEntity>();

            CurrentSet = Sets.First<SetEntity>(); 

            // Now open up the Datafile Manager 
            mgrFiles.Initialize();

            // And start background processing
            // mgrProcess.StartProcessQueue();
        }

        private void ValidateSettings()
        {
            log.Debug("ValidateSettings()");

            // Verify that the settings file exists and that we have values for:
            // * Database Directory
            // * Download Directory
            // * Enable Compression
            // * Log Directory

            if (mgrSettings["dir_database"] == null)
            {
                mgrSettings["dir_database"] = mgrSettings.AppDataDirectory;
            }

            if (mgrSettings["dir_download"] == null)
            {
                mgrSettings["dir_download"] = Path.Combine(InstallationInfo.Instance.DocumentBaseDir, "Downloads");
            }

            if (mgrSettings["bool_compress"] == null)
            {
                mgrSettings["bool_compress"] = "true";
            }

            if (mgrSettings["dir_log"] == null)
            {
                mgrSettings["dir_log"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Electronic Arts", "CC Magic 2.5", "Logs");
            }
        }

        private void EnableLogging()
        {
            // Redirect logging to the configured log file
            log.Debug("EnableLogging()");

            LogManager.SetFilename(Path.Combine(mgrSettings["dir_log"], "CCMagic25.log"));
        }
        #endregion

        #region Configuration Tab actions
        public void AddConfig()
        {
            ConfigEntity NewCFG = ViewContext.Configurations.Create<ConfigEntity>();
            NewCFG.IsDefault = false;
            NewCFG.IsActive = true;
            NewCFG.Name = "<New Config>";
            NewCFG.Description = "";

            ViewContext.Configurations.Add(NewCFG);
            CurrentConfig = NewCFG;
            ViewContext.SaveChanges();
        }

        public void RemoveConfig()
        {
            ConfigEntity temp = CurrentConfig;
            temp.IsActive = false;
            _CurrentConfig = null;

            CurrentConfig = (from i in ViewContext.Configurations
                             where i.IsDefault == true
                             select i).First<ConfigEntity>();

            temp.Sets.Clear();
            ViewContext.Configurations.Remove(temp);
            
            ViewContext.SaveChanges();
        }

        public void EnableSets(List<SetEntity> Sets)
        {
            foreach (SetEntity entry in Sets)
            {
                entry.Configuations.Add(CurrentConfig);
            }
            ViewContext.SaveChanges();
            UpdateSetLists();
        }

        public void DisableSets(List<SetEntity> Sets)
        {
            foreach (SetEntity entry in Sets)
            {
                entry.Configuations.Remove(CurrentConfig);
            }
            ViewContext.SaveChanges();
            UpdateSetLists();
        }

        private ObservableCollection<SetEntity> _EnabledSetList;
        private ObservableCollection<SetEntity> _DisabledSetList;

        private void UpdateSetLists()
        {
            if (_EnabledSetList == null)
            {
                _EnabledSetList = new ObservableCollection<SetEntity>();
            }

            if (_DisabledSetList == null)
            {
                _DisabledSetList = new ObservableCollection<SetEntity>();
            }
            
            List<SetEntity> temp = ViewContext.Sets.ToList<SetEntity>();

            // add any new sets
            foreach (SetEntity entry in temp)
            {
                if ((!_EnabledSetList.Contains(entry) & (!_DisabledSetList.Contains(entry))))
                {
                    _DisabledSetList.Add(entry);
                }               
            }

            // Disable any newly disabled
            temp = _EnabledSetList.ToList<SetEntity>();
            foreach (SetEntity entry in temp)
            {
                if (!entry.Configuations.Contains(CurrentConfig))
                {
                    _EnabledSetList.Remove(entry);
                    _DisabledSetList.Add(entry);
                }
            }

            // Enable any newly enabled
            temp = _DisabledSetList.ToList<SetEntity>();
            foreach (SetEntity entry in temp)
            {
                if (entry.Configuations.Contains(CurrentConfig))
                {
                    _EnabledSetList.Add(entry);
                    _DisabledSetList.Remove(entry);
                }
            }
        }

        private ObservableCollection<SetEntity> GetEnabledSetList()
        {
            if (_EnabledSetList == null)
            {
                UpdateSetLists();
            }
            return _EnabledSetList;
        }

        private ObservableCollection<SetEntity> GetDisabledSetList()
        {
            if (_DisabledSetList == null)
            {
                UpdateSetLists();
            }
            return _DisabledSetList;
        }

        #endregion

        #region Set Tab actions
        public void AddSet()
        {
            SetEntity temp = ViewContext.Sets.Create<SetEntity>();
            temp.IsDirty = true;
            temp.Name = "<New Set>";
            temp.Description = string.Empty;
            ViewContext.Sets.Add(temp);
            CurrentConfig.Sets.Add(temp);
            CurrentSet.ChildSets.Add(temp);
            CurrentSet = temp;
            ViewContext.SaveChanges();
            OnPropertyChanged("RootSets");
        }

        public void RemoveSet()
        {
            SetEntity temp = CurrentSet;
            
            _CurrentSet = null;

            if (temp.ParentSet != null)
            {
                CurrentSet = temp.ParentSet;
            }
            else
            {
                CurrentSet = (from i in ViewContext.Sets
                              where i.IsDefault == true & i.Name == "Default"
                              select i).First<SetEntity>();
            }

            temp.Configuations.Clear();

            foreach (DatafileEntity entry in temp.Datafiles)
            {
                CurrentSet.Datafiles.Add(entry);
            }

            foreach (SetEntity entry in temp.ChildSets)
            {
                CurrentSet.ChildSets.Add(entry);
            }

            ViewContext.Sets.Remove(temp);

            ViewContext.SaveChanges();
            OnPropertyChanged("RootSets");
        }
        #endregion


    }
}
