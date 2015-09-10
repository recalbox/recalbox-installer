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
        private ObservableCollection<Release> _observableCollectionRecalbox;
        private List<Release> _releases;

        public RecalboxReleaseViewModel()
        {
            GetReleaseFromGithub();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Release> RecalboxRelease
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

        private void GetReleaseFromGithub()
        {
            var client = new GitHubClient(new ProductHeaderValue("recalboxInstaller"));
            var releases = client.Release.GetAll("digitalLumberjack", "recalbox-os").GetAwaiter().GetResult();
            //client.Release.GetAll("digitalLumberjack", "recalbox-os") += new Task<IReadOnlyList<Release>>(UpdateListRelease
            _releases = new List<Release>(releases);
            UpdateListRelease(false);

        }

        public void UpdateListRelease(bool beta)
        {
            foreach (var release in _releases)
            {
                if((release.Prerelease && beta) || (!release.Prerelease))
                    RecalboxRelease.Add(release);
            }
            
        }

       
    }
}
