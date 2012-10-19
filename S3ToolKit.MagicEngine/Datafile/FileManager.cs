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
                    Console.WriteLine("Found file: {0}", (Process as ImportPackage).Filename);

                }
                else
                {
                    Console.WriteLine("oops, couldn't get a task");
                }
            }
        }
        #endregion
    }
}
