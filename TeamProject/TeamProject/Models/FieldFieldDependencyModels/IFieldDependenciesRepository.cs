using FormGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamProject.Models.FieldDependencyModels;

namespace TeamProject.Models.FieldFieldDependencyModels
{
    public interface IFieldDependenciesRepository
    {
        IQueryable<FieldFieldDependency> Dependencies { get; }
        void SaveDependency(FieldFieldDependency dependency);
        IEnumerable<Field> GetAllDependFields();
    }
}
