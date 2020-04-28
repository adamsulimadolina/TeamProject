using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormGenerator.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TeamProject.Generators;
using TeamProject.Helpers;
using TeamProject.Models.FormGeneratorModels;
using TeamProject.Models.Modele_pomocnicze;

namespace TeamProject.Controllers
{
    public class ExcelExportController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private FormGeneratorContext _context;
        private XlsxFileGenerator xlsxFileGenerator = new XlsxFileGenerator();
        private const string VIRTUAL_PATH = @"\excelExports\";
        private const string XLSX_MIME_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ExcelExportController(IHostingEnvironment env, FormGeneratorContext context)
        {
            _hostingEnvironment = env;
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.Message = TempData?["ExcelExportMessage"] ?? "";
            List<SelectListItem> formsList = new List<SelectListItem>();
            var formularze = _context.Forms.ToList();

            foreach (var elem in formularze)
            {
                SelectListItem tmp = new SelectListItem();
                tmp.Text = elem.Name;
                tmp.Value = elem.Id.ToString();
                formsList.Add(tmp);
            }

            ViewBag.List = formsList;
            return View();
        }
        public IActionResult Details(int id)
        {
            
            List<SelectListItem> formsList = new List<SelectListItem>();
            var formularze = _context.Forms.ToList();
            foreach (var elem in formularze)
            {
                SelectListItem tmp = new SelectListItem();
                tmp.Text = elem.Name;
                tmp.Value = elem.Id.ToString();
                formsList.Add(tmp);
            }
            ViewBag.List = formsList;

            ListOfFields listOfFields = new ListOfFields();
            var fields = _context.Field.ToList();
            var formField = _context.FormField.Where(x => x.IdForm.Equals(id)).ToList();

            var field = new FieldForExcel();

            listOfFields.Fields = new List<FieldForExcel>();

            foreach (var item in formField)
            {
                foreach (var f in fields)
                {
                    if (f.Id.Equals(item.IdField))
                    {
                        field = new FieldForExcel()
                        {

                            IdField = item.IdField,
                            NameOfField = f.Name,
                            IsCheck = true
                        };
                        listOfFields.Fields.Add(field);
                        
                    }

                }
            }
            listOfFields.IdForm = id;
            listOfFields.NumberOfRecords = 0;
            listOfFields.IsAllRecords = true;


            return View(listOfFields);
        }


        public IActionResult DownloadXlsxFile(ListOfFields listOfFields)
        {
            Forms form = _context.Forms.AsNoTracking().FirstOrDefault(f => f.Id == listOfFields.IdForm);
            if (form == null || (listOfFields.IsAllRecords==false && listOfFields.NumberOfRecords<1))
            {
                TempData["ExcelExportMessage"] = "Ilość zwracanych rekordów musi być większa od 0";
                return RedirectToAction(nameof(Index));
            }
            List<int> fields = listOfFields.Fields.Where(x => x.IsCheck == true).Select(x => x.IdField).ToList();

            List<UserAnswers> userAnswers = _context.UserAnswers.Where(w => w.IdForm == listOfFields.IdForm && fields.Contains(w.IdField)).ToList();

            List<Dictionary<string, object>> result = xlsxFileGenerator.GenerateRequiredDataTypeForUserAnswers(userAnswers, _context);
            result = result.Take(listOfFields.IsAllRecords ? result.Count : listOfFields.NumberOfRecords).ToList();


            ExcelPackage excelPackage = xlsxFileGenerator.CreateXlsxFile(result);
            FileProcessor fileProcessor = new FileProcessor(_hostingEnvironment);
            string filename = Guid.NewGuid().ToString();
            fileProcessor.SaveExcelFileToAppFolder(VIRTUAL_PATH, filename, excelPackage);
            byte[] fileData = System.IO.File.ReadAllBytes(_hostingEnvironment.ContentRootPath + VIRTUAL_PATH + filename + ".xlsx");
            fileProcessor.DeleteFileFromAppFolder(VIRTUAL_PATH + filename + ".xlsx");
            return File(fileData, XLSX_MIME_TYPE, $"{form.Name}.xlsx");
        }
    }
}