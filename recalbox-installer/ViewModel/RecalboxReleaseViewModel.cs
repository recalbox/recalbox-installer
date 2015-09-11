using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Octokit;
using recalbox_installer.Annotations;
using recalbox_installer.Model;


namespace recalbox_installer.ViewModel
{
    class RecalboxReleaseViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _observableCollectionRecalbox;
        private List<Release> _releases;
        private string _downloadLink;

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

        public string DownloadLink
        {
            get { return _downloadLink; }
            set { _downloadLink = value; }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void GetReleaseFromGithub()
        {

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

        public async Task GetUrlWithVersionName(string vName)
        {
            Release release;
            List<ReleaseAsset> releaseAssets = new List<ReleaseAsset>();

            release = _releases.Find(x => x.Name == vName);

            var client = new GitHubClient(new ProductHeaderValue("recalboxInstaller"));
            var assets = await client.Release.GetAllAssets("digitalLumberjack", "recalbox-os", release.Id);

            foreach (var releaseAsset in assets)
            {
                if (releaseAsset.Name.Contains(".zip"))
                {
                    _downloadLink = releaseAsset.BrowserDownloadUrl;
                    break;
                }
            }
        }

    }
}
