using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using WebUXv2.Models;

namespace WebUXv2.Components
{
    public interface ITaskManager
    {
        TaskDbContext TaskDbContext { get; }
        object CreateInstance<T>() where T : new();
        object CreateInstanceOfTypeName(string typeName);
        LogicalUnit GetLogicalUnit(LuTask luTask);
        LogicalUnit GetLogicalUnit(int id);
        LogicalUnit GetLogicalUnit(LogicalUnit parentLu, string luTypeName, string userName, string clientRef);
        T GetLogicalUnit<T>(LogicalUnit parentLu, string clientRef, Action<T> initializer = null) where T : LogicalUnit, new();
        T GetSingletonComponent<T>() where T : Component, ISingletonComponent;
        LuTask GetLuTask(int luTaskId);
        Type GetType(string typename);
        UserExperience GetUserExperience(UxTask uxTask);
         //UserExperience GetRefreshedUserExperience(int id);
        UserExperience GetUserExperience(int id);
        Task<UserExperience> GetUserExperienceAsync(int id);

        UserExperience GetUserExperience(LogicalUnit parentLu, string uxTypeName, string clientRef);
        T GetRefreshedUserExperience<T>(LogicalUnit parentLu, string clientRef) where T : UserExperience, new();
        T GetUserExperience<T>(LogicalUnit parentLu, string clientRef, Action<T> initializer = null) where T : UserExperience, new();
        UxTask GetUxTask(int uxTaskId);
        T JsonToObject<T>(string jsonData);
        string ObjectToJson(object obj);

        string GetContextDescription(object obj);

        //string GetComponentTitle(Component component);
        //string GetComponentTitle(object obj);
        //string GetComponentTitle<T>();
        string GetComponentTitle(Type type);
        string GetComponentTitle(string typename);
        IEnumerable<string> GetUserExperienceTaskBreadcrumbs(int uxTaskId);

        LuTask GetUltimateParentLauncherLogicalUnitTask(int uxTaskId);

        int GetParentLauncherRootComponentTaskId(int uxTaskId);
        int GetUltimateParentLogicalUnitId(int uxTaskId);
        LuTask GetUltimateParentLogicalUnitTask(int uxTaskId);
        string GetUltimateParentLogicalUnitTitle(int uxTaskId);
        //string GetExecutingUserExperienceTitle(int uxTaskId);

        bool UserHasAccessToComponent(IPrincipal user, Component component);

        RouteValueDictionary GetComponentInputParams(object obj);

        bool ComponentIsSingleton(object obj);
        bool ComponentIsSingleton<T>() where T : Component;
        bool ComponentIsSingleton(Type type);
        bool ComponentIsSingleton(string typename);

        void SaveLogicalUnit(LogicalUnit lu);
        void SaveUserExperience(UserExperience ux);

        Dictionary<string, string> ComponentInputProperties(object obj);

        Dictionary<string, string> NullComponentInputProperties(object obj);

        RouteValueDictionary ParseSearchText(string searchText);

    }
}