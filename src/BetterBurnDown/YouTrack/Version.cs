using System;

namespace BetterBurnDown.YouTrack
{
    public class Version
    {
        public bool Released { get; set; }
        public bool Archived { get; set; }
        public long ReleaseDate { get; set; }
        public string Value { get; set; }

        public DateTime ReleaseDateAsDate { get { return Client.Convert(ReleaseDate); } }
    }
}