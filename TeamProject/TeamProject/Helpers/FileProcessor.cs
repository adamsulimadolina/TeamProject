﻿using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TeamProject.Helpers
{
    public class FileProcessor
    {
        private IHostingEnvironment _hostingEnvironment;
        private string _appRootFolder;
        public FileProcessor(IHostingEnvironment env)
        {
            _hostingEnvironment = env;
            _appRootFolder = _hostingEnvironment.ContentRootPath;
        }

        public string SaveExcelFileToAppFolder(string appVirtualFolderPath, string filename, ExcelPackage excelPackage)
        {
            string pathToFile = _appRootFolder + appVirtualFolderPath + filename+".xlsx";
            FileInfo fileInfo = new FileInfo(pathToFile);
            Directory.CreateDirectory(_appRootFolder+appVirtualFolderPath);
            excelPackage.SaveAs(fileInfo);
            return pathToFile;
        }

        public void DeleteFileFromAppFolder(string virtualPathToFileWithExtension)
        {
            string pathToFile = _appRootFolder + virtualPathToFileWithExtension;
            File.Delete(pathToFile);
        }
    }
}
