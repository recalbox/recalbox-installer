using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace recalbox_installer.Model
{
    public class DriveManager
    {
        public static List<string> GetAllDrive()
        {
            List<string> listDriveLetter = new List<string>();
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady && d.VolumeLabel != "OS" && d.Name.Remove(2) != "C:")
                    {
                        
                        string ko = d.VolumeLabel;
                        string dt = System.Convert.ToString(d.DriveType);
                        listDriveLetter.Add(d.Name + d.VolumeLabel);
                    }

                }
            }
            catch {/* MessageBox.Show("Error Fetching Drive Info", "Error");*/ }
            return listDriveLetter;
        }


      //  Read more: http://buffernow.com/format-drive-using-c/#ixzz3lGbOpBso


        #region SetLabel

        /// <summary>
        /// set a drive label to the desired value
        /// </summary>
        /// <param name="driveLetter">drive letter. Example : 'A', 'B', 'C', 'D', ..., 'Z'.</param>
        /// <param name="label">label for the drive</param>
        /// <returns>true if success, false if failure</returns>
        public static bool SetLabel(char driveLetter, string label = "")
        {
            #region args check

            if (!Char.IsLetter(driveLetter))
            {
                return false;
            }
            if (label == null)
            {
                label = "";
            }

            #endregion
            try
            {
                DriveInfo di = DriveInfo.GetDrives()
                                        .Where(d => d.Name.StartsWith(driveLetter.ToString()))
                                        .FirstOrDefault();
                di.VolumeLabel = label;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region FormatDrive

        /// <summary>
        /// Format a drive using the best available method
        /// </summary>
        /// <param name="driveLetter">drive letter. Example : 'A', 'B', 'C', 'D', ..., 'Z'.</param>
        /// <param name="label">label for the drive</param>
        /// <param name="fileSystem">file system. Possible values : "FAT", "FAT32", "EXFAT", "NTFS", "UDF".</param>
        /// <param name="quickFormat">quick formatting?</param>
        /// <param name="enableCompression">enable drive compression?</param>
        /// <param name="clusterSize">cluster size (default=null for auto). Possible value depends on the file system : 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, ...</param>
        /// <returns>true if success, false if failure</returns>
        public static bool FormatDrive(char driveLetter, string label = "", string fileSystem = "NTFS", bool quickFormat = true, bool enableCompression = false, int? clusterSize = null)
        {
            return FormatDrive_CommandLine(driveLetter, label, fileSystem, quickFormat, enableCompression, clusterSize);
        }

        #endregion

        #region FormatDrive_CommandLine

        /// <summary>
        /// Format a drive using Format.com windows file
        /// </summary>
        /// <param name="driveLetter">drive letter. Example : 'A', 'B', 'C', 'D', ..., 'Z'.</param>
        /// <param name="label">label for the drive</param>
        /// <param name="fileSystem">file system. Possible values : "FAT", "FAT32", "EXFAT", "NTFS", "UDF".</param>
        /// <param name="quickFormat">quick formatting?</param>
        /// <param name="enableCompression">enable drive compression?</param>
        /// <param name="clusterSize">cluster size (default=null for auto). Possible value depends on the file system : 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, ...</param>
        /// <returns>true if success, false if failure</returns>
        public static bool FormatDrive_CommandLine(char driveLetter, string label = "", string fileSystem = "NTFS", bool quickFormat = true, bool enableCompression = false, int? clusterSize = null)
        {
            #region args check

            if (!Char.IsLetter(driveLetter) ||
                !IsFileSystemValid(fileSystem))
            {
                return false;
            }

            #endregion
            bool success = false;
            string drive = driveLetter + ":";
            try
            {
                var di = new DriveInfo(drive);
                var psi = new ProcessStartInfo();
                psi.FileName = "format.com";
                psi.WorkingDirectory = Environment.SystemDirectory;
                psi.Arguments = "/FS:" + fileSystem +
                                             " /Y" +
                                             " /V:" + label +
                                             (quickFormat ? " /Q" : "") +
                                             ((fileSystem == "NTFS" && enableCompression) ? " /C" : "") +
                                             (clusterSize.HasValue ? " /A:" + clusterSize.Value : "") +
                                             " " + drive;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                var formatProcess = Process.Start(psi);
                var swStandardInput = formatProcess.StandardInput;
                swStandardInput.WriteLine();
                formatProcess.WaitForExit();
                success = true;
            }
            catch (Exception) { }
            return success;
        }

        #endregion

 

        #region interop

        // http://msdn.microsoft.com/en-us/library/windows/desktop/bb762169(v=vs.85).aspx
        [DllImport("shell32.dll")]
        private static extern uint SHFormatDrive(IntPtr hwnd, uint drive, SHFormatFlags fmtID, SHFormatOptions options);

        private enum SHFormatFlags : uint
        {
            SHFMT_ID_DEFAULT = 0xFFFF,
            /// <summary>
            /// A general error occured while formatting. This is not an indication that the drive cannot be formatted though.
            /// </summary>
            SHFMT_ERROR = 0xFFFFFFFF,
            /// <summary>
            /// The drive format was cancelled by user/OS.
            /// </summary>
            SHFMT_CANCEL = 0xFFFFFFFE,
            /// <summary>
            /// A serious error occured while formatting. The drive is unable to be formatted by the OS.
            /// </summary>
            SHFMT_NOFORMAT = 0xFFFFFFD
        }

        [Flags]
        private enum SHFormatOptions : uint
        {
            /// <summary>
            /// Full formatting
            /// </summary>
            SHFMT_OPT_COMPLETE = 0x0,
            /// <summary>
            /// Quick Format
            /// </summary>
            SHFMT_OPT_FULL = 0x1,
            /// <summary>
            /// MS-DOS System Boot Disk
            /// </summary>
            SHFMT_OPT_SYSONLY = 0x2
        }

        #endregion


        #region IsFileSystemValid

        /// <summary>
        /// test if the provided filesystem value is valid
        /// </summary>
        /// <param name="fileSystem">file system. Possible values : "FAT", "FAT32", "EXFAT", "NTFS", "UDF".</param>
        /// <returns>true if valid, false if invalid</returns>
        public static bool IsFileSystemValid(string fileSystem)
        {
            #region args check

            if (fileSystem == null)
            {
                return false;
            }

            #endregion
            switch (fileSystem)
            {
                case "FAT":
                case "FAT32":
                case "EXFAT":
                case "NTFS":
                case "UDF":
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}
