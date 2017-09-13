using System;
using System.Collections.Generic;
using WebUXv2.Models;

namespace WebUXv2.UserExperiences.Interfaces
{
    public interface IUxPerformSearch<T>
    {
        IEnumerable<T> PerformSearch();
    }
}
