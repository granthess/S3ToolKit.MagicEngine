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
using S3ToolKit.Utils;
using S3ToolKit.Utils.Logging;
using System.IO;
using S3ToolKit.Utils.Registry;

namespace S3ToolKit.MagicEngine.Core
{
    // Singleton using Lazy<T> from http://geekswithblogs.net/BlackRabbitCoder/archive/2010/05/19/c-system.lazylttgt-and-the-singleton-design-pattern.aspx
    public class SettingsManager
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton
        private static readonly Lazy<SettingsManager> _instance = new Lazy<SettingsManager>(() => new SettingsManager());

        public static SettingsManager Instance { get { return _instance.Value; } }
        #endregion

        #region Fields
        
        private IniFile SettingsFile;
        #endregion

        #region Properties
        public string this[string index] { get { return GetValue(index); } set { SetValue(index, value); } }
        public string AppDataDirectory { get; private set; }
        public InstalledGameEntry Game { get { return InstallationInfo.Instance.NewestGame; } }
        #endregion

        #region Constructors
        private SettingsManager() 
        {
            AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CC Magic");
            SettingsFile = new IniFile(Path.Combine(AppDataDirectory, "CCMAGIC.ini"));
        }
        #endregion

        #region Helper Methods
        private string GetValue(string index)
        {
            return SettingsFile.GetValue("Global", index);
        }

        private void SetValue(string index, string value)
        {
            SettingsFile.SetValue("Global", index, value);
        }
        #endregion

    }
}
