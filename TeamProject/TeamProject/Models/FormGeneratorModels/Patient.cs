using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FormGenerator.Models
{
    
        public class Patient
        {
            [Key]
            public int IdPatient { get; set; }
    }
    
}