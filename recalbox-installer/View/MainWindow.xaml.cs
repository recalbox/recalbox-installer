using System.Windows;
using recalbox_installer.ViewModel;

namespace recalbox_installer.View
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RecalboxReleaseViewModel _recalboxReleaseViewModel;

        public MainWindow()
        {
            InitializeComponent();

            _recalboxReleaseViewModel = new RecalboxReleaseViewModel();
            GridStepOne.DataContext = _recalboxReleaseViewModel;
            GridStepTwo.DataContext = new DriveManagerViewModel();
        }

        private void checkBoxBeta_Checked(object sender, RoutedEventArgs e)
        {
            _recalboxReleaseViewModel.UpdateListRelease(checkBoxBeta.IsChecked.Value);
        }
    }
}
