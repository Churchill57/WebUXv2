using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using WebUXv2.LogicalUnits.TaskManager;
using WebUXv2.Models;

namespace WebUXv2.Components
{
    //[System.Runtime.InteropServices.Guid("2943CDA2-071A-4AE7-B0CB-6A326E9D3923")]
    public class TaskManager : ITaskManager
    {
        private readonly TaskDbContext _taskDbContext;

        public TaskManager() : this(new TaskDbContext())
        {
            
        }
        public TaskManager(TaskDbContext taskDbContext)
        {
            _taskDbContext = taskDbContext;
        }

        public TaskDbContext TaskDbContext => _taskDbContext;

        public UxTask GetUxTask(int uxTaskId)
        {
            return _taskDbContext.UxTasks.FirstOrDefault(t => t.Id == uxTaskId);
        }

        public T GetSingletonComponent<T>() where T : Component, ISingletonComponent
        {
            var componentName = typeof(T).Name;
            if (!ComponentIsSingleton<T>()) throw new NotImplementedException($"Component {componentName} is not marked as a singleton");

            var lu = _taskDbContext.LuTasks.FirstOrDefault(t => t.Name == typeof(T).Name);
            if (lu != null) return GetLogicalUnit(lu.Id) as T;
            var ux = _taskDbContext.UxTasks.FirstOrDefault(t => t.Name == componentName);
            if (ux != null) return GetUserExperience(ux.Id) as T;

            return null;
        }

        public LuTask GetLuTask(int luTaskId)
        {
            return _taskDbContext.LuTasks.FirstOrDefault(t => t.Id == luTaskId);
        }

        public Type GetType(string typename)
        {
            var result = (
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                select t
            ).FirstOrDefault(t => t.Name == typename);
            return result;
        }


        public T GetLogicalUnit<T>(LogicalUnit parentLu, string clientRef, Action<T> initializer = null) where T : LogicalUnit, new()
        {

            if (ComponentIsSingleton<T>())
            {
                var luSingletonTask = _taskDbContext.LuTasks.FirstOrDefault(t => t.Name == typeof(T).Name && t.ClientRef== clientRef && t.ParentLuTaskId == parentLu.TaskId);
                var initialize = (luSingletonTask == null);

                luSingletonTask = _taskDbContext.LuTasks.FirstOrDefault(t => t.Name == typeof(T).Name);
                if (luSingletonTask != null)
                {
                    var luSingleton = GetLogicalUnit(luSingletonTask) as T;
                    if (luSingleton != null)
                    {
                        if (initialize) initializer?.Invoke(luSingleton);
                        luSingletonTask.ClientRef = clientRef;
                        luSingletonTask.ParentLuTaskId = parentLu.TaskId;
                        luSingletonTask.State = ObjectToJson(luSingleton.State);
                        luSingletonTask.Title = luSingleton.Title();
                        _taskDbContext.Entry(luSingletonTask).State = EntityState.Modified;
                        _taskDbContext.SaveChanges();
                        return luSingleton;
                    }
                }
            }

            var luTask = _taskDbContext.LuTasks.FirstOrDefault(t => t.ParentLuTaskId == parentLu.TaskId && t.ClientRef == clientRef);

            if (luTask != null) return GetLogicalUnit(luTask) as T;

            var lu = (T)CreateInstance<T>();
            //var lu = new T();

            initializer?.Invoke(lu);

            var newLuTask = new LuTask(parentLu.UserName)
            {
                Name = typeof(T).Name,
                ClientRef = clientRef,
                State = ObjectToJson(lu.State),
                Status = LogicalUnitStatusEnum.Started,
                Title=lu.Title(),
                ParentLuTaskId = parentLu.TaskId,
                ParentLuTask = null
            };

            _taskDbContext.LuTasks.Add(newLuTask);
            _taskDbContext.SaveChanges();

            lu.TaskId = newLuTask.Id;
            lu.Status = LogicalUnitStatusEnum.Started;

            return lu;

        }

        public LogicalUnit GetLogicalUnit(LogicalUnit parentLu, string luTypeName, string userName, string clientRef)
        {
            var luTask = _taskDbContext.LuTasks.FirstOrDefault(t => t.ParentLuTaskId == parentLu.TaskId && t.Name == luTypeName && t.ClientRef == clientRef);

            if (luTask != null) return GetLogicalUnit(luTask);

            var lu = CreateInstanceOfTypeName(luTypeName) as LogicalUnit;
            if (lu == null) return null;

            var newLuTask = new LuTask(userName)
            {
                Name = luTypeName,
                ClientRef = clientRef,
                State = ObjectToJson(lu.State),
                Title = lu.Title(),
                Status = LogicalUnitStatusEnum.Started,
                ParentLuTaskId = parentLu.TaskId,
                //ParentLuTask = _taskDbContext.LuTasks.Find(parentLu.TaskId)
            };

            _taskDbContext.LuTasks.Add(newLuTask);
            _taskDbContext.SaveChanges();

            lu.TaskId = newLuTask.Id;
            lu.Status = LogicalUnitStatusEnum.Started;

            return lu;
        }
        public LogicalUnit GetLogicalUnit(int id)
        {
            var luTask = _taskDbContext.LuTasks.FirstOrDefault(x => x.Id == id);
            return GetLogicalUnit(luTask);
        }

        public LogicalUnit GetLogicalUnit(LuTask luTask)
        {
            if (luTask != null)
            {
                var lu = CreateInstanceOfTypeName(luTask.Name) as LogicalUnit;
                if (lu != null)
                {
                    lu.TaskId = luTask.Id;
                    lu.ClientRef = luTask.ClientRef;
                    lu.State = JsonToObject<Dictionary<string, object>>(luTask.State);
                    //TODO: EntityContext properties .Description may have changed since lu was last hydrated & run. Update these automatically.
                    lu.LastSetState = luTask.State;
                    lu.Status = luTask.Status;
                    lu.UserName = luTask.UserName;
                    return lu;
                }
            }
            return null;
        }

        public UserExperience GetUserExperience(LogicalUnit parentLu, string uxTypeName, string clientRef)
        {
            var uxTask = _taskDbContext.UxTasks.FirstOrDefault(t => t.ParentLuTaskId == parentLu.TaskId && t.Name == uxTypeName && t.ClientRef == clientRef);

            if (uxTask != null) return GetUserExperience(uxTask);

            var ux = CreateInstanceOfTypeName(uxTypeName) as UserExperience;
            if (ux == null) return null;

            var newUxTask = new UxTask()
            {
                Name = uxTypeName,
                ClientRef = clientRef,
                State = ObjectToJson(ux.State),
                Title = ux.Title(),
                ParentLuTaskId = parentLu.TaskId,
                //ParentLuTask = _taskDbContext.LuTasks.Find(parentLu.TaskId)
            };

            _taskDbContext.UxTasks.Add(newUxTask);
            _taskDbContext.SaveChanges();

            ux.TaskId = newUxTask.Id;

            return ux;
        }

        public T GetRefreshedUserExperience<T>(LogicalUnit parentLu, string clientRef) where T : UserExperience, new()
        {
            var uxTask = _taskDbContext.UxTasks.FirstOrDefault(t => t.ParentLuTaskId == parentLu.TaskId && t.ClientRef == clientRef);

            var objectContext = ((IObjectContextAdapter)_taskDbContext).ObjectContext;
            objectContext.Refresh(RefreshMode.StoreWins, uxTask);

            return GetUserExperience(uxTask) as T;
        }

        public T GetUserExperience<T>(LogicalUnit parentLu, string clientRef, Action<T> initializer = null) where T : UserExperience, new()
        {
            var uxTask = _taskDbContext.UxTasks.FirstOrDefault(t => t.ParentLuTaskId == parentLu.TaskId && t.ClientRef == clientRef);

            if (uxTask != null) return GetUserExperience(uxTask) as T;

            var ux = (T)CreateInstance<T>();
            //var ux = new T();

            initializer?.Invoke(ux);

            var newUxTask = new UxTask()
            {
                Name = typeof(T).Name,
                ClientRef = clientRef,
                State = ObjectToJson(ux.State),
                Title = ux.Title(),
                ParentLuTaskId = parentLu.TaskId,
                //ParentLuTask = _taskDbContext.LuTasks.Find(parentLu.TaskId)
            };

            _taskDbContext.UxTasks.Add(newUxTask);
            _taskDbContext.SaveChanges();

            ux.TaskId = newUxTask.Id;

            return ux;

        }

        //public UserExperience GetRefreshedUserExperience(int id)
        //{
        //    var uxTask = _taskDbContext.UxTasks.FirstOrDefault(x => x.Id == id);

        //    var objectContext = ((IObjectContextAdapter)_taskDbContext).ObjectContext;
        //    objectContext.Refresh(RefreshMode.StoreWins, uxTask);

