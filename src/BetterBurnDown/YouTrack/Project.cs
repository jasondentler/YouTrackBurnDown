using System.Collections.Generic;
using Newtonsoft.Json;

namespace BetterBurnDown.YouTrack
{
    public class Project
    {

        public string Versions { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string Description { get; set; }

        public bool IsImporting { get; set; }

        public List<Item> Subsystems { get; set; }

        public List<Item> AssigneesLogin { get; set; }

        public List<Item> AssigneesFullName { get; set; }

        public string[] Sprints
        {
            get
            {
                return Versions == null
                           ? new string[0]
                           : JsonConvert.DeserializeObject<string[]>(Versions);
            }
        }

    }
}