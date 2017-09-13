using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;

namespace WebUXv2.Components
{
    public abstract class UserExperience : Component
    {

        public override void Save()
        {
            TaskMan.SaveUserExperience(this);
        }
    }
}