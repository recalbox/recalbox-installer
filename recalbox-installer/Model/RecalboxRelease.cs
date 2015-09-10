using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recalbox_installer.Model
{
    class RecalboxRelease
    {
        private string _name;
        private string _url;
        private bool _isPreRelease;

        public RecalboxRelease(string name, string url, bool isPreRelease)
        {
            Name = name;
            Url = url;
            IsPreRelease = isPreRelease;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public bool IsPreRelease
        {
            get { return _isPreRelease; }
            set { _isPreRelease = value; }
        }
    }
}
