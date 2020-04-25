using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormGenerator.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using TeamProject.Generators;
using TeamProject.Helpers;
using TeamProject.Models.FormGeneratorModels;

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
            return View();
        }

        public IActionResult DownloadXlsxFile(int formId)
        {
            Forms form = _context.Forms.AsNoTracking().FirstOrDefault(f => f.Id == formId);
            if (form == null)
            {
                return RedirectToAction(nameof(Index));
            }
            List<UserAnswers> userAnswers = _context.UserAnswers.Where(w => w.IdForm == formId).ToList();
            List<Dictionary<string, object>> result = xlsxFileGenerator.GenerateRequiredDataTypeForUserAnswers(userAnswers,_context);
            ExcelPackage excelPackage = xlsxFileGenerator.CreateXlsxFile(result);
            FileProcessor fileProcessor = new FileProcessor(_hostingEnvironment);
            string filename = Guid.NewGuid().ToString();
            fileProcessor.SaveExcelFileToAppFolder(VIRTUAL_PATH, filename, excelPackage);
            byte[] fileData = System.IO.File.ReadAllBytes(_hostingEnvironment.ContentRootPath + VIRTUAL_PATH + filename + ".xlsx");
            fileProcessor.DeleteFileFromAppFolder(VIRTUAL_PATH + filename + ".xlsx");
            return File(fileData,XLSX_MIME_TYPE,$"{form.Name}.xlsx");
        }
    }
}