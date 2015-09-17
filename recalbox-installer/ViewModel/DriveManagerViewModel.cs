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
        private Thread _threadUpdate;
        private bool _stopThread; 


        public DriveManagerViewModel()
        {
            _stopThread = false;
            _threadUpdate = new Thread(this.UpdateDriveAvailable);
            _threadUpdate.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> DriveLetter
        {
            get { return _observableCollectionDriveLetter; }
            set
            {
                _observableCollectionDriveLetter = value;
                OnPropertyChanged();
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
    
            while (!_stopThread)
            {
                if (listOfAvailableDrive.Count != DriveManager.GetAllDrive().Count)
                {
                    listOfAvailableDrive = DriveManager.GetAllDrive();
                    DriveLetter = new ObservableCollection<string>(listOfAvailableDrive);   
                }
                Thread.Sleep(100);
            }

        }

        public void StopThread()
        {
            _stopThread = true;
        }

        public bool FormatDrive(char letter)
        {
            return DriveManager.FormatDrive(letter, "RecalboxOs", "FAT32");
        }

        public bool CheckDrive(string driveLetter)
        {
            return DriveManager.IsFat32(driveLetter);
        }
    }
}
