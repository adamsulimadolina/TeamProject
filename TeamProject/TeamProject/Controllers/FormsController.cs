using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FormGenerator.Models;
using FormGenerator.Models.Modele_pomocnicze;
using TeamProject.Models.FormGeneratorModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using TeamProject.Models;
using TeamProject.Models.NewTypeAndValidation;
using TeamProject.Models.FieldFieldDependencyModels;
using TeamProject.Models.FieldDependencyModels;

namespace FormGenerator.Controllers
{
    [Authorize()]
    public class FormsController : Controller
    {
        private readonly FormGeneratorContext _context;
        private readonly UserManager<MyUser> _userManager;
        private readonly IFieldDependenciesRepository pomik;

        public FormsController(FormGeneratorContext context, UserManager<MyUser> userManager, IFieldDependenciesRepository repository)
        {
            _context = context;
            _userManager = userManager;
            pomik = repository;

        }

        //wyświetlenie listy formularzy
        public async Task<IActionResult> ListaFormularzy()
        {
            return View(await _context.Forms.ToListAsync());
        }

        // GET: Forms/Details/5
        public IActionResult Formularz(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //wyszukanie oraz przekonwertowanie do listy Id pól które są dołączone do formularza
            //bierzemy pod uwagę tylko id pól należących do formularza
            var fieldsInForm = _context.FormField.Where(ff=>ff.IdForm==id).Select(ff=>ff.IdField).ToList();
            //pobieramy dane pól których id pobrano powyżej
            var field = _context.Field.Where(f => fieldsInForm.Contains(f.Id)).ToList();

            var xd = pomik.Dependencies.Where(d => field.Contains(d.ThisField)).Select(d=>d.RelatedFields).ToList();

            //var fieldDep = pomik.GetAllDependFields();
            //przekształcenie do modelu pozwalającego przenoszenie wartości
            //czyli dodanie miejsca na wartość "Value" której nie potrzebójemy w bazie danyc
            //potrzebne jest jedynie w celu przeniesienia wartości wpisanych w otworzonym formularzu
            //do metody POST
            List<FieldWithValue> fieldWithValues = new List<FieldWithValue>();

            List<Field> nadrzedne = new List<Field>();
            var lista = pomik.Dependencies.Where(d => fieldsInForm.Contains(d.Id)).ToList();

            foreach (Field f in field)
            {
                Boolean znaleziono = false;
                foreach(FieldFieldDependency dep in lista)
                {
                    foreach(Field depField in dep.RelatedFields)
                    {
                        if (f.Equals(depField))
                        {
                            znaleziono = true;
                           break;
                        }
                    }
                    if (znaleziono == true)
                        break;
                }
                
                if(znaleziono==false)
                    nadrzedne.Add(f);
            }
            

            //przepisywanie danych do odpowiedniego modelu. ma ktoś pomysł jak bardziej optymalnie przenosić???
            foreach(var key in nadrzedne)
            {
                FieldWithValue pom = new FieldWithValue();
                pom.TextValue = "";
                pom.Field.Id = key.Id;
                pom.Field.Name = key.Name;
                pom.Field.Type = key.Type;
                var dependencie = lista.FirstOrDefault(l => l.ThisField.Equals(key));
                if (dependencie != null) {
                    pom.Dependencies = dependencie;
                    for (int x = 0; x < dependencie.RelatedFields.Count; x++)
                        pom.DepndenciesValue.Add(new StringBoolType());

                }

                
                fieldWithValues.Add(pom);
            }
            ViewBag.modelcount = fieldWithValues.Count;
            if (field == null)
            {
                return NotFound();
            }
            ViewBag.formid = Convert.ToInt32(id);
            try
            {
                fieldWithValues[0].options = _context.SelectFieldOptions.ToList();
            }
            catch (Exception e) { }
            //List<Validation> validationToFields = _context.Validations
            //    .Where(v => fieldsInForm.Contains(v.idField)).ToList();
            return View(fieldWithValues);
        }

        // w tej metodzie w przyszłości nastąpi wysłanie wpisanych formularzy do bazy danych
        [HttpPost]

        public async Task<IActionResult> Formularz(List<FieldWithValue> fields, int formId, int patientId)
        {
            MyUser user = await GetUser();
            foreach (var field in fields)
            {
                UserAnswers answer = new UserAnswers
                {
                    IdForm = formId,
                    IdField = field.Field.Id,
                    IdPatient = patientId,
                    IdUser = user.CustomID
                };

                switch (field.Field.Type)
                {
                    case "checkbox":
                        answer.Answer = field.BoolValue.ToString();
                        break;
                    case "text":
                        answer.Answer = field.TextValue;
                        break;
                    case "number":
                        answer.Answer = field.TextValue;
                        break;
                    case "select":
                        answer.Answer = field.TextValue;
                        break;
                }
                _context.Add(answer);

                for(int i = 0; i < field.Dependencies.RelatedFields.Count; i++)
                {
                    UserAnswers pomik = new UserAnswers
                    {
                        Answer = field.Dependencies.RelatedFields[i].Type == "checkbox" ? field.DepndenciesValue[i].boolVal.ToString() : field.DepndenciesValue[i].textVal,
                        IdField = field.Dependencies.RelatedFields[i].Id,
                        IdForm=formId,
                        IdPatient = patientId,
                        IdUser = user.CustomID
                    };
                    _context.Add(pomik);
                }
            }
            _context.SaveChanges();
            return View("WyslanoFormularz", fields);
        }

