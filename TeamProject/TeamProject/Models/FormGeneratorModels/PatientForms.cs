using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FormGenerator.Models
{
    public class PatientForms
    {
        [Key]
        public int Id { get; set; }
        public int IdPatient { get; set; }
        public int IdForm { get; set; }
        public Boolean? agreement { get; set; }
        public int IdTest { get; set; }
        public bool IsSendBefore { get; set; }


     

    }

    public class PatientFormsHelper
    {
        public int Id;
        public int IdPatient;
        public int IdForm;
        public string nazwa_formularza;
        public Boolean? agreement { get; set; }
        public int IdTest { get; set; }
        public bool IsSendBefore { get; set; }
    }
}
