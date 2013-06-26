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
using S3ToolKit.Utils.Logging;
using S3ToolKit.MagicEngine.Database;
using System.Reflection;
using System.Collections.Concurrent;
using System.IO;
using S3ToolKit.MagicEngine.Core;
using S3ToolKit.Utils.Registry;
using S3ToolKit.GameFiles.Package;
using System.Threading.Tasks;

namespace S3ToolKit.MagicEngine.Datafile
{
    public class FileManager : INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton
        private static readonly Lazy<FileManager> _instance = new Lazy<FileManager>(() => new FileManager());

        public static FileManager Instance { get { return _instance.Value; } }
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
        private ConcurrentBag<IFileProcess> FileTasks;
        private Task Background;
        #endregion
        
        #region Manager Properties
        public SettingsManager mgrSettings { get; private set; }
        #endregion

        #region Contructors
        private FileManager()
        {
            mgrSettings = SettingsManager.Instance; 

            FileTasks = new ConcurrentBag<IFileProcess>();
        }
        #endregion

        #region Public Methods
        public void Initialize()
        {
            // start up background processing thread and return to caller.
            
            // HACK: We are going to manually create the list of files to import
            string DownloadFolder = Path.Combine(InstallationInfo.Instance.DocumentBaseDir ,"Downloads");
            Directory.CreateDirectory (DownloadFolder);  // just in case

            var FileList = Directory.GetFiles(DownloadFolder, "*.package");

            foreach (string Filename in FileList)
            {
                FileTasks.Add (new ImportPackage(Filename));
            }

            Background = new Task(() => BackgroundMethod());

            Background.Start();
            Background.Wait();
        }
        #endregion

        #region worker thread methods
        private void BackgroundMethod()
        {
            IFileProcess Process;
            while (FileTasks.Count > 0)
            {
                if (FileTasks.TryTake(out Process))
                {
                    log.Debug(string.Format("Starting background process: {0}", Process.ToString()));

                    string ProcessType = Process.ProcessType;                    
                    if (ProcessType == "ImportPACKAGE")
                    {
                        ImportDBPF(Process.Filename);
                    }
                    else if (ProcessType == "ImportTS3PACK")
                    {
                        ImportTS3Pack(Process.Filename);
                    }
                    else
                    {
                        log.Warn(string.Format("Unknown background process type {0}", ProcessType));
                    }
                }                
                else
                {
                    log.Warn("No tasks left!");
                    break;
                }
            }
        }
        #endregion

        #region Import from file
        private void ImportDBPF(string Filename)
        {
            // The database context to work with
            MagicContext Context = DatabaseManager.Instance.GetNewContext();
            
            // find out if this file already exists in the database ??!?!
            bool dupName = (from i in Context.Datafiles
                            where i.FileName == Filename
                            select i).Count() > 0;

            // Just skip for now since we aren't doing the proper move to packages directory stuff yet
            if (dupName)
                return;

            // Get a new DatafileEntity and start populating it
            DatafileEntity newFile = Context.Datafiles.Create<DatafileEntity>();
            newFile.FileName = Filename;
            newFile.InstallDate = DateTime.Now;
            newFile.Rating = -1;  
            newFile.Category = string.Empty;
            newFile.Description = string.Empty;
            newFile.URL = string.Empty;
        
            newFile.IsEnabled = true;
            newFile.IsTS3Pack = false;

            // now import the DBPF portion
            using (Stream datastream = File.OpenRead(Filename))
            {
                ImportSubPackage(Context, datastream, newFile, Path.GetFileNameWithoutExtension(Filename));
            }

            Context.Datafiles.Add(newFile);            
            Context.SaveChanges();
        }

        private void ImportTS3Pack(string Filename)
        {
            throw new NotImplementedException();
        }

        private PackageEntity ImportSubPackage(MagicContext Context, Stream datastream, DatafileEntity entity, string Name)
        {
            // Load up the package and retreive the information
            DBPFPackage pkg = new DBPFPackage(datastream);

            // since we didn't exception out, we have a package...
            // import the values
            PackageEntity package = Context.Packages.Create<PackageEntity>();
            Context.Packages.Add(package);
            package.Name = Name;
            package.IsEnabled = true;
            package.Description = string.Empty;
            package.ParentDatafile = entity;
            
            // and the resources
            foreach (ResourceEntry Resource in pkg.Resources)
            {
                ResourceEntity Rsrc = Context.Resources.Create<ResourceEntity>();
                Context.Resources.Add(Rsrc);
                Rsrc.IsActive = true;
                Rsrc.Key = Resource.Key.ToString();
                Rsrc.ParentPackage = package;
            }

            return package;
        }
        #endregion

    }
}
