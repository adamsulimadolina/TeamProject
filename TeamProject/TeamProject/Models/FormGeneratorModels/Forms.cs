using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FormGenerator.Models
{
    public class Forms
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int id_Category { get; set; }
       
        public string Name { get; set; }
        
        public string Validate(FormGeneratorContext context)
        {
            if (context.UserAnswers.AsNoTracking().FirstOrDefault(ans => ans.IdForm == this.Id) != null)
            {
                return "Nie można edytować formularza, gdyż został już użyty w ankietowaniu!";
            }

            return null;
        }
    }

    public class formsModel
    {
       public  Forms form;
        public IEnumerable<Forms> Dzieci;
    }
}
