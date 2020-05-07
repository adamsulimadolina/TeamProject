using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using FormGenerator.Models;
using TeamProject.Models.FormGeneratorModels;

namespace FormGenerator.Models.Modele_pomocnicze
{
    public class UserAnswerList
    { 
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Id_User { get; set; }

        public List<UserAnswers> user_answer_list { get; set; } = new List<UserAnswers>();

    }

    


}
