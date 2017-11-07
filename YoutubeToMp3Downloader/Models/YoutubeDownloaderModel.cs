using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace YoutubeToMp3Downloader
{
    public sealed class YoutubeReqeust
    {
        private readonly string[] _replacements = new string[] {
            "https://www.youtube.com/watch?v=",
            "http://www.youtube.com/watch?v=",
            "https://youtu.be/",
            "http://youtu.be/"
        };

        public string Url { get; set; }

        public string VideoID
        {
            get
            {
                StringBuilder _builder;

                _builder = new StringBuilder();
                _builder.Append(this.Url);

                foreach (var _item in this._replacements)
                    _builder.Replace(_item, string.Empty);

                return _builder.ToString();
            }
        }
    }

    public sealed class YoutubeInfo
    {
        public string VideoID { get; set; }
        public string Name { get; set; }
    }
}