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
using Microsoft.AspNetCore.Http;

namespace FormGenerator.Controllers
{
    [Authorize()]
    public class FormsController : Controller
    {
        private readonly FormGeneratorContext _context;
        private readonly UserManager<MyUser> _userManager;
        private readonly IFieldDependenciesRepository pomik;//mikroserwis z ktorego korzysta kontroler

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
            ViewBag.FORMID = id;
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
                var dependencie = lista.Where(l => l.ThisField.Equals(key)).ToList();
                if (dependencie.Count > 0 ) {
                    pom.Dependencies = dependencie;
                    //for (int x = 0; x < dependencie.RelatedFields.Count; x++)
                    //    pom.DepndenciesValue.Add(new StringBoolType());

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

            foreach(var x in fieldWithValues)
            {
                foreach(var y in x.Dependencies)
                {
                    x.podrzedneFieldAnswers.Add(y.IdDependency, new List<StringBoolType>());
                    foreach (var z in y.RelatedFields)
                    {
                        x.podrzedneFieldAnswers[y.IdDependency].Add(new StringBoolType());
                    }
                }
            }



            return View(fieldWithValues);
        }

        // w tej metodzie w przyszłości nastąpi wysłanie wpisanych formularzy do bazy danych
        [HttpPost]
        public async Task<IActionResult> Formularz(List<FieldWithValue> fields, int formId, int patientId)
        {
            int? current_test = HttpContext.Session.GetInt32("current_test");
            MyUser user = await GetUser();
            await UpdateSendStatusForPacjentForms(patientId, formId, current_test);


            foreach (var field in fields)
            {
                UserAnswers answer = new UserAnswers
                {
                    IdForm = formId,
                    IdField = field.Field.Id,
                    IdPatient = patientId,
                    IdUser = user.CustomID,
                    IdTest = (int) current_test
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

                var Dependencies = pomik.Dependencies;
                foreach (var x in field.podrzedneFieldAnswers)
                {
                    var relatedFields = Dependencies.FirstOrDefault(el => el.IdDependency == x.Key).RelatedFields;
                    for(int i =0; i<x.Value.Count; i++)
                    {
                        UserAnswers userAnswers = new UserAnswers
                        {
                            Answer = relatedFields[i].Type == "checkbox" ? x.Value[i].boolVal.ToString() : x.Value[i].textVal,
                            IdField = relatedFields[i].Id,
                            IdForm = formId,
                            IdTest = (int)current_test,
                            IdPatient = patientId,
                            IdUser = user.CustomID
                        };
                        _context.Add(userAnswers);
                    }
                }
            }
            _context.SaveChanges();

           
            return View("WyslanoFormularz", fields);
        }

        public async Task  UpdateSendStatusForPacjentForms(int IdPacjenta, int IdFormularza,int? Idtest)
        {

            var pacjentForm = _context.PatientForms.FirstOrDefault(f => f.IdPatient == IdPacjenta && f.IdForm == IdFormularza&&f.IdTest==Idtest);
            pacjentForm.IsSendBefore = true;
            _context.PatientForms.Update(pacjentForm);
            await _context.SaveChangesAsync();
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
                List<Test> tests = await _context.Tests.ToListAsync(); 
                foreach (var patient in patients)
                {
                    foreach (var test in tests)
                    {
                        if (test.IdPatient == patient.IdPatient)
                        {
                            _context.PatientForms.Add(new PatientForms()
                            {
                                IdPatient = patient.IdPatient,
                                IdTest = test.IdTest,
                                IdForm = forms.Id,
                                agreement = true
                            });
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Forms");
            }
            return View(forms);
        }


      

        public async Task<IActionResult> EdycjaWypelnionegoFormularza(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.FORMID = id;

            MyUser user = await GetUser();
            int? current_id_pacjent = HttpContext.Session.GetInt32("current_id_pacjent");
            int? current_test = HttpContext.Session.GetInt32("current_test");



            List<UserAnswers> answerList = _context.UserAnswers.Where(x => x.IdTest == current_test && x.IdPatient == current_id_pacjent && x.IdForm == id).ToList();

            List<FieldWithValue> fieldWithValues = new List<FieldWithValue>();
        

           
            //wyszukanie oraz przekonwertowanie do listy Id pól które są dołączone do formularza
            //bierzemy pod uwagę tylko id pól należących do formularza
            var fieldsIdsInForm = _context.FormField.Where(ff => ff.IdForm == id).Select(ff => ff.IdField).ToList();
            //pobieramy dane pól których id pobrano powyżej
            var fieldsInForm = _context.Field.Where(f => fieldsIdsInForm.Contains(f.Id)).ToList();
            if (fieldsInForm == null)
            {
                return NotFound();
            }


            List<Field> nadrzedne = new List<Field>();
            var allDependencyFromFormularz = pomik.Dependencies.Where(d => fieldsIdsInForm.Contains(d.Id)).ToList();

            foreach (Field f in fieldsInForm)//wszystkie pola w formularzu
            {
                bool znaleziono = false;
                foreach (FieldFieldDependency dep in allDependencyFromFormularz)
                {
                    foreach (Field depField in dep.RelatedFields)
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

                if (znaleziono == false)
                    nadrzedne.Add(f);
            }

            //przepisywanie danych do odpowiedniego modelu. ma ktoś pomysł jak bardziej optymalnie przenosić???
            foreach (var key in nadrzedne)
            {
                FieldWithValue pom = new FieldWithValue();
                pom.TextValue = answerList.Where(x => x.IdField == key.Id).First().Answer;
                pom.Field.Id = key.Id;
                pom.Field.Name = key.Name;
                pom.Field.Type = key.Type;

                var dependencie = allDependencyFromFormularz.Where(l => l.ThisField.Equals(key)).ToList();

                if (dependencie.Count > 0)
                {
                    pom.Dependencies = dependencie;
                    for (int x = 0; x < dependencie.Count; x++)
                        pom.podrzedneFieldAnswers.Where(z => z.Key == pom.Field.Id);

                }


                fieldWithValues.Add(pom);
            }



            var Dependencies = pomik.Dependencies;

            ViewBag.modelcount = fieldWithValues.Count;
         
            ViewBag.formid = Convert.ToInt32(id);
            try
            {
                fieldWithValues[0].options = _context.SelectFieldOptions.ToList();
            }
            catch (Exception e) { }

            foreach (var x in fieldWithValues)
            {
                foreach (var y in x.Dependencies)
                {
                    x.podrzedneFieldAnswers.Add(y.IdDependency, new List<StringBoolType>());
                    foreach (var z in y.RelatedFields)
                    {
                        x.podrzedneFieldAnswers[y.IdDependency].Add(new StringBoolType());
                    }
                }
            }


            foreach (var item in fieldWithValues)
            {


                foreach (var fieldPodrzedne in item.podrzedneFieldAnswers)
                {
                    var relatedFields = Dependencies.FirstOrDefault(el => el.IdDependency == fieldPodrzedne.Key).RelatedFields;
                    // item.podrzedneFieldAnswers[fieldPodrzedne.Key] = answerList.Where(x => x.IdField == fieldPodrzedne.Key).
                    for (int i = 0; i < relatedFields.Count; i++)
                    {
                        if (relatedFields[i].Type == "checkbox")
                        {
                            fieldPodrzedne.Value[i].boolVal = bool.Parse(answerList.Where(x=>x.IdField== relatedFields[i].Id).FirstOrDefault().Answer);
                        }
                        else
                        {
                            fieldPodrzedne.Value[i].textVal = answerList.Where(x => x.IdField == relatedFields[i].Id).FirstOrDefault().Answer;
                            
                        }

                    }
                }


            }



            return View(fieldWithValues);
        }


        [HttpPost]
        public async Task<IActionResult> EdycjaWypelnionegoFormularza(List<FieldWithValue> fields, int formId)
        {
            int? current_id_pacjent = HttpContext.Session.GetInt32("current_id_pacjent");
            int? current_test = HttpContext.Session.GetInt32("current_test");
            MyUser user = await GetUser();
     

            List<UserAnswers> answerList = _context.UserAnswers.Where(x => x.IdTest == current_test && x.IdPatient == current_id_pacjent  && x.IdForm == formId).ToList();


            foreach (var field in fields)
            {
                UserAnswers answer = answerList.FirstOrDefault(x=>x.IdField==field.Field.Id);
                answer.IdField = field.Field.Id;
                answer.IdForm = formId;
                answer.IdPatient = (int)current_id_pacjent;
                answer.IdUser = user.CustomID;
                answer.IdTest = (int)current_test;
           

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
                
                _context.UserAnswers.Update(answer);
              
                var Dependencies = pomik.Dependencies;
                foreach (var x in field.podrzedneFieldAnswers)
                {
                    var relatedFields = Dependencies.FirstOrDefault(el => el.IdDependency == x.Key).RelatedFields;
                    for (int i = 0; i < x.Value.Count; i++)
                    {

                        UserAnswers userAnswers = answerList.FirstOrDefault(z=> z.IdField == relatedFields[i].Id);
                        userAnswers.IdField = relatedFields[i].Id;
                        userAnswers.IdForm = formId;
                        userAnswers.IdPatient = (int)current_id_pacjent;
                        userAnswers.IdUser = user.CustomID;
                        userAnswers.IdTest = (int)current_test;
                        userAnswers.Answer = relatedFields[i].Type == "checkbox" ? x.Value[i].boolVal.ToString() : x.Value[i].textVal;

                       _context.UserAnswers.Update(userAnswers);
                    }
                }
            }
            await _context.SaveChangesAsync();
            ViewBag.EdycjaFormularza = "Formularz został zedytowany";

            return View("WyslanoFormularz", fields);
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
            var listFormsinEntenceForms = _context.EntranceConnections.Where(m => m.IdForm == id).ToList();
            foreach (var elem in listFormsinEntenceForms)
            {
                _context.EntranceConnections.Remove(elem);
            }

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
