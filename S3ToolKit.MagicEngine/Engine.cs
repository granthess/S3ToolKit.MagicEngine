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

            // Now open up the Datafile Manager 
            mgrFiles.Initialize();

            // And start background processing
            mgrProcess.StartProcessQueue();
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

        
    }
}
