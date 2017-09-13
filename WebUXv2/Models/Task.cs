using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using WebUXv2.Components;

namespace WebUXv2.Models
{
    public class TaskType
    {
        public int Id { get; set; }

        [Required]
        public string Host { get; set; }

        public string Name { get; set; }

        [Required]
        [Display(Name = "Root Component Name")]
        public string RootComponentName { get; set; }

        [Display(Name = "Search Tags")]
        public string SearchTags { get; set; }
        public virtual ICollection<TaskInput> TaskInputs { get; set; }

        [NotMapped]
        public string SecondaryController { get; set; }

        [NotMapped]
        public string SecondaryAction { get; set; }

        [NotMapped]
        public int CriteriaMatchScore { get; set; }

    }

    public class TaskInput
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public virtual int TaskTypeId { get; set; }
        public virtual TaskType TaskType { get; set; }
    }

    public class LuTask
    {
        private LuTask() {}

        public LuTask(string userName)
        {
            UserName = userName;
        }

        [Display(Name = "Lu Id")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Lu Name")]
        public string Name { get; set; }
        public string Title { get; set; }


        [Required]
        public string ClientRef { get; set; }

        public string State { get; set; }


        [Required]
        public LogicalUnitStatusEnum Status { get; set; }


        public int? ParentLuTaskId { get; set; }
        public virtual LuTask ParentLuTask { get; set; }

        [Required]
        public string UserName { get; set; }

        [NotMapped]
        public string Cargo { get; set; }

    }

    public class UxTask
    {
        [Display(Name = "Ux Id")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ux Name")]
        public string Name { get; set; }
        public string Title { get; set; }

        [Required]
        public string ClientRef { get; set; }

        public string State { get; set; }

        public int ParentLuTaskId { get; set; }
        public virtual LuTask ParentLuTask { get; set; }

        [NotMapped]
        public string Cargo { get; set; }

    }

    public class SearchAndRunAppViewModel
    {
        public string SearchText { get; set; }
        public bool RunBestMatch { get; set; }

        [Display(Name = "Got it")]
        public bool HideInstructions { get; set; }
    }

    public class ActiveTask
    {
        public int TaskId { get; set; }
        public string Name { get; set; }
        public int UltimateParentLauncherTaskId { get; set; }
   }
    public class CompletedTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}