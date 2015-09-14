using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows;
using Ionic.Zip;
using Microsoft.Win32;
using recalbox_installer.ViewModel;

namespace recalbox_installer.View
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RecalboxReleaseViewModel _recalboxReleaseViewModel;
        private DriveManagerViewModel _driveManagerViewModel;
        private string _selectedItemRelease;
        private string _fileToUnzip;
        private char _selectedItemLetter;
        private Thread _threadDownload;
        private Thread _threadFormat;
        private Thread _threadUnzip;
        private bool _downloadFinish;
        private bool _formatFinish;

        public MainWindow()
        {
            InitializeComponent();

            _recalboxReleaseViewModel = new RecalboxReleaseViewModel();
            _driveManagerViewModel = new DriveManagerViewModel();
            GridStepOne.DataContext = _recalboxReleaseViewModel;
            GridStepTwo.DataContext = _driveManagerViewModel;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(_threadDownload != null)
                _threadDownload.Abort();
            _driveManagerViewModel.StopThread();
        }

        private void checkBoxBeta_Click(object sender, RoutedEventArgs e)
        {
            _recalboxReleaseViewModel.UpdateListRelease(checkBoxBeta.IsChecked.Value);
            comboBoxReleases.SelectedIndex = 0;
        }

        private void buttonSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Zip Files (.zip)|*.zip";
            openFileDialog.FilterIndex = 1;

            bool? okValue = openFileDialog.ShowDialog();
            if (okValue == true)
            {
                textBoxFileDir.Text = openFileDialog.FileName;
            }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            comboBoxReleases.IsEditable = false;
            comboBoxDriveLetter.IsEditable = false;

            if (
                MessageBox.Show(
                    "Backup all your data before formatting.\nFormatting will erase all data on the memory device.\nDo you want continue ?",
                    "WARNING", MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _selectedItemRelease = comboBoxReleases.SelectedItem.ToString();
                _selectedItemLetter = char.Parse(comboBoxDriveLetter.SelectedItem.ToString().Substring(0, 1));
                

                if (textBoxFileDir.Text == "")
                {
                    _threadDownload = new Thread(StartDownload);
                    _threadDownload.Start();
                }
                else
                {
                    _fileToUnzip = textBoxFileDir.Text;
                    _downloadFinish = true;
                }

                _threadFormat = new Thread(StartFormat);
                _threadFormat.Start();
                _threadUnzip = new Thread(UnzipFile);
                _threadUnzip.Start();

            }
            else
            {
                comboBoxReleases.IsEditable = true;
                comboBoxDriveLetter.IsEditable = true;
            }


        }

        public void DownloadZipFile(string url)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileAsync(new Uri(url), Path.GetTempPath() + "recalbox.zip");
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (progressBarDownload.Dispatcher.CheckAccess())
                progressBarDownload.Value = e.ProgressPercentage;

            else
            {
                Action act = () => { progressBarDownload.Value = e.ProgressPercentage; };
                progressBarDownload.Dispatcher.Invoke(act);
            }
           
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            _downloadFinish = true;
            _fileToUnzip = Path.GetTempPath() + "recalbox.zip";
        }

        private async void StartDownload()
        {
            await _recalboxReleaseViewModel.GetUrlWithVersionName(_selectedItemRelease);
            DownloadZipFile(_recalboxReleaseViewModel.DownloadLink);
        }

        private void StartFormat()
        {
            _formatFinish = _driveManagerViewModel.FormatDrive(_selectedItemLetter);
        }

        public void UnzipFile()
        {
            while (!_downloadFinish || !_formatFinish) Thread.Sleep(100);

            using (ZipFile zip = ZipFile.Read(_fileToUnzip))
            {
                foreach (ZipEntry file in zip)
                {
                    file.Extract(_selectedItemLetter + @":\", ExtractExistingFileAction.OverwriteSilently);
                }
            }

            MessageBox.Show(
                "Now put your SD card in your Raspberry Pi","Work complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