        // stworzenie formularza
        public IActionResult Create(int ?id)
        {
            if(id==null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.bag = id;
            return View();
        }
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,id_Category,Parent")] Forms forms)
        {
            if (ModelState.IsValid)
            {               
                _context.Add(forms);
                await _context.SaveChangesAsync();
                List<Patient> patients = await _context.Patients.ToListAsync();
                foreach (var patient in patients)
                {
                    _context.PatientForms.Add(new PatientForms()
                    {
                        IdPatient = patient.IdPatient,
                        IdForm = forms.Id,
                        agreement = false
                    });
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Forms");
            }
            return View(forms);
        }

        // GET: Forms/Edit/5
        public IActionResult EdycjaFormularza(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            IdFieldBool idFieldBool;
            //pobranie do listy wszystkich pól(id i nazwę)
            var allFields = _context.Field.Select(af => new { af.Id, af.Name }).ToList();
            //pobranie do listy wszystkich przypisań pól które są przypisane do formularza
            var fieldsInForm = _context.FormField.Where(ff => ff.IdForm == id).Select(ff => ff.IdField).ToList();
            //tworzenie instancji modelu potrzebnego do wyświetlenia danych w widoku raz pobrania niezbędnych
            //informacji
            FormContainsField formContainsField = new FormContainsField
            {
                IdForm = id
            };
            foreach (var field in allFields)
            {
                idFieldBool = new IdFieldBool
                {
                    IdField = field.Id,
                    NameField = field.Name
                };
                if (fieldsInForm.Contains(field.Id))
                {
                    idFieldBool.ContainsField = true;
                    formContainsField.fields.Add(idFieldBool);
                }
                else
                {
                    idFieldBool.ContainsField = false;
                    formContainsField.fields.Add(idFieldBool);
                }
            }

            
           
            return View(formContainsField);
        }

        // POST: Forms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EdycjaFormularza(int id, [Bind("IdForm, fields")] FormContainsField formContainsField)
        {
            

            if (ModelState.IsValid)
            {
                //pobranie wszystkich powiązań pól z edytowanym formularzem
                var FormField = _context.FormField.Where(ff => ff.IdForm == formContainsField.IdForm).Select(ff => ff.IdField).ToList();
                FormField formField;
                foreach(var key in formContainsField.fields)
                {
                        if(key.ContainsField==true && !FormField.Contains(key.IdField)) //jeśli chcemy dodać pole
                        {                                                               //którego jeszcze nie przypisano
                        formField = new FormField
                        {
                            IdField = key.IdField,
                            IdForm = (int)formContainsField.IdForm
                        };
                            _context.Update(formField);
                            await _context.SaveChangesAsync();
                        }
                        // jeśli chcemy usunąć pole które już było przypisane
                        else if(key.ContainsField==false && FormField.Contains(key.IdField))
                        {
                        var IdDoUsuniecia = _context.FormField.Where(ff => ff.IdField == key.IdField && ff.IdForm == formContainsField.IdForm).Select(ff => ff.Id).ToList();
                        var doUsuniecia = await _context.FormField.FindAsync(IdDoUsuniecia[0]);
                        _context.FormField.Remove(doUsuniecia);
                        await _context.SaveChangesAsync();
                        }
                }
                return RedirectToAction(nameof(ListaFormularzy));
            }
            return View(formContainsField);
        }

        // GET: Forms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forms = await _context.Forms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (forms == null)
            {
                return NotFound();
            }

            return View(forms);
        }

        // POST: Forms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var forms = await _context.Forms.FindAsync(id);
            _context.Forms.Remove(forms);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListaFormularzy));
        }

        private bool FormsExists(int id)
        {
            return _context.Forms.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Index(int ?id)
        {
            ViewBag.bag = id;
            TempData.Keep("lista");
            return View(await _context.Forms.ToListAsync()) ;
        }
        public JsonResult GetForms(string order)
        {
            int id = Convert.ToInt32(order);
            var result = _context.Forms.Where(m => m.id_Category == id).ToList();
            return Json(result);
        }

        public async Task<MyUser> GetUser()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }
        //Daniel - api do logów
        [HttpPost]
        public async Task<JsonResult> AddLog()
        {

            string type = Request.Form["type"];
            string fieldvalue = Request.Form["value"];
            int formid = Convert.ToInt32(Request.Form["formid"]);
            int fieldid = Convert.ToInt32(Request.Form["fieldid"]);
            var user = await GetUser();
            Logs log = new Logs()
            {
                FieldID = fieldid,
                FormID = formid,
                date = DateTime.Now,
                UserID = user.CustomID,
                AnswerValue = fieldvalue
            };
            var oldlog = await _context.Logs.Where(l => l.FieldID == fieldid && l.FormID == formid && l.AnswerValue == fieldvalue).LastOrDefaultAsync();
            if (oldlog != null)
            {
                TimeSpan timeSpan = log.date - oldlog.date;
                if (timeSpan.TotalMinutes < 5) return Json("Log already exists");
            }
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
            return Json(log);
        }

    }
}
