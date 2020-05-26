using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FormGenerator.Models;
using FormGenerator.Models.Modele_pomocnicze;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace TeamProject.Controllers
{
    [Authorize]
    public class EntranceFormFieldsController : Controller
    {
        private readonly FormGeneratorContext _context;

        public EntranceFormFieldsController(FormGeneratorContext context)
        {
            _context = context;
        }

        // GET: EntranceFormFields
        public async Task<IActionResult> Index()
        {
            var entrance_form = _context.EntranceFormFields
                .Select(t => t.IdField)
                .ToList();
            var entrance_q = _context.Field
                .Where(t => entrance_form.Contains(t.Id))
                .ToList();
            
            return View(entrance_q);
        }

        // GET: EntranceFormFields/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entranceFormFields = await _context.EntranceFormFields
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entranceFormFields == null)
            {
                return NotFound();
            }

            return View(entranceFormFields);
        }

        // GET: EntranceFormFields/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EntranceFormFields/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Type")] Field @field)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@field);
                await _context.SaveChangesAsync();
                Field current = _context.Field.Where(t => t == (@field)).ToList()[0];
                var entrance_field = new EntranceFormFields();
                entrance_field.IdField = current.Id;
                _context.Add(entrance_field);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@field);
        }

        // GET: EntranceFormFields/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entranceFormFields = await _context.EntranceFormFields.FindAsync(id);
            if (entranceFormFields == null)
            {
                return NotFound();
            }
            return View(entranceFormFields);
        }

        // POST: EntranceFormFields/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdField")] EntranceFormFields entranceFormFields)
        {
            if (id != entranceFormFields.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entranceFormFields);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntranceFormFieldsExists(entranceFormFields.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(entranceFormFields);
        }

        // GET: EntranceFormFields/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entranceFormFields = await _context.EntranceFormFields
                .FirstOrDefaultAsync(m => m.IdField == id);
            if (entranceFormFields == null)
            {
                return NotFound();
            }

             return View(entranceFormFields);
           // return RedirectToAction("Delete", new { id = entranceFormFields.Id });
            
        }

        // POST: EntranceFormFields/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entranceFormFields = await _context.EntranceFormFields
                .Where(t => t.IdField == id)
                .FirstOrDefaultAsync();
            var entranceConnections = await _context.EntranceConnections
                .Where(t => t.IdField == id)
                .ToListAsync();

            foreach(var el in entranceConnections)
            {
                _context.Remove(el);
            }

            _context.EntranceFormFields.Remove(entranceFormFields);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntranceFormFieldsExists(int id)
        {
            return _context.EntranceFormFields.Any(e => e.Id == id);
        }


        public async Task<IActionResult> EntranceForm(int id)
        {
            var current_test = HttpContext.Session.GetInt32("current_test");

            var entrance_form = _context.EntranceFormFields
                .Select(t => t.IdField)
                .ToList();
            var entrance_q = _context.Field
                .Where(t => entrance_form.Contains(t.Id))
                .ToList();
            List<EntranceFormAnswers> list = new List<EntranceFormAnswers>();
            EntranceFormAnswers ans; new EntranceFormAnswers();
            foreach(var elem in entrance_q)
            {
                ans = new EntranceFormAnswers();
                ans.IdField = elem.Id;
                ans.Name = elem.Name;
                var patient_fields = await _context.EntranceConnections
                    .Where(t => t.IdField == ans.IdField)
                    .ToListAsync();
                
                foreach(var el in patient_fields)
                {
                    if (el.IdField == ans.IdField)
                    {
                        var patient_agreement = await _context.PatientForms
                            .Where(t => t.IdForm == el.IdForm)
                            .Where(t => t.IdPatient == id)
                            .Where(t => t.IdTest == current_test)
                            .FirstOrDefaultAsync();
                        if (patient_agreement != null)
                        {
                            if (patient_agreement.agreement == true)
                            {
                                ans.Answer = true;
                            }
                            else patient_agreement.agreement = false;
                        }
                        break;
                    }
                }
                
                list.Add(ans);
            }
            ViewBag.bag = id;
            return View(list);
        }

        [HttpPost]
        public async Task<ActionResult> EntranceForm(List<EntranceFormAnswers> tuple_list)
        {
            int? current_test = HttpContext.Session.GetInt32("current_test");
            List<EntranceFormAnswers> tuple = tuple_list;
            var idFields = tuple.Select(t => t.IdField).ToList();
            var Connections = await _context.EntranceConnections.Where(m=>idFields.Contains(m.IdField)).ToListAsync();
            var patientForms = await _context.PatientForms.Where(p => p.IdPatient == tuple[0].IdPatient && p.IdTest == current_test).ToListAsync();

            foreach(var connect in Connections)
            {
                var form = patientForms.Where(p => p.IdForm == connect.IdForm).ToList()[0];
                var answer = tuple.Where(t => t.IdField == connect.IdField).ToList()[0];
                form.agreement = answer.Answer;
                _context.PatientForms.Update(form);

            } 
            
            _context.SaveChanges();


            return RedirectToAction("AllPatientForms", "Patient", new { id =tuple.FirstOrDefault().IdPatient });
        }

        [HttpGet]
        public IActionResult AddConnection (int id)
        {
            ViewBag.bag = id;
            List<Forms> listallforms = _context.Forms.ToList();
            var listallfield = _context.EntranceConnections.ToList();
            int pom = 0;
            List<Forms> listEmptyForms = new List<Forms>();
            for(int i = 0; i < listallforms.Count; i++)
            {
                pom = 0;
                for(int j = 0; j < listallfield.Count; j++)
                {
                    if (listallforms[i].Id == listallfield[j].IdForm)
                    {
                        pom = 1;
                    }
                }
                if (pom == 0)
                {
                    listEmptyForms.Add(listallforms[i]);
                }
            }
            ViewBag.listforms = listEmptyForms;

            List<EntranceConnectionShow> entranceConnections = new List<EntranceConnectionShow>();
            var ConnectionList = _context.EntranceConnections.Where(m => m.IdField == id).ToList();
            var Field = _context.Field.FirstOrDefault(m => m.Id.Equals(id));
            var Forms = _context.Forms.ToList();

            foreach(EntranceConnections connection in ConnectionList)
            {
                EntranceConnectionShow foo = new EntranceConnectionShow
                {
                    Field = Field.Name,
                    Id = connection.Id,
                    Form = Forms.FirstOrDefault(m => m.Id.Equals(connection.IdForm)).Name,
                    IdForm = connection.IdForm,
                    IdField = Field.Id
                };
                entranceConnections.Add(foo);
            }
           



            return View(entranceConnections);
        }

        [HttpPost]
        public async Task<IActionResult> AddConnection([Bind("Id,IdField,IdForm")] EntranceConnections @entranceConnections)
        {
            
            if (ModelState.IsValid)
            {
                EntranceConnections tmp = new EntranceConnections
                {
                    IdField = entranceConnections.IdField,
                    IdForm = entranceConnections.IdForm
                };
                _context.EntranceConnections.Add(tmp);
                await _context.SaveChangesAsync();
                //  EntranceConnections current = _context.EntranceConnections.FirstOrDefault(t => t == (@entranceConnections));
                List<Forms> listallforms = _context.Forms.ToList();
                var listallfield = _context.EntranceConnections.ToList();
                int pom = 0;
                List<Forms> listEmptyForms = new List<Forms>();
                for (int i = 0; i < listallforms.Count; i++)
                {
                    pom = 0;
                    for (int j = 0; j < listallfield.Count; j++)
                    {
                        if (listallforms[i].Id == listallfield[j].IdForm)
                        {
                            pom = 1;
                        }
                    }
                    if (pom == 0)
                    {
                        listEmptyForms.Add(listallforms[i]);
                    }
                }
                ViewBag.listforms = listEmptyForms;
                List<EntranceConnectionShow> entranceConnectio = new List<EntranceConnectionShow>();
                var ConnectionList = _context.EntranceConnections.Where(m => m.IdField == tmp.IdField).ToList();
                var Field = _context.Field.FirstOrDefault(m => m.Id.Equals(tmp.IdField));
                var Forms = _context.Forms.ToList();

                foreach (EntranceConnections connection in ConnectionList)
                {
                    EntranceConnectionShow foo = new EntranceConnectionShow
                    {
                        Field = Field.Name,
                        Id = connection.Id,
                        Form = Forms.FirstOrDefault(m => m.Id.Equals(connection.IdForm)).Name,
                        IdForm = connection.IdForm,
                        IdField = Field.Id
                    };
                    entranceConnectio.Add(foo);
                }
                return View(entranceConnectio);
            }
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> DeleteDependence(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entranceFormFields = await _context.EntranceConnections
                .FirstOrDefaultAsync(m => m.IdForm == id);
            ViewBag.form = id;
            ViewBag.field = entranceFormFields.IdField;
            if (entranceFormFields == null)
            {
                return NotFound();
            }

            return View(entranceFormFields);
        }

        // POST: EntranceFormFields/Delete/5
        [HttpPost, ActionName("DeleteDependence")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDependence(int id)
        {
            var entranceFormFields = await _context.EntranceConnections
                .Where(t => t.IdForm == id)
                .FirstOrDefaultAsync();
           
            _context.EntranceConnections.Remove(entranceFormFields);
            await _context.SaveChangesAsync();
            return RedirectToAction("AddConnection", new { id = entranceFormFields.IdField });
        }
    }
}