        //    return GetUserExperience(uxTask);
        //}

        public UserExperience GetUserExperience(int id)
        {
            var uxTask = _taskDbContext.UxTasks.FirstOrDefault(x => x.Id == id);
            return GetUserExperience(uxTask);
        }

        public async Task<UserExperience> GetUserExperienceAsync(int id)
        {
            var uxTask = await _taskDbContext.UxTasks.FirstOrDefaultAsync(x => x.Id == id);
            return GetUserExperience(uxTask);
        }

        public UserExperience GetUserExperience(UxTask uxTask)
        {
            if (uxTask != null)
            {
                var ux = CreateInstanceOfTypeName(uxTask.Name) as UserExperience;
                if (ux != null)
                {
                    ux.TaskId = uxTask.Id;
                    ux.ClientRef = uxTask.ClientRef;
                    ux.State = JsonToObject<Dictionary<string, object>>(uxTask.State);
                    //TODO: EntityContext properties .Description may have changed since ux was last hydrated & run. Update these automatically.
                    ux.LastSetState = uxTask.State;
                    return ux;
                }
            }
            return null;
        }

        public object CreateInstanceOfTypeName(string typeName)
        {
            var type = GetType(typeName);
            if (type == null) return null;

            var obj = Activator.CreateInstance(type);
            var component = obj as Component;
            if (component != null) component.TaskMan = this;
            return obj;
        }

        public object CreateInstance<T>() where T: new()
        {
            var obj = new T();

            var component = obj as Component;
            if (component != null) component.TaskMan = this;
            return obj;

        }

        public string ObjectToJson(object obj)
        {
            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};
            return JsonConvert.SerializeObject(obj, settings);
            
            //MemoryStream stream1 = new MemoryStream();
            //DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
            //ser.WriteObject(stream1, obj);
            //stream1.Position = 0;
            //StreamReader sr = new StreamReader(stream1);
            //return sr.ReadToEnd();
        }

        public T JsonToObject<T>(string jsonData)
        {
            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};
            var obj = JsonConvert.DeserializeObject<T>(jsonData, settings);
            return obj;

