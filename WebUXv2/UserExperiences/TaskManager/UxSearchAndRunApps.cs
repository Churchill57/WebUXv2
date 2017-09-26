using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.TaskManager
{
    [ComponentTitle("What do you want to do ?")]
    [PrimaryActionController("SearchAndRunApps", "TaskManager")]
    public class UxSearchAndRunApps : UserExperience
    {

        [ComponentState]
        public string SearchText { get; set; } = null;

        [ComponentState]
        public bool HideInstructions { get; set; } = false;

        [ComponentState]
        public bool RunBestMatch { get; set; } = false;

        public IEnumerable<TaskType> SearchResults()
        {
            var showAll = String.IsNullOrEmpty(SearchText) || SearchText.Trim() == "*";
            var results = (
                from assy in AppDomain.CurrentDomain.GetAssemblies()
                from type in assy.GetTypes()
                let launchAttr = type.GetCustomAttribute<LaunchableComponentAttribute>()
                where Attribute.IsDefined(type, typeof(LaunchableComponentAttribute))
                let titleAttr = type.GetCustomAttribute<ComponentTitleAttribute>().Title
                let matches = StringMatch(launchAttr.SearchTags, SearchText)
                where matches > 0 || showAll
                orderby matches descending, titleAttr.Replace("* ","zz") ascending
                select new TaskType() { Id = 0, RootComponentName = type.Name, Name = titleAttr, SearchTags = launchAttr.SearchTags, CriteriaMatchScore = matches}
            ).ToList();

            return results;
        }

        private int StringMatch(string subject, string find)
        {
            if (find == null) return 0;

            // The more space delimited parts of the 'find' string in the 'subject' string, the higher the match weight.
            // The earlier the 'find' string appears in the 'subject' string, the higher the match weight.
            int matchWeight = 0;
            int matchCount = 0;
            var parts = find.Split(' ');
            foreach (var part in parts)
            {
                var matchIndex = subject.IndexOf(part, StringComparison.InvariantCultureIgnoreCase);
                if (matchIndex != -1)
                {
                    matchCount += 1;
                    matchWeight += part.Length;
                    //matchWeight += part.Length * Math.Max(50 - matchIndex, 1);
                }
            }
            return matchWeight * matchCount;
        }

    }

}