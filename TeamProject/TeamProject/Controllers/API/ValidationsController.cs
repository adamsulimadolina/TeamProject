using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormGenerator.Models;
using TeamProject.Models.NewTypeAndValidation;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace TeamProject.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ValidationsController : ControllerBase
    {
        private readonly FormGeneratorContext _context;

        public ValidationsController(FormGeneratorContext context)
        {
            _context = context;
        }

        // GET: api/Validations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Validation>>> GetValidations()
        {
            return await _context.Validations.ToListAsync();
        }

        // GET: api/Validations/5
        [HttpGet("{id}/{value}")]
        public string GetValidation(int id, string value)
        {
            decimal valueDecimal;
            int valueInteger;
            string json;
            bool isNumber = Decimal.TryParse(value, out valueDecimal);
            bool isInteger = Int32.TryParse(value, out valueInteger);
            if (!isNumber || value[0].Equals(',')==true || value.Last<char>().Equals(',')==true)
            {
                json = JsonConvert.SerializeObject(new {response="Wpisana wartość nie jest liczbą" });
                return json;
            }

            var validations = _context.Validations
                .Where(v => v.idField == id)
                .ToList();
            foreach (var validate in validations)
            {
                if (validate.type == "min" && valueDecimal < validate.value)
                {
                    json = JsonConvert.SerializeObject(new { response = "Liczba musi być większa niż " + validate.value.ToString() });
                    return json;
                }
                else if (validate.type == "max" && valueDecimal > validate.value)
                {
                    json = JsonConvert.SerializeObject(new { response = "Liczba musi być mniejsza niż " + validate.value.ToString() });
                    return json;
                }
                else if (validate.type == "integerVal" && isInteger == false) 
                {
                    json = JsonConvert.SerializeObject(new { response = "Liczba musi być całkowita" });
                    return json;
                }
            }

            json = JsonConvert.SerializeObject(new { response = "true" });
            return json;


        }


        private bool ValidationExists(int id)
        {
            return _context.Validations.Any(e => e.idValidation == id);
        }
    }
}
