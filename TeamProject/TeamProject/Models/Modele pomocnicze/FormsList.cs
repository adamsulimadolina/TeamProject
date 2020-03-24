using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TeamProject.Models.FieldDependencyModels;
using TeamProject.Models.NewTypeAndValidation;

namespace FormGenerator.Models.Modele_pomocnicze
{
    public class FormsList
    {
            
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public String Category { get; set; }

        public string Name { get; set; }

    }

}

