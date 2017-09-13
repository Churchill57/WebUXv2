using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace WebUXv2.Components
{
    public class EntityContextManager : IEntityContextManager
    {
        private readonly Dictionary<string, IEntityContext> _entityContexts = new Dictionary<string, IEntityContext>();
        private readonly Dictionary<string, IEntityContextRelationship> _entityRelationships = new Dictionary<string, IEntityContextRelationship>();

        public IEntityContext GetCurrentContext { get; private set; }

        public IEntityContext SetContext(int id, string name, string description)
        {
            IEntityContext entityContext = null;

            var key = ContextKey(id, name);

            if (_entityContexts.ContainsKey(key))
            {
                entityContext = _entityContexts[key];
                if (!String.IsNullOrEmpty(description))
                {
                    ((EntityContext)entityContext).Description = description;
                }
                ((EntityContext)entityContext).WhenSet = DateTime.Now;
            }
            else
            {
                entityContext = new EntityContext(id, name.ToLower(), description);
                _entityContexts.Add(key, entityContext);
            }

            GetCurrentContext = entityContext;

            return entityContext;

        }

        public IEntityContext SetContext(IEntityContext entityContext)
        {
            if (entityContext == null) return null;
            return SetContext(entityContext.Id, entityContext.Name, entityContext.Description);
        }

        internal static string ContextKey(int id, string name)
        {
            return $"{id}|ctx|{name.ToLower()}";
        }
        public static string ContextKey(IEntityContext entityContext)
        {
            return ContextKey(entityContext.Id, entityContext.Name);
        }
        public static string DirectRelationshipKey(string name, IEntityContext entityContext)
        {
            return $"{ContextKey(entityContext)}|rln|{name.ToLower()}";
        }

        internal static string RelationshipKey(IEntityContextRelationship entityContextRelationship)
        {
            return $"{ContextKey(entityContextRelationship.EntityContext1)}|rln|{entityContextRelationship.Name}|rln|{ContextKey(entityContextRelationship.EntityContext2)}";
        }

        internal static string ReverseRelationshipKey(IEntityContextRelationship entityContextRelationship)
        {
            return $"{ContextKey(entityContextRelationship.EntityContext2)}|rln|{entityContextRelationship.Name}|rln|{ContextKey(entityContextRelationship.EntityContext1)}";
        }

        public IEntityContext GetContext(int id, string name)
        {
            return GetContext(ContextKey(id, name));
        }

        public IEntityContext GetContext(string key)
        {
            IEntityContext entityContext = null;
            try
            {
                entityContext = _entityContexts[key];
            }
            catch (KeyNotFoundException)
            {
            }
            return entityContext;
        }

        public IEnumerable<IEntityContext> GetContexts()
        {
            return _entityContexts.Values.OrderByDescending(x => x.WhenSet);
        }

        public IEntityContextDirectRelationship SetDirectRelationship(IEntityContext entityContext1, string relationshipName, IEntityContext entityContext2)
        {

            var context1 = SetContext(entityContext1.Id, entityContext1.Name, entityContext1.Description);
            var context2 = SetContext(entityContext2.Id, entityContext2.Name, entityContext2.Description);
            var relationName = relationshipName.ToLower();
            var relationship = new EntityContextRelationship() { EntityContext1 = context1, Name = relationName, EntityContext2 = context2 };

            var directRelationship1 = new EntityContextDirectRelationship( relationName, context2 );

            var relationshipKey = RelationshipKey(relationship);
            var relationshipKeyReverse = ReverseRelationshipKey(relationship);
            if (!_entityRelationships.ContainsKey(relationshipKey) && !_entityRelationships.ContainsKey(relationshipKeyReverse))
            {
                _entityRelationships.Add(relationshipKey, relationship);
            }

            return directRelationship1;
        }

        public IEnumerable<IEntityContextDirectRelationship> GetDirectRelationships(IEntityContext entityContext, string relationshipName = null, string relatedEntityName = null)
        {
            return GetDirectRelationships(entityContext.Id, entityContext.Name,relationshipName, relatedEntityName);
        }

        private IEnumerable<IEntityContextDirectRelationship> GetDirectRelationships(int id, string name, string relationshipName = null, string relatedEntityName = null)
        {
            var relationships = (
                from r in _entityRelationships.Values
                where String.Equals(r.EntityContext1.Name, name, StringComparison.CurrentCultureIgnoreCase)
                   && r.EntityContext1.Id == id
                   && (String.Equals(r.Name, relationshipName, StringComparison.CurrentCultureIgnoreCase) || String.IsNullOrEmpty(relationshipName))
                   && (String.Equals(r.EntityContext2.Name, relatedEntityName, StringComparison.CurrentCultureIgnoreCase) || String.IsNullOrEmpty(relatedEntityName))
                select new EntityContextDirectRelationship(r.Name, r.EntityContext2)
            ).Union(
                from r in _entityRelationships.Values
                where String.Equals(r.EntityContext2.Name, name, StringComparison.CurrentCultureIgnoreCase)
                   && r.EntityContext2.Id == id
                   && (String.Equals(r.Name, relationshipName, StringComparison.CurrentCultureIgnoreCase) || String.IsNullOrEmpty(relationshipName))
                   && (String.Equals(r.EntityContext1.Name, relatedEntityName, StringComparison.CurrentCultureIgnoreCase) || String.IsNullOrEmpty(relatedEntityName))
                select new EntityContextDirectRelationship(r.Name, r.EntityContext1)
            );
            return relationships;
        }

        public void Clear()
        {
            _entityContexts.Clear();
            GetCurrentContext = null;
        }

        public IEntityContext ResolveContext(string requiredContextName, string preferredRelationshipName = null)
        {
            if (GetCurrentContext != null)
            {
                // Is the required context name the same as the current entity context name.
                if (String.Equals(GetCurrentContext.Name, requiredContextName, StringComparison.CurrentCultureIgnoreCase)) return GetCurrentContext;
            }

            // Is the required context name directly related to the current entity context.
            if (GetCurrentContext != null)
            {
                var relatedContext =
                    GetDirectRelationships(GetCurrentContext, null, requiredContextName)
                    .OrderByDescending(dr => String.Equals(dr.Name, preferredRelationshipName, StringComparison.CurrentCultureIgnoreCase) ? 1 : 0)
                    .ThenByDescending(dr => dr.EntityContext.WhenSet)
                    .FirstOrDefault();
                if (relatedContext != null) return relatedContext.EntityContext;
            }

            // Is the required context name indirectly related to the current entity context.
            if (GetCurrentContext != null)
            {
                var relatedContext = (
                    from EntityContextDirectRelationship dr in GetRelatedContexts(GetCurrentContext, 5)
                    orderby dr.Level
                           ,String.Equals(dr.Name, preferredRelationshipName, StringComparison.CurrentCultureIgnoreCase) ? 1 : 0 descending 
                           ,dr.EntityContext.WhenSet descending
                    where String.Equals(dr.EntityContext.Name, requiredContextName, StringComparison.CurrentCultureIgnoreCase)
                    select dr.EntityContext
                ).FirstOrDefault();
                if (relatedContext != null) return relatedContext;
            }
            return null;
        }

        public void RemoveDirectRelationship(IEntityContext entityContext, string relationshipName)
        {
            if (entityContext == null) return;
            RemoveDirectRelationship(entityContext.Id, entityContext.Name, relationshipName);
        }

        public void RemoveDirectRelationship(int id, string name, string relationshipName)
        {
            var relationshipKeys = new List<string>();
            foreach (var entityRelationship in _entityRelationships)
            {
                if (entityRelationship.Value.EntityContext1.Id == id &&
                    string.Equals(entityRelationship.Value.EntityContext1.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(entityRelationship.Value.Name, relationshipName, StringComparison.CurrentCultureIgnoreCase))
                {
                    relationshipKeys.Add(entityRelationship.Key);
                }
                if (entityRelationship.Value.EntityContext2.Id == id &&
                    string.Equals(entityRelationship.Value.EntityContext2.Name, name, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(entityRelationship.Value.Name, relationshipName, StringComparison.CurrentCultureIgnoreCase))
                {
                    relationshipKeys.Add(entityRelationship.Key);
                }
            }

            foreach (var relationshipKey in relationshipKeys)
            {
                _entityRelationships.Remove(relationshipKey);
            }

        }

        private IEnumerable<IEntityContextDirectRelationship> GetRelatedContexts(IEntityContext entityContext, int levelsBeyondDirect = 1)
        {
            var relatedContexts = new Dictionary<string, EntityContextDirectRelationship>();
            AppendDirectlyRelatedContexts(relatedContexts, entityContext, 0, levelsBeyondDirect);
            return relatedContexts.Values.ToList();
        }

        private void AppendDirectlyRelatedContexts(Dictionary<string, EntityContextDirectRelationship> relatedContexts, IEntityContext entityContext, int level, int levelsBeyondDirect)
        {
            var directRelationships = GetDirectRelationships(entityContext);
            foreach (var dr in directRelationships)
            {
                var contextKey = ContextKey(dr.EntityContext);
                if (!relatedContexts.ContainsKey(contextKey))
                {
                    relatedContexts.Add(contextKey, (EntityContextDirectRelationship)dr);
                    if (level <= levelsBeyondDirect) AppendDirectlyRelatedContexts(relatedContexts, dr.EntityContext, level + 1, levelsBeyondDirect);
                }
            }
        }

    }

    public interface IEntityContextManager
    {
        IEntityContext GetCurrentContext { get; }
        IEntityContext SetContext(int id, string name, string description);
        IEntityContext SetContext(IEntityContext entityContext);
        IEntityContext GetContext(int id, string name);
        IEntityContext GetContext(string key);
        IEnumerable<IEntityContext> GetContexts();
        IEntityContextDirectRelationship SetDirectRelationship(IEntityContext entityContext1, string relationshipName, IEntityContext entityContext2);
        IEnumerable<IEntityContextDirectRelationship> GetDirectRelationships(IEntityContext entityContext, string relationshipName = null, string relatedEntityName = null);
        void Clear();
        IEntityContext ResolveContext(string requiredContextName, string preferredRelationshipName = null);

        void RemoveDirectRelationship(IEntityContext entityContext, string relationshipName);
        void RemoveDirectRelationship(int id, string name, string relationshipName);
    }

    public class EntityContext : IEntityContext
    {
        public EntityContext()
        {
            // for de-serialization
        }
        public EntityContext(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
            WhenSet = DateTime.Now;
        }
        public override string ToString()
        {
            return $"{Id}|{Name}|{Description}";
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCurrent => (SingletonService.Instance.EntityContextManager.GetCurrentContext == this);
        public DateTime WhenSet { get; internal set; }
    }



    public interface IEntityContext
    {
        int Id { get; }
        string Name { get; }
        string Description { get; }
        bool IsCurrent { get; }
        DateTime WhenSet { get; }
    }



    public class EntityContextDirectRelationship : IEntityContextDirectRelationship
    {
        public string Name { get; set; }
        public IEntityContext EntityContext { get; set; }
        internal int Level { get; set; }
        public EntityContextDirectRelationship(string name, IEntityContext entityContext)
        {
            Name = name;
            this.EntityContext = entityContext;
        }
        public override string ToString()
        {
            return $"{Name}|rln|{EntityContextManager.ContextKey(EntityContext)}";
        }


    }

    public interface IEntityContextDirectRelationship
    {
        string Name { get; set; }
        IEntityContext EntityContext { get; set; }
    }

    internal interface IEntityContextRelationship
    {
        IEntityContext EntityContext1 { get; set; }
        string Name { get; set; }
        IEntityContext EntityContext2 { get; set; }
    }

    internal class EntityContextRelationship : IEntityContextRelationship
    {
        public IEntityContext EntityContext1 { get; set; }
        public string Name { get; set; }
        public IEntityContext EntityContext2 { get; set; }
    }

}