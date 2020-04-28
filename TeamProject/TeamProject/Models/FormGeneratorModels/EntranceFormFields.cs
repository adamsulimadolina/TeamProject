using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FormGenerator.Models
{
    public class EntranceFormFields
    {
        [Key]
        public int Id { get; set; }
        public int IdField { get; set; }
    }
}