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
           
            
            Thread threadDownload = new Thread(StartDownload);
            Thread threadFormat = new Thread(StartFormat);

            threadDownload.Start();
          
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
            progressBarDownload.Value = e.ProgressPercentage;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
          //  DownloadComplete = true;
            MessageBox.Show("Download completed!");
        }

        private async void StartDownload()
        {
            await _recalboxReleaseViewModel.GetUrlWithVersionName(comboBoxReleases.SelectedItem.ToString());
            DownloadZipFile(_recalboxReleaseViewModel.DownloadLink);
        }

        private void StartFormat()
        {
            throw new NotImplementedException();
        }
    }
}
