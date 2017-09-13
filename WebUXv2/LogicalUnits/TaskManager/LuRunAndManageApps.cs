using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebUXv2.Components;
using WebUXv2.Events.TaskManager;
using WebUXv2.UserExperiences.TaskManager;

namespace WebUXv2.LogicalUnits.TaskManager
{
    [ComponentTitle("")]
    //[SingletonComponent]
    //[LaunchableComponent("app application start run execute search find locate")]

    public class LuRunAndManageApps : LogicalUnit, ISingletonComponent
    {
        private string _nextClientRef = null;

        [ComponentState]
        internal string NextClientRef
        {
            get { return _nextClientRef; }
            set
            {
                LastClientRef = _nextClientRef;
                _nextClientRef = value;
            }
        }

        [ComponentState]
        internal string LastClientRef { get; set; } = null;

        [ComponentState]
        internal string ComponentNameToAdd { get; set; } = null;

        [ComponentState]
        internal string ComponentHost { get; set; } = null;

        [ComponentState]
        internal int? ComponentIdToAmend { get; set; } = null;

        [ComponentState]
        internal int? ComponentIdToDelete { get; set; } = null;

        [ComponentState]
        internal string SearchText { get; set; } = null;

        [ComponentState]
        internal int? ReturnTaskId { get; set; }

        internal const string SEARCH_AND_RUN_APPS = "SearchAndRunApps";
        internal const string SEARCH_AND_ADD_APP = "SearchAndAddApp";
        internal const string SEARCH_AND_ADD_SECONDARY_APP = "SearchAndAdd2ndApp";
        internal const string ADD_TASK_TYPE = "AddTaskType";
        internal const string AMEND_TASK_TYPE = "AmendTaskType";
        internal const string DELETE_TASK_TYPE = "DeleteTaskType";
        internal const string MY_APPS = "MyApps";
        internal const string MY_2ND_APPS = "My2ndApps";
        internal const string MY_2ND_APPS_DONE = "2ndAppsDone";
        internal const string ACTIVE_TASKS = "ActiveTasks";
        internal const string COMPLETED_TASKS = "CompletedTasks";
        internal const string ABORT_TASK = "AbortTask";
        internal const string APP_HOST = "app";

        public LuRunAndManageApps()
        {
            InitialiseState();
        }

        // TODO: Consider doing for all uxs on this lu.
        public UxMy2ndApps UxMy2ndApps => GetUserExperience<UxMy2ndApps>(MY_2ND_APPS);

        public void InitialiseState() { } // This long running singleton component relies on Command property being set to steer work flow. 

