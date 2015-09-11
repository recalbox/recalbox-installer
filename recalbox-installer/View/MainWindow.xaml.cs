using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
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
        private char _selectedItemLetter;
        private Thread _threadDownload;
        private Thread _threadFormat;
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
            _threadDownload.Abort();
            _driveManagerViewModel.StopThread();
        }

        private void checkBoxBeta_Click(object sender, RoutedEventArgs e)
        {
            _recalboxReleaseViewModel.UpdateListRelease(checkBoxBeta.IsChecked.Value);
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
            _selectedItemRelease = comboBoxReleases.SelectedItem.ToString();
            _selectedItemLetter = char.Parse(comboBoxDriveLetter.SelectedItem.ToString().Remove(1));

            _threadDownload = new Thread(StartDownload);
            _threadFormat = new Thread(StartFormat);

            _threadDownload.Start();
            _threadFormat.Start();

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

    }
}
