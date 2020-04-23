using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamProject.Models.FormGeneratorModels
{
    public class GUIDFileNameMap
    {
        [Key]
        public string Guid { get; set; }
        public string FileName { get; set; }
    }
}
