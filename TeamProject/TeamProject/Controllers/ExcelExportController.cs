﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormGenerator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TeamProject.Generators;
using TeamProject.Helpers;
using TeamProject.Models.FieldDependencyModels;
using TeamProject.Models.FieldFieldDependencyModels;
using TeamProject.Models.FormGeneratorModels;
using TeamProject.Models.Modele_pomocnicze;

namespace TeamProject.Controllers
{
    [Authorize]
    public class ExcelExportController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private FormGeneratorContext _context;
        private IFieldDependenciesRepository _dependenciesRepository;
        private XlsxFileGenerator xlsxFileGenerator = new XlsxFileGenerator();
        private const string VIRTUAL_PATH = @"\excelExports\";
        private const string XLSX_MIME_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ExcelExportController(IHostingEnvironment env, FormGeneratorContext context, IFieldDependenciesRepository dependenciesRepository)
        {
            _hostingEnvironment = env;
            _context = context;
            _dependenciesRepository = dependenciesRepository;
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

            var listIdField = GetDependentFieldIds(listOfFields.Fields.Select(x => x.IdField).ToList()).ToList();

            var fields_second_time = _context.Field.Where(x => listIdField.Contains(x.Id)).ToList();

           
                foreach (var f in fields_second_time)
                {
                   
                        field = new FieldForExcel()
                        {

                            IdField = f.Id,
                            NameOfField = f.Name,
                            IsCheck = true
                        };
                        listOfFields.Fields.Add(field);

                    

                }
            

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
            List<int> dependentFieldIds = GetDependentFieldIds(fields).Where(x=>fields.Contains(x)).ToList();
            fields.AddRange(dependentFieldIds);

            List<UserAnswers> userAnswers = _context.UserAnswers.Where(w => w.IdForm == listOfFields.IdForm && fields.Contains(w.IdField)).ToList();
            

            if (userAnswers == null || userAnswers.Count == 0)
            {
                TempData["ExcelExportMessage"] = "Nie można wygenerować pliku xls ponieważ nie ma rekordów spełniających warunek";
                return RedirectToAction(nameof(Index));
            }

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

        private List<int> GetDependentFieldIds(List<int> fieldIds)
        {
            List<int> result = new List<int>();
            if (fieldIds == null && fieldIds.Count == 0)
                return result;

            foreach(int fieldId in fieldIds)
            {
                List<FieldFieldDependency> dependencies = _dependenciesRepository.Dependencies.Where(w => w.Id == fieldId).ToList();
                foreach(FieldFieldDependency dependency in dependencies)
                {
                    List<int> relatedFieldIds = dependency.RelatedFields.Select(s => s.Id).ToList();
                    result.AddRange(relatedFieldIds);
                }
            }
            return result;
        }
    }
}