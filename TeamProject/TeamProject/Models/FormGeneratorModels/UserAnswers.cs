using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamProject.Models.FormGeneratorModels
{
    public class UserAnswers
    {
        [Key]
        public int Id { get; set; }
        public int IdForm { get; set; }
        public int IdField { get; set; }
        public int IdUser { get; set; }
        public int IdPatient { get; set; }
        public string Answer { get; set; }
        public int IdTest { get; set; }
    }
}
