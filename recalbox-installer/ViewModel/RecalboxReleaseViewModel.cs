using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using recalbox_installer.Annotations;
using recalbox_installer.Model;

namespace recalbox_installer.ViewModel
{
    class RecalboxReleaseViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<RecalboxRelease> _observableCollectionRecalbox; 

        public RecalboxReleaseViewModel()
        {
            getReleaseFromGithub(@"https://api.github.com/repos/digitalLumberjack/recalbox-os/releases");
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getReleaseFromGithub(string githubLink)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                var json = client.DownloadString(githubLink);

            }

        }
    }
}
