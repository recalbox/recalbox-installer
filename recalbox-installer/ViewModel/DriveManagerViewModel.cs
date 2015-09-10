using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using recalbox_installer.Annotations;
using recalbox_installer.Model;

namespace recalbox_installer.ViewModel
{
    class DriveManagerViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _observableCollectionDriveLetter;

        public DriveManagerViewModel()
        {
            // _observableCollectionDriveLetter = new ObservableCollection<string>(DriveManager.GetAllDrive());
            Thread threadUpdate = new Thread(this.UpdateDriveAvailable);
            threadUpdate.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> DriveLetter
        {
            get { return _observableCollectionDriveLetter; }
            set
            {
                _observableCollectionDriveLetter = value;
                OnPropertyChanged("DriveLetter");
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateDriveAvailable()
        {
            List<string> listOfAvailableDrive = new List<string>();
    
            while (true)
            {
                if (listOfAvailableDrive.Count != DriveManager.GetAllDrive().Count)
                {
                    listOfAvailableDrive = DriveManager.GetAllDrive();
                    DriveLetter = new ObservableCollection<string>(listOfAvailableDrive);   
                }
                Thread.Sleep(100);
            }

        }
    }
}
