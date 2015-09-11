using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using recalbox_installer.Annotations;
using recalbox_installer.Model;


namespace recalbox_installer.ViewModel
{
    class RecalboxReleaseViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _observableCollectionRecalbox;
        private List<Release> _releases;

        public RecalboxReleaseViewModel()
        {
            _observableCollectionRecalbox = new ObservableCollection<string>();
            GetReleaseFromGithub();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> RecalboxRelease
        {
            get { return _observableCollectionRecalbox; }
            set
            {
                _observableCollectionRecalbox = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void GetReleaseFromGithub()
        {

            // Work but dirty...
            /*
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.UserAgent] = "recalboxInstaller";
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                var json = client.DownloadString(@"https://api.github.com/repos/digitalLumberjack/recalbox-os/releases");
                var test = json;
             
            }*/


            // Don't work but I follow doc
            var client = new GitHubClient(new ProductHeaderValue("recalboxInstaller"));
            var releases = await client.Release.GetAll("digitalLumberjack", "recalbox-os");
            _releases = new List<Release>(releases);

            UpdateListRelease(false);

        }

        public void UpdateListRelease(bool beta)
        {

            RecalboxRelease = new ObservableCollection<string>();
            foreach (var release in _releases)
            {
                if ((release.Prerelease && beta) || (!release.Prerelease))
                {
                    RecalboxRelease.Add(release.Name);
                }
                   
            }
            
        }

       
    }
}
