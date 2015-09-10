using System.Windows;
using recalbox_installer.ViewModel;

namespace recalbox_installer.View
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            InitializeComponent();
            
            GridStepOne.DataContext = new RecalboxReleaseViewModel();
            GridStepTwo.DataContext = new DriveManagerViewModel();
        }
    }
}
