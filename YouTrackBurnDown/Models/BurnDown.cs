using System.Collections.Generic;
using YouTrackBurnDown.Api;

namespace YouTrackBurnDown.Models
{
    public class BurnDown
    {

        public static BurnDown Load()
        {
            var client = new Client();
            var projects = client.GetProjects();

            return new BurnDown()
                {
                    Projects = projects
                };
        }

        public IEnumerable<Project> Projects { get; set; }

    }
}