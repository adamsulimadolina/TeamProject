
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamProject.Models.FieldDependencyModels;
using TeamProject.Models.NewTypeAndValidation;

namespace FormGenerator.Models.Modele_pomocnicze
{
    public class FieldWithValue
    {
        public Field Field { get; set; } = new Field();
        public string TextValue { get; set; }
        public bool BoolValue { get; set; }
        public FieldFieldDep fieldFieldDep { get; set; } = new FieldFieldDep();
        public List<FieldFieldDependency> Dependencies { get; set; } = new List<FieldFieldDependency>();
        public List<StringBoolType> DepndenciesValue { get; set; } = new List<StringBoolType>();
        public List<SelectFieldOptions> options { get; set; } = new List<SelectFieldOptions>();

    }

    public class StringBoolType
    {
        public string textVal { get; set; }
        public Boolean boolVal { get; set; }

    }
}