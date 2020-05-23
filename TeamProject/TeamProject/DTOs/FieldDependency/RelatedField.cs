using FormGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamProject.DTOs.FieldDependency
{
    public class RelatedField
    {
        Field field { get; set; }
        public List<String> fieldAnswers { get; set; }

       
    }
}
