using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FormGenerator.Models;
using TeamProject.Models.Modele_pomocnicze;
using System.Diagnostics;
using TeamProject.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using TeamProject.Models.NewTypeAndValidation;
using Newtonsoft.Json;

namespace FormGenerator.Controllers
{
    [Authorize()]
    public class FieldsController : Controller
    {
        private readonly FormGeneratorContext _context;
        

        public FieldsController(FormGeneratorContext context)
        {
            _context = context;
            
        }

        // GET: Fields
        public async Task<IActionResult> Index()
        {
            return View(await _context.Field.ToListAsync());
        }

        // GET: Fields/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @field = await _context.Field
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@field == null)
            {
                return NotFound();
            }

            return View(@field);
        }

        // GET: Fields/Create
        public IActionResult Create()
        {            
            return View();
        }

        // POST: Fields/Create
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
                return RedirectToAction(nameof(Index));
            }
            return View(@field);
        }
        //przeładowane metody w celu dodaniapytania od razu do formularza
        
       
        public IActionResult AddNewField(int? id)
        {
            if ((TempData["Error"] = _context.Forms.AsNoTracking().FirstOrDefault(form => form.Id == id).Validate(_context)) != null)
            {
                return RedirectToAction("Index","Forms"); //gdy już formularz został wypełniony-> nie można go edytować
            }

            ViewBag.formid = Convert.ToInt32(id);
            NewFieldList newFieldList = new NewFieldList();
            var idFieldsInForm = _context.FormField.Where(ff => ff.IdForm == id)
                .Select(ff => ff.IdField).ToList();
            var fieldsInForm = _context.Field.Where(f => idFieldsInForm.Contains(f.Id)).ToList();
            List<FieldWithValidation> fieldWithValidations = new List<FieldWithValidation>();
            foreach(var item in fieldsInForm)
            {
                FieldWithValidation pom = new FieldWithValidation
                {
                    Id = item.Id,
                    Name = item.Name,
                    Type = item.Type
                };
                fieldWithValidations.Add(pom);
            }
            newFieldList.fields = fieldWithValidations;
            newFieldList.FormId =Convert.ToInt32(id);
            return View(newFieldList);
        }
        [HttpPost]
        public IActionResult AddToList(NewFieldList newFieldList)
        {
            //jeśli pole które chcemy stworzyć znajduje się już w systemie->nie pozwalamy
            //if (_context.Field.AsNoTracking().FirstOrDefault(f => f.Name == newFieldList.currentNameToCreate) != null)
            //{
            //    TempData["Error"] = "Pole o podanej nazwie już istnieje w systemie. Nie można dodać pola o tej samej nazwie!";
            //    return View("AddNewField", newFieldList);
            //}

            FieldWithValidation field;
            if (newFieldList.currentName == "")
                return View("AddNewField", newFieldList);

            var containsCurrentName = newFieldList.fields.Where(f => f.Name == newFieldList.currentName);
            var condatinsCreateName = newFieldList.fields.Where(f => f.Name == newFieldList.currentNameToCreate);
            if(condatinsCreateName.Count()!=0 || containsCurrentName.Count() != 0)
            {
                ViewBag.Error = "Pole o tej nazwie znajduje się już w formularzu!";
                return View("AddNewField", newFieldList);
            }

            if (newFieldList.currentId != 0)
            {
                field = new FieldWithValidation
                {
                    Id=newFieldList.currentId,
                    Name = newFieldList.currentName,
                    Type = newFieldList.currentType
                };
            }
            else
            {
                field = new FieldWithValidation
                {
                    Name = newFieldList.currentNameToCreate,
                    Type = newFieldList.currentTypeToCreate
                };



            }
            newFieldList.fields.Add(field);
            var concreteField = newFieldList.fields.Where(f => f == field).ToList()[0];
            //jeśli polem jest select-> należy dodać do listy "pól do dołączenia(fields)" opcje
            if (newFieldList.currentTypeToCreate == "select")
            {
                List<SelectFieldOptionsJsonData> jsonDatas = JsonConvert.DeserializeObject<List<SelectFieldOptionsJsonData>>(newFieldList.optionsJson);
                jsonDatas = jsonDatas.Where(w => w.Wartosc != null).ToList();
                foreach(var option in jsonDatas)
                {
                    var selectOption = new SelectFieldOptions();
                    selectOption.option = option.Wartosc;
                    concreteField.options.Add(selectOption);
                }
            }

            if (newFieldList.currentTypeToCreate == "number")//w przypadku gdy pole jest typu number z walidacją, oraz jest nowe(nie ma id)
            {
                decimal pom;
                bool parseSuccess = Decimal.TryParse(newFieldList.minString, out pom);
                if (parseSuccess)
                    newFieldList.min.value = pom;
                parseSuccess = Decimal.TryParse(newFieldList.maxString, out pom);
                if (parseSuccess)
                    newFieldList.max.value = pom;
            }


            if (newFieldList.currentTypeToCreate=="number" && field.Id==0)
            {
                if (newFieldList.min.value != null)
                {
                    newFieldList.min.type = "min";
                    concreteField.validations.Add(newFieldList.min);
                }
                if (newFieldList.max.value!=null)
                {
                    newFieldList.max.type = "max";
                    concreteField.validations.Add(newFieldList.max);
                }
                //kod 100 oznacza że pole może mieć tylko wartości całkowite
                if (newFieldList.integerVal.value == 100)
                {
                    newFieldList.integerVal.type = "integerVal";
                    concreteField.validations.Add(newFieldList.integerVal);
                }

            }


            TempData.Put<NewFieldList>("newFieldListModel", newFieldList);
            return RedirectToAction("ListWithFields");
        }

        public IActionResult DeleteFromList(int fieldId, NewFieldList newFieldList)
        {

            newFieldList.fields = newFieldList.fields.Where(f => f.Id != fieldId).ToList();
            TempData.Put<NewFieldList>("newFieldListModel", newFieldList);
            return RedirectToAction("ListWithFields");
        }

        [HttpGet]
        public IActionResult ListWithFields()
        {
            var newFieldList = TempData.Get<NewFieldList>("newFieldListModel");
            newFieldList.currentId = 0;
            newFieldList.currentName = null;
            newFieldList.currentNameToCreate = null;
            newFieldList.currentTypeToCreate = null;
            newFieldList.currentType = null;
            newFieldList.minString = null;
            newFieldList.maxString = null;
            newFieldList.min = new Validation();
            newFieldList.max = new Validation();
            newFieldList.integerVal = new Validation();
            newFieldList.optionsToCurrentField = new List<SelectFieldOptions>();
            newFieldList.optionsJson = "";
            return View("AddNewField", newFieldList);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewField(NewFieldList newFieldList)
        {
            //wejście so bazy
            var toRemove = _context.FormField.Where(ff => ff.IdForm == newFieldList.FormId).ToList();
            foreach (var toremove in toRemove)
                _context.FormField.Remove(toremove);
            await _context.SaveChangesAsync();

            foreach(var item in newFieldList.fields)
            {
                if (item.Id != 0)
                {
                    FormField ff = new FormField
                    {
                        IdField = item.Id,
                        IdForm = newFieldList.FormId
                    };

                    _context.FormField.Add(ff);
                }
                //nie istnieje pole
                else
                {
                    Field newField = new Field
                    {
                        Name = item.Name,
                        Type = item.Type
                    };
                    _context.Field.Add(newField);
                    _context.SaveChanges();
                    var thisField = _context.Field.Where(f => f == newField).ToList()[0];
                    FormField ff = new FormField
                    {
                        IdField = thisField.Id,
                        IdForm = newFieldList.FormId
                    };
                    //dodanie dla selectów ich odpowiedzi

                    foreach(var selectOption in item.options)
                    {
                        selectOption.idField = ff.IdField;
                        _context.SelectFieldOptions.Add(selectOption);
                    }
                    //dodanie ich walidacji:
                    foreach(var validationItem in item.validations)
                    {
                        validationItem.idField = ff.IdField;
                        _context.Validations.Add(validationItem);
                    }

                    _context.FormField.Add(ff);
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Formularz", "Forms", new { id = newFieldList.FormId });
        }
        // GET: Fields/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @field = await _context.Field.FindAsync(id);
            if (@field == null)
            {
                return NotFound();
            }
            return View(@field);
        }

        // POST: Fields/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type")] Field @field)
        {
            if (id != @field.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@field);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FieldExists(@field.Id))
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
            return View(@field);
        }

        // GET: Fields/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @field = await _context.Field
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@field == null)
            {
                return NotFound();
            }

            return View(@field);
        }

        

        // GET: Fields1/Edit/5
        public async Task<IActionResult> EditNewField(int? id,int form)
        {
            ViewBag.IDFORM = form;
            
            if (id == null)
            {
                return NotFound();
            }

            var @field = await _context.Field.FindAsync(id);
            if (@field == null)
            {
                return NotFound();
            }
            return View(@field);
        }

        // POST: Fields1/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNewField(int id, [Bind("Id,Name,Type")] Field @field,int form)
        {
            ViewBag.IDFORM = form;
            if (id != @field.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@field);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FieldExists(@field.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                ViewBag.message = true;
                return View();
            }
            return View(@field);
        }


        // GET: Fields1/Delete/5
        public async Task<IActionResult> DeleteNewField(int? id,int form)
        {
            ViewBag.IDFORM = form;
            if (id == null)
            {
                return NotFound();
            }

            var @field = await _context.Field
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@field == null)
            {
                return NotFound();
            }

            return View(@field);
        }

        // POST: Fields1/Delete/5
        [HttpPost, ActionName("DeleteNewField")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNewFieldConfirmed(int id,int form)
        {
            ViewBag.IDFORM = form;
            var @field = await _context.Field.FindAsync(id);
            _context.Field.Remove(@field);
            await _context.SaveChangesAsync();
            ViewBag.message = true;
            return View();
        }

        public async Task<IActionResult> DetailsNewField(int? id, int form)
        {
            ViewBag.IDFORM = form;
            if (id == null)
            {
                return NotFound();
            }

            var @field = await _context.Field
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@field == null)
            {
                return NotFound();
            }

            return View(@field);
        }


        // POST: Fields/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @field = await _context.Field.FindAsync(id);
            _context.Field.Remove(@field);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
      
        private bool FieldExists(int id)
        {
            return _context.Field.Any(e => e.Id == id);
        }
    }
}
