using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TeamProject.Models.FormGeneratorModels;
using Microsoft.AspNetCore.Authorization;

namespace TeamProject.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {

        private readonly FormGeneratorContext _context;

        public PatientController(FormGeneratorContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ObjectResult> PatientForms(int id)
        {
           var patientForms = await _context.PatientForms.Where(m => m.IdPatient == id && m.agreement==true).ToListAsync();
            var forms = await _context.Forms.ToListAsync();

            List<PatientFormsHelper> list = new List<PatientFormsHelper>();

            foreach (PatientForms x in patientForms)
            {
                PatientFormsHelper pom = new PatientFormsHelper
                {
                    Id = x.Id,
                    IdForm = x.IdForm,
                    IdPatient = x.IdPatient,
                    nazwa_formularza = forms.FirstOrDefault(n => n.Id == x.IdForm).Name,
                    agreement = x.agreement
                };
                list.Add(pom);
            }
           

            return Ok(list);
        }

        [HttpGet]
        public async Task<ActionResult> AllPatientForms(int id)
        {
            var patientForms = await _context.PatientForms.Where(m => m.IdPatient == id ).ToListAsync();
            var forms = await _context.Forms.ToListAsync();

            List<PatientFormsHelper> list = new List<PatientFormsHelper>();

            foreach (PatientForms x in patientForms)
            {
                PatientFormsHelper pom = new PatientFormsHelper
                {
                    Id = x.Id,
                    IdForm = x.IdForm,
                    IdPatient = x.IdPatient,
                    nazwa_formularza = forms.FirstOrDefault(n => n.Id == x.IdForm).Name,
                    agreement = x.agreement
                };
                list.Add(pom);
            }


            return RedirectToAction("EntranceForm", "EntranceFormFields", new { id = id });
        }


        [HttpGet]
        public IActionResult Create()
        {
            HttpContext.Session.SetString("lista", "ch");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPatient")] Patient patient)
        {
            var entranceConnections = await _context.EntranceConnections.ToListAsync();
            int y = patient.IdPatient; 
            if (ModelState.IsValid)
            {
                _context.Patients.Add(patient);

                var formsId =  _context.Forms;

                foreach(var x in formsId)
                {
                    var response = entranceConnections.FirstOrDefault(m => m.IdForm == x.Id);
                    if (response != null)
                    {
                        PatientForms patientForms = new PatientForms();
                        patientForms.IdPatient = patient.IdPatient;
                        patientForms.IdForm = x.Id;
                        _context.PatientForms.Add(patientForms);
                    }
                    else
                    {
                        PatientForms patientForms = new PatientForms();
                        patientForms.IdPatient = patient.IdPatient;
                        patientForms.IdForm = x.Id;
                        patientForms.agreement = true;
                        _context.PatientForms.Add(patientForms);
                    }
                }
                _context.SaveChanges();

            }
            return View();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.IdPatient == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }
        [HttpPost]
        public async Task<ActionResult> Index(string Id)
        {
                var IdPatient = _context.Patients;

                var pom =  IdPatient.FirstOrDefault(m => m.IdPatient == Convert.ToInt32(Id));

                if(pom != null)
                {
                    TempData["Message"] = "znalazlo";
                    return RedirectToAction("EntranceForm","EntranceFormFields", new {id = Id});
                }
                else
                {
                    
                    TempData["Message"]="Id pacjetna nie znajduje się w bazie";
                }
                //Po co przekazywanie listy pacjentów do widoku??
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<ActionResult> Index()
        {

            var patient = from m in _context.Patients select m;
            return View(await _context.Patients.ToListAsync());
        }

    }
}