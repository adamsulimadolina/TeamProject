
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamProject.Infrastructure.Enums;
using TeamProject.Models.NewTypeAndValidation;

namespace FormGenerator.Models.Modele_pomocnicze
{
    public class FieldFieldDep
    {
        public FieldFieldDependencyType DependencyType { get; set; } //typ zależności
        public string ActivationValue { get; set; } 
        public List<Field> dependentFields { get; set; } = new List<Field>();
       

    }
}