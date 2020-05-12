using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamProject.Models.Modele_pomocnicze
{
    public class ListOfFields
    {
        public int IdForm { get; set; }
        public int NumberOfRecords { get; set; }
        public bool IsAllRecords { get; set; }
        public List<FieldForExcel> Fields { get; set; }
    }
}
