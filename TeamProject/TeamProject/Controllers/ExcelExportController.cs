using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormGenerator.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace TeamProject.Controllers
{
    public class ExcelExportController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private FormGeneratorContext _context;
        private const string VIRTUAL_PATH = @"\excelExports\";
        private const string XLSX_MIME_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ExcelExportController(IHostingEnvironment env, FormGeneratorContext context)
        {
            _hostingEnvironment = env;
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.Message = _hostingEnvironment.ContentRootPath + VIRTUAL_PATH;
            return View();
        }

        public IActionResult DownloadXlsxFile(string fileGuid)
        {
            string fileNameWithExtension = _context.GUIDFileNameMap.FirstOrDefault(f => f.Guid == fileGuid).FileName;
            string pathToFile = _hostingEnvironment.ContentRootPath + VIRTUAL_PATH + fileNameWithExtension;
            byte[] fileData = System.IO.File.ReadAllBytes(pathToFile);
            return File(fileData, XLSX_MIME_TYPE);
        }
    }
}