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

using S3ToolKit.Utils.Logging;
using System.Runtime.InteropServices;
using S3ToolKit.Utils.Registry;
using System.IO;
using System.Diagnostics;

namespace S3ToolKit.MagicEngine.Core
{
    public class GameVersionEntry
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());

        // public BitmapImage Image { get; set; }
        public string DisplayName { get { return baseEntry.DisplayName; } }
        public string DisplayValue { get { return string.Format("{0}\n{1}", DisplayName, Version); } }
        public string Version { get; set; }
        // public System.Windows.Controls.Image Content { get { return GetContent(); } }

        //private System.Windows.Controls.Image GetContent()
        //{
        //    System.Windows.Controls.Image Img = new Image();

        //    Img.Source = this.Image;
        //    Img.ToolTip = this.DisplayValue;

        //    return Img;
        //}

        [DllImport("shell32.dll")]
        static extern IntPtr ExtractIcon(
            IntPtr hInst,
            [MarshalAs(UnmanagedType.LPStr)] string lpszExeFileName,
            uint nIconIndex);


        public InstalledGameEntry baseEntry;

        public GameVersionEntry(InstalledGameEntry Entry)
        {

            if (Entry.IsGame != true)
                throw new ArgumentException("Not an installed Game/Expansion Pack/Stuff Pack");

            this.baseEntry = Entry;

            GenerateBitmap();
            GenerateVersion();
        }

        private void GenerateVersion()
        {
            string VersionFileName = Path.Combine(baseEntry.InstallDir, "Game", "Bin", "skuversion.txt");

            if (!File.Exists(VersionFileName))
            {
                log.Error("Cannot Load Version Number from file: " + VersionFileName);
                Version = "Unknown";
                return;
            }

            StreamReader Reader = File.OpenText(VersionFileName);
            string Line;
            while (!Reader.EndOfStream)
            {
                Line = Reader.ReadLine();

                if (Line.ToLower().StartsWith("gameversion"))
                {
                    Version = Line.Substring(Line.IndexOf('=') + 1).Trim();
                    break;
                }
            }

            Reader.Close();
        }

        protected string GetBinDirectory()
        {
            string DLLFileName = Path.Combine(baseEntry.InstallDir, "Game", "Bin");

            if (!Directory.Exists(DLLFileName))
            {
                log.Error(string.Format("Directory Does Not Exist: {0}", DLLFileName));
                return "";
            }

            var FileList = Directory.GetFiles(DLLFileName, "Sims3*GDF.dll");

            foreach (var FileName in FileList)
            {
                return FileName;
            }
            return "";
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", DisplayName, Version);
        }

        private void GenerateBitmap()
        {
            //string FileName = GetBinDirectory();

            //if (FileName == "")
            //{
            //    log.Error("Cannot Load Icon from file: " + FileName);
            //    return;
            //}

            //var inst = Process.GetCurrentProcess().Handle;
            //var x = ExtractIcon(inst, FileName, 0);

            //System.Drawing.Icon myIcon = System.Drawing.Icon.FromHandle(x);
            //System.Drawing.Bitmap bmp = myIcon.ToBitmap();

            //MemoryStream temp = new MemoryStream();
            //bmp.Save(temp, System.Drawing.Imaging.ImageFormat.Png);

            //BitmapImage bmpImage = new BitmapImage();


            //bmpImage.BeginInit();

            //temp.Seek(0, SeekOrigin.Begin);

            //bmpImage.StreamSource = temp;

            //bmpImage.EndInit();

            //Image = bmpImage;
            //Image.CacheOption = BitmapCacheOption.Default;

            ////temp.Close();
        }
    }
}
