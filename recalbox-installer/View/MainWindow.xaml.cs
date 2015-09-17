using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Ionic.Zip;
using Microsoft.Win32;
using Octokit;
using recalbox_installer.ViewModel;
using Label = System.Windows.Controls.Label;

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
            GridStepThree.DataContext = _driveManagerViewModel;
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
            if (_formatFinish)
            {
                if (textBoxFileDir.Text != "")
                {
                    _fileToUnzip = textBoxFileDir.Text;
                    _downloadFinish = true;
                }

                labelUnzipState.Content = "Waiting for download...";
                labelUnzipState.Foreground = Brushes.Red;

                _threadUnzip = new Thread(UnzipFile);
                _threadUnzip.Start();
            }
            else
                MessageBox.Show("You must format your SDCARD to FAT32 filesystem", "Format your SD card",
                    MessageBoxButton.OK, MessageBoxImage.Error);

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
            UpdateLabel(labelDownloadState, "Download complete", Brushes.Green);
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
            while (!_downloadFinish)
            {
                Thread.Sleep(1000);
            }

            UpdateLabel(labelFormatState, "OK", Brushes.Green);
            UpdateLabel(labelDownloadState, "Download complete", Brushes.Green);
            UpdateLabel(labelUnzipState, "Unzip recalbox.zip to " + _selectedItemLetter + @":\", Brushes.Red);

            using (ZipFile zip = ZipFile.Read(_fileToUnzip))
            {
                foreach (ZipEntry file in zip)
                {
                    file.Extract(_selectedItemLetter + @":\", ExtractExistingFileAction.OverwriteSilently);
                }
            }

            UpdateLabel(labelUnzipState, "Unzip complete", Brushes.Green);
         

            MessageBox.Show("Now put your SD card in your Raspberry Pi","Work complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateLabel(Label label, string  content, SolidColorBrush colorBrush)
        {
            if (label.Dispatcher.CheckAccess())
            {
                label.Content = content;
                label.Foreground = colorBrush;
            }
            else
            {
                Action act = () =>
                {
                    label.Content = content;
                    label.Foreground = colorBrush;

                };
                label.Dispatcher.Invoke(act);
            }
        }

        private void buttonFormatDevice_Click(object sender, RoutedEventArgs e)
        {
            string title = "WARNING";
            string message ="Backup all your data before formatting. Formatting will erase all data on the memory device. Do you want continue ?";
            MessageBoxResult messageBoxResult = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                _threadFormat = new Thread(StartFormat);
                _threadFormat.Start();
            }
        }

        private void comboBoxDriveLetter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBoxDriveLetter.SelectedItem == null) return;

            if ((string) comboBoxDriveLetter.SelectedItem != "")
            {
                if (_driveManagerViewModel.CheckDrive(comboBoxDriveLetter.SelectedItem.ToString().Substring(0, 1)))
                {
                    buttonFormatDevice.Content = "Already FAT32";
                }
                else
                {
                    buttonFormatDevice.Content = "Need Format !";
                }


            }
        }

        private void buttonDownload_Click(object sender, RoutedEventArgs e)
        {
            _selectedItemRelease = (string) comboBoxReleases.SelectedItem;

            _threadDownload = new Thread(StartDownload);
            _threadDownload.Start();
            labelDownloadState.Content = "Downloading...";
            labelDownloadState.Foreground = Brushes.Red;
        }

        private void comboBoxDriveLetterInstall_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBoxDriveLetter.SelectedItem == null) return;

            if ((string)comboBoxDriveLetter.SelectedItem != "")
            {
                if (_driveManagerViewModel.CheckDrive(comboBoxDriveLetter.SelectedItem.ToString().Substring(0, 1)))
                {                
                    UpdateLabel(labelFormatState, "OK", Brushes.Green);
                    _formatFinish = true;
                }
                else
                {
                    UpdateLabel(labelFormatState, "Need Format", Brushes.Red);
                    _formatFinish = false;
                }


            }
        }
    }
}
