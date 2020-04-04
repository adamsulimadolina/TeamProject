using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamProject.Models.FormGeneratorModels
{
    public class TableNameTranslation
    {
        [Key]
        public int Id { get; set; }
        public string DatabaseName { get; set; }
        public string DisplayedName { get; set; }

        
    }
}