            //MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonData));
            //DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            //T result = (T)ser.ReadObject(ms);
            //ms.Close();
            //return result;
        }

        //public string GetComponentTitle(object obj)
        //{
        //    return GetComponentTitle(obj.GetType());
        //}

        //public string GetComponentTitle<T>()
        //{
        //    return GetComponentTitle(typeof(T));
        //}

        //public string GetComponentTitle(Component component)
        //{
        //    string title = component.Title;
        //    if (title != null) return title;

        //    if (component.GetType().IsDefined(typeof(ComponentTitleAttribute), false))
        //    {
        //        return component.GetType().GetCustomAttribute<ComponentTitleAttribute>().Title;
        //    }

        //    return $"<undefined title on {component.GetType().Name}>";
        //}

        public string GetComponentTitle(Type type)
        {
            if (!type.IsDefined(typeof(ComponentTitleAttribute), false)) return null;
            var titleAttr = type.GetCustomAttribute<ComponentTitleAttribute>();
            return titleAttr.Title;
        }

        public string GetComponentTitle(string typename)
        {
            return GetComponentTitle(GetType(typename));
        }


        public bool UserHasAccessToComponent(IPrincipal user, Component component)
        {
            var type = component.GetType();
            if (!type.IsDefined(typeof(AuthorizeAttribute), false)) return true;
            var authAttr = type.GetCustomAttribute<AuthorizeAttribute>();
            return authAttr.Roles.Split(',').Any(r => user.IsInRole(r));
        }

        public RouteValueDictionary GetComponentInputParams(object obj)
        {
            var inputParams = new RouteValueDictionary();
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var attr = propertyInfo.GetCustomAttributes(typeof(ComponentInputAttribute), false);
                if (attr.Length == 1)
                {
                    inputParams.Add(propertyInfo.Name, propertyInfo.GetValue(obj)?.ToString());
                }
            }
            return inputParams;
        }

        public string GetContextDescription(object obj)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType != typeof(EntityContext)) continue;
                var attr = propertyInfo.GetCustomAttributes(typeof(ComponentInputAttribute), false);
                if (attr.Length == 1)
                {
                    var entityContext = propertyInfo.GetValue(obj) as EntityContext;
                    return entityContext?.Description;
                }
            }
            return null;
        }

        //public string GetComponentTitle(string typename)
        //{
        //    return GetComponentTitle(GetType(typename));
        //}

        public IEnumerable<string> GetUserExperienceTaskBreadcrumbs(int uxTaskId)
        {
            var breadcrumbs = new List<string>();
            var uxTask = GetUxTask(uxTaskId);
            if (uxTask != null)
            {
                breadcrumbs.Add(uxTask.Name);
                var luTask = GetLuTask(uxTask.ParentLuTaskId);
                while (luTask.ParentLuTaskId.HasValue)
                {
                    breadcrumbs.Add(luTask.Name);
                    luTask = GetLuTask(luTask.ParentLuTaskId.Value);
                }
                breadcrumbs.Add(luTask.Name);
            }
            return breadcrumbs;
        }


        //public IEnumerable<string> GetUserExperienceTaskBreadcrumbs(int uxTaskId)
        //{
        //    var breadcrumbs = new List<string>();
        //    var uxTask = GetUxTask(uxTaskId);
        //    if (uxTask != null)
        //    {
        //        breadcrumbs.Add(uxTask.Name);
        //        var luTask = GetLuTask(uxTask.ParentLuTaskId);
        //        while (luTask.ParentLuTaskId.HasValue)
        //        {
        //            breadcrumbs.Add(luTask.Name);
        //            luTask = GetLuTask(luTask.ParentLuTaskId.Value);
        //        }
        //    }
        //    return breadcrumbs;
        //}

        //public string GetUltimateParentLogicalUnitTitle(int uxTaskId)
        //{
        //    var title = "<indeterminate ultimate parent logical unit title>";
        //    var uxTask = GetUxTask(uxTaskId);
        //    if (uxTask != null)
        //    {
        //        var luTask = GetLuTask(uxTask.ParentLuTaskId);
        //        while ((luTask = GetLuTask(luTask.ParentLuTaskId ?? 0)) != null)
        //        title = GetComponentTitle(luTask.Name);
        //    }
        //    return title;
        //}

        //public string GetUltimateParentLogicalUnitTitle(int uxTaskId)
        //{
        //    var title = "<indeterminate ultimate parent logical unit title>";
        //    var uxTask = GetUxTask(uxTaskId);
        //    if (uxTask != null)
        //    {
        //        var luTask = GetLuTask(uxTask.ParentLuTaskId);
        //        while (luTask.ParentLuTaskId.HasValue)
        //        {
        //            luTask = GetLuTask(luTask.ParentLuTaskId.Value);
        //        }
        //        title = GetComponentTitle(luTask.Name);
        //    }
        //    return title;
        //}

        //public string GetUltimateParentLauncherLogicalUnitTitle(int uxTaskId)
        //{
        //    var luTask = GetUltimateParentLauncherLogicalUnitTask(uxTaskId);
        //    return ((LuLauncher)GetLogicalUnit(luTask)).Title; ;

        //}

        public LuTask GetUltimateParentLauncherLogicalUnitTask(int uxTaskId)
        {
            LuTask luTask = null;
            var uxTask = GetUxTask(uxTaskId);
            if (uxTask != null)
            {
                luTask = GetLuTask(uxTask.ParentLuTaskId);
                while (luTask.ParentLuTaskId.HasValue)
                {
                    luTask = GetLuTask(luTask.ParentLuTaskId.Value);
                }
            }
            return luTask;
        }



        public string GetUltimateParentLogicalUnitTitle(int uxTaskId)
        {
            var luTask = GetUltimateParentLogicalUnitTask(uxTaskId);
            return luTask.Title;
            //return ((LuLauncher)GetLogicalUnit(luTask)).Title(); ;

        }

        public LuTask GetUltimateParentLogicalUnitTask(int uxTaskId)
        {
            LuTask result = null;
            LuTask luTask = null;
            var uxTask = GetUxTask(uxTaskId);
            if (uxTask != null)
            {
                luTask = GetLuTask(uxTask.ParentLuTaskId);
                result = luTask;
                while (luTask.ParentLuTaskId.HasValue)
                {
                    result = luTask;
                    luTask = GetLuTask(luTask.ParentLuTaskId.Value);
                }
            }
            return result;

            //LuTask luTask = null;
            //var uxTask = GetUxTask(uxTaskId);
            //if (uxTask != null)
            //{
            //    luTask = GetLuTask(uxTask.ParentLuTaskId);
            //    while (luTask.ParentLuTaskId.HasValue)
            //    {
            //        luTask = GetLuTask(luTask.ParentLuTaskId.Value);
            //    }
            //}
            //return luTask;
        }

        public int GetParentLauncherRootComponentTaskId(int uxTaskId)
        {
            int result = 0;
            LuTask luTask = null;
            var uxTask = GetUxTask(uxTaskId);
            if (uxTask != null)
            {
                luTask = GetLuTask(uxTask.ParentLuTaskId);
                result = luTask.Id;
                while (luTask.Name != typeof(LuLauncher).Name)
                {
                    result = luTask.Id;
                    luTask = GetLuTask(luTask.ParentLuTaskId.Value);
                }
            }
            return result;
        }


        //public string GetExecutingUserExperienceTitle(int uxTaskId)
        //{
        //    var title = "<indeterminate user experience title>";
        //    var uxTask = GetUxTask(uxTaskId);
        //    if (uxTask != null)
        //    {
        //        //title = GetComponentTitle(uxTask.Name);
        //        title = uxTask.Title;
        //    }
        //    return title;
        //}

        public bool ComponentIsSingleton(object obj)
        {
            return ComponentIsSingleton(obj.GetType());
        }

        public bool ComponentIsSingleton<T>() where T : Component
        {
            return ComponentIsSingleton(typeof(T));
        }

        public bool ComponentIsSingleton(Type type)
        {
            return typeof(ISingletonComponent).IsAssignableFrom(type);
        }
        public bool ComponentIsSingleton(string typename)
        {
            return ComponentIsSingleton(GetType(typename));
        }

        public void SaveLogicalUnit(LogicalUnit lu)
        {
            if (!lu.IsDirty) return;

            var luTask = _taskDbContext.LuTasks.FirstOrDefault(x => x.Id == lu.TaskId);
            if (luTask == null) return;

            luTask.State = ObjectToJson(lu.State);
            luTask.Title = lu.Title();
            _taskDbContext.Entry(luTask).State = EntityState.Modified;
            _taskDbContext.SaveChanges();
        }

        public void SaveUserExperience(UserExperience ux)
        {
            if (!ux.IsDirty) return;

            var uxTask = _taskDbContext.UxTasks.FirstOrDefault(x => x.Id == ux.TaskId);
            if (uxTask == null) return;

            uxTask.State = ObjectToJson(ux.State);
            uxTask.Title = ux.Title();
            _taskDbContext.Entry(uxTask).State = EntityState.Modified;
            _taskDbContext.SaveChanges();
        }

        public Dictionary<string, string> NullComponentInputProperties(object obj)
        {
            var nullAttributedProperties = (
                from propertyInfo in obj.GetType().GetProperties()
                let attr = propertyInfo.GetCustomAttributes(typeof(ComponentInputAttribute), false)
                where attr.Length == 1 && propertyInfo.GetValue(obj) == null
                select new { PropertyName = propertyInfo.Name, ContextName = ((ComponentInputAttribute)attr.First()).ContextName }
            ).ToDictionary(k => k.PropertyName, v => v.ContextName);
            return nullAttributedProperties;
        }

        public Dictionary<string, string> ComponentInputProperties(object obj)
        {
            var nullAttributedProperties = (
                from propertyInfo in obj.GetType().GetProperties()
                let attr = propertyInfo.GetCustomAttributes(typeof(ComponentInputAttribute), false)
                where attr.Length == 1
                select new { PropertyName = propertyInfo.Name, ContextName = ((ComponentInputAttribute)attr.First()).ContextName }
            ).ToDictionary(k => k.PropertyName, v => v.ContextName);
            return nullAttributedProperties;
        }

        public RouteValueDictionary ParseSearchText(string searchText)
        {
            var candidateContextValues = new RouteValueDictionary();

            if (searchText == null) return candidateContextValues;

            var searchTextParts = searchText.Split(' ');
            int id;
            for (int p = 0; p < searchTextParts.Length; p++)
            {
                // Look for first/next numerical value.
                var part = searchTextParts[p];
                if (int.TryParse(part, out id))
                {
                    // Associate numerical value with each prior part of the search text.
                    for (int p2 = 0; p2 < p; p2++)
                    {
                        part = searchTextParts[p2];
                        if (!candidateContextValues.ContainsKey(part)) candidateContextValues.Add(part, id);
                    }
                }
            }
            return candidateContextValues;
        }

        public int GetUltimateParentLogicalUnitId(int uxTaskId)
        {
            var luTask = GetUltimateParentLogicalUnitTask(uxTaskId);
            return luTask.Id;
        }


    }
}