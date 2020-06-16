using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeamProject.Models.FormGeneratorModels
{
    public class Logs
    {
        [Key]
        public int LogID { get; set; }
        public int UserID { get; set; }
        public int FormID { get; set; }
        public int FieldID { get; set; }
        public int TestID { get; set; }
        public string AnswerValue { get; set; }
        public DateTime date { get; set; }
    }
}
