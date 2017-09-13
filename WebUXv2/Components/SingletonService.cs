using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUXv2.Components
{
    public sealed class SingletonService
    {
        private static readonly SingletonService instance = new SingletonService(new EntityContextManager());

        private SingletonService(IEntityContextManager entityContextManager)
        {
            EntityContextManager = entityContextManager;
        }
        public static SingletonService Instance { get { return instance; } }

        public IEntityContextManager EntityContextManager { get; set; }

        public string UserMessage { get; set; }
    }
}