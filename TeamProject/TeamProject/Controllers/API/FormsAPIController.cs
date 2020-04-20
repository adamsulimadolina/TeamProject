using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormGenerator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamProject.Models.FormGeneratorModels;

namespace TeamProject.Controllers.API
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FormsAPIController : ControllerBase
    {
        private readonly FormGeneratorContext _context;
        public FormsAPIController(FormGeneratorContext context)
        {
            _context = context;
        }
        // GET: api/FormsAPI
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/FormsAPI/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/FormsAPI
        [HttpPost] 
        public async void SaveTranslation([FromBody]TableNameTranslation  translation)
        {
            _context.TableNameTranslations.Add(translation);
            await _context.SaveChangesAsync();
        }

        // PUT: api/FormsAPI/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