        [ComponentInput()]
        public string Command
        {
            get { return null; }
            set
            {
                // This is a singleton component so technically doesn't ever have to complete.
                // However, if it does have to complete, the next command issued will re-start it.
                //Status=LogicalUnitStatusEnum.Started;

                if (value.StartsWith(ABORT_TASK, StringComparison.InvariantCultureIgnoreCase))
                {
                    int taskIdToAbort;
                    if (int.TryParse((value + "|").Split('|')[1], out taskIdToAbort))
                    {
                        AbortTask(taskIdToAbort);
                    }
                    ComponentHost = APP_HOST;

                }

                if (value.StartsWith(ACTIVE_TASKS, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = ACTIVE_TASKS;
                }

                if (value.StartsWith(COMPLETED_TASKS, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = COMPLETED_TASKS;
                }

                if (value.StartsWith(SEARCH_AND_RUN_APPS, StringComparison.InvariantCultureIgnoreCase))
                    if (value.StartsWith(SEARCH_AND_RUN_APPS, StringComparison.InvariantCultureIgnoreCase))
                    {
                        NextClientRef = SEARCH_AND_RUN_APPS;
                    // SearchText will later be used as initial value for appropriate ux.
                    if (value.Contains("|")) SearchText = value .Split('|')[1];
                }

                if (value.StartsWith(SEARCH_AND_ADD_APP, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = SEARCH_AND_ADD_APP;
                    if (value.Contains("|")) SearchText = value .Split('|')[1];
                    ComponentHost = (value + "||").Split('|')[2];
                }

                if (value.StartsWith(SEARCH_AND_ADD_SECONDARY_APP, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = SEARCH_AND_ADD_SECONDARY_APP;
                    if (value.Contains("|")) SearchText = value .Split('|')[1];
                    ComponentHost = (value + "||").Split('|')[2];
                }

                if (value.StartsWith(ADD_TASK_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = ADD_TASK_TYPE;
                    ComponentNameToAdd = (value + "|").Split('|')[1];
                    ComponentHost = (value + "||").Split('|')[2];
                }

                if (value.StartsWith(AMEND_TASK_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = AMEND_TASK_TYPE;
                    int componentIdToAmend;
                    if (int.TryParse((value + "|").Split('|')[1], out componentIdToAmend)) ComponentIdToAmend = componentIdToAmend;
                }

                if (value.StartsWith(DELETE_TASK_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = DELETE_TASK_TYPE;
                    int componentIdToDelete;
                    if (int.TryParse((value + "|").Split('|')[1], out componentIdToDelete)) ComponentIdToDelete = componentIdToDelete;
                }

                if (value.StartsWith(MY_APPS, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = MY_APPS;
                    ComponentHost = APP_HOST;
                }

                if (value.StartsWith(MY_2ND_APPS, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = MY_2ND_APPS;
                    ReturnTaskId = null;
                    int returnTaskId;
                    if (int.TryParse((value + "|").Split('|')[1], out returnTaskId))
                    {
                        var ux = TaskMan.GetUserExperience(returnTaskId);
                        if (ux != null)
                        {
                            ReturnTaskId = returnTaskId;
                            ComponentHost = ux.GetType().Name;
                            //ComponentHost = TaskMan.GetComponentTitle(ux);
                            if (string.IsNullOrEmpty(ComponentHost)) ComponentHost = ux.GetType().Name;
                        }
                    }
                }

            }
        }

        private void AbortTask(int id)
        {
            var luTask = TaskMan.GetLuTask(id);
            if (luTask == null) return;
            luTask.Status=LogicalUnitStatusEnum.Aborted;
            TaskMan.TaskDbContext.Entry(luTask).State = EntityState.Modified;
            TaskMan.TaskDbContext.SaveChanges();
        }

        public override Component GetNextComponent()
        {
            if (String.Equals(NextClientRef, ACTIVE_TASKS, StringComparison.InvariantCultureIgnoreCase))
            {
                return GetUserExperience<UxActiveTasks>(ACTIVE_TASKS);
            }

            if (String.Equals(NextClientRef, COMPLETED_TASKS, StringComparison.InvariantCultureIgnoreCase))
            {
                return GetUserExperience<UxCompletedTasks>(COMPLETED_TASKS);
            }

            if (String.Equals(NextClientRef, MY_2ND_APPS_DONE, StringComparison.InvariantCultureIgnoreCase))
            {
                if (ReturnTaskId.HasValue) return TaskMan.GetUserExperience(ReturnTaskId.Value);
            }

            if (String.Equals(NextClientRef, MY_APPS, StringComparison.InvariantCultureIgnoreCase))
            {
                var uxMyApps = GetUserExperience<UxMyApps>(MY_APPS);
                uxMyApps.Host = ComponentHost;
                //uxMyApps.Title = "My Apps";
                return uxMyApps;
            }

            if (String.Equals(NextClientRef, MY_2ND_APPS, StringComparison.InvariantCultureIgnoreCase))
            {
                var uxMy2ndApps = UxMy2ndApps;
                uxMy2ndApps.Host = ComponentHost;
                //uxMy2ndApps.Title = $"{ComponentHost} - Secondary Tasks";
                return uxMy2ndApps;
            }

            if (String.Equals(NextClientRef, ADD_TASK_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                var uxAddTaskType = GetUserExperience<UxAddTaskType>(ADD_TASK_TYPE);
                uxAddTaskType.ComponentNameToAdd = ComponentNameToAdd;
                uxAddTaskType.ComponentHostToAdd = ComponentHost;
                //if (String.Equals(LastClientRef, SEARCH_AND_ADD_SECONDARY_APP, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    //uxAddTaskType.Title = $"{ComponentHost} - Add Secondary Task";
                //}
                //else
                //{
                //    //uxAddTaskType.Title = "Add to My Apps";
                //}

                return uxAddTaskType;
            }

            if (String.Equals(NextClientRef, AMEND_TASK_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                var uxAmendTaskType = GetUserExperience<UxAmendTaskType>(AMEND_TASK_TYPE);
                uxAmendTaskType.ComponentId = ComponentIdToAmend;
                uxAmendTaskType.ComponentHost = ComponentHost;
                //if (String.Equals(LastClientRef, MY_APPS, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    uxAmendTaskType.Title = "Amend My App";
                //}
                //else
                //{
                //    uxAmendTaskType.Title = $"{ComponentHost} - Amend Secondary Task";
                //}
                return uxAmendTaskType;
            }

            if (String.Equals(NextClientRef, DELETE_TASK_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                var uxDeleteTaskType = GetUserExperience<UxDeleteTaskType>(DELETE_TASK_TYPE);
                uxDeleteTaskType.ComponentId = ComponentIdToDelete;
                uxDeleteTaskType.ComponentHost = ComponentHost;
                return uxDeleteTaskType;
            }

            if (String.Equals(NextClientRef, SEARCH_AND_ADD_APP, StringComparison.InvariantCultureIgnoreCase))
            {
                var uxSearchAndAddApp = GetUserExperience<UxSearchAndAddApp>(SEARCH_AND_ADD_APP);
                if (SearchText != null) uxSearchAndAddApp.SearchText = SearchText;
                uxSearchAndAddApp.AppHost = ComponentHost;
                return uxSearchAndAddApp;
            }

            if (String.Equals(NextClientRef, SEARCH_AND_ADD_SECONDARY_APP, StringComparison.InvariantCultureIgnoreCase))
            {
                var uxSearchAndAddApp = GetUserExperience<UxSearchAndAddApp>(SEARCH_AND_ADD_APP);
                if (SearchText != null) uxSearchAndAddApp.SearchText = SearchText;
                uxSearchAndAddApp.AppHost = ComponentHost;
                return uxSearchAndAddApp;
            }

            // Default navigation to search and run apps. 
            NextClientRef = SEARCH_AND_RUN_APPS;
            var uxSearchAndRunApps =  GetUserExperience<UxSearchAndRunApps>(SEARCH_AND_RUN_APPS);
            // N.B. SearchText property of this lu (if specified) is only used as starting value for ux.
            // Otherwise last ux SearchText takes precedence.
            if (SearchText != null) uxSearchAndRunApps.SearchText = SearchText;
            return uxSearchAndRunApps;
        }

        public override Component GetPrevComponent()
        {
            return GetNextComponent();
        }

        public override void ComponentCompleted(Component comp)
        {
            if (comp.ClientRef == MY_2ND_APPS)
            {
                NextClientRef = MY_2ND_APPS_DONE;
            }
            if (comp.ClientRef == ADD_TASK_TYPE)
            {
                if (String.Equals(LastClientRef, SEARCH_AND_ADD_SECONDARY_APP, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = MY_2ND_APPS;
                }
                else
                {
                    NextClientRef = MY_APPS;
                }
            }
            if (comp.ClientRef == AMEND_TASK_TYPE)
            {
                if (String.Equals(LastClientRef, MY_2ND_APPS, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = MY_2ND_APPS;
                }
                else
                {
                    NextClientRef = MY_APPS;
                }
                ComponentIdToAmend = null;
            }
            if (comp.ClientRef == DELETE_TASK_TYPE)
            {
                if (String.Equals(LastClientRef, MY_2ND_APPS, StringComparison.InvariantCultureIgnoreCase))
                {
                    NextClientRef = MY_2ND_APPS;
                }
                else
                {
                    NextClientRef = MY_APPS;
                }
                ComponentIdToDelete = null;
            }
        }

        public override void HandleComponentEvent(ComponentEventArgs eventArgs)
        {
            if (eventArgs is BackEventArgs)
            {
                if (String.Equals(eventArgs.Component.ClientRef, ADD_TASK_TYPE, StringComparison.InvariantCultureIgnoreCase)
                 || String.Equals(eventArgs.Component.ClientRef, SEARCH_AND_ADD_APP, StringComparison.InvariantCultureIgnoreCase)
                 || String.Equals(eventArgs.Component.ClientRef, AMEND_TASK_TYPE, StringComparison.InvariantCultureIgnoreCase)
                 || String.Equals(eventArgs.Component.ClientRef, DELETE_TASK_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (String.Equals(LastClientRef, SEARCH_AND_RUN_APPS, StringComparison.InvariantCultureIgnoreCase))
                    {
                        NextClientRef = SEARCH_AND_RUN_APPS;
                    }
                    else if (LastClientRef.Contains("2nd")) 
                    {
                        NextClientRef = MY_2ND_APPS;
                    }
                    else
                    {
                        NextClientRef = MY_APPS;
                    }
                }
            }
        }


        private void IgnoreException(Action someCode)
        {
            try
            {
                someCode.Invoke();
            }
            catch (Exception)
            {
                // Ignore exception.
            }
        }

    }
}