using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamProject.Models.FormGeneratorModels
{
    public class Test
    {
        [Key]
        public int Id { get; set; }
        public int IdTest { get; set; }
        public DateTime DateOfTest { get; set; }
        public int IdPatient { get; set; }
    }
}
