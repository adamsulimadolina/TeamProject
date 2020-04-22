using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamProject.Models.FormGeneratorModels;
using System.Reflection;

namespace TeamProject.Generators
{
    public class XlsxFileGenerator
    {
        public ExcelPackage CreateXlsxFile(List<Dictionary<string, object>> data)
        {
            ExcelPackage excelPackage = new ExcelPackage();
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
            GenerateWorksheetBasedOnData(data, worksheet);
            return excelPackage;
        }
        private void GenerateWorksheetBasedOnData(List<Dictionary<string, object>> data, ExcelWorksheet worksheet)
        {
            string[] columns = data?[0].Keys.Select(s => s).ToArray();
            GenerateHeadersWithColumns(worksheet, columns, Color.CornflowerBlue);
            FillWorksheetWithData(data, worksheet, columns);
        }
        private void GenerateHeadersWithColumns(ExcelWorksheet worksheet, string[] columns, Color backgroundColor)
        {
            for (int i = 1; i < columns.Length + 1; i++)
            {
                worksheet.Cells[1, i].Value = columns[i - 1];
                worksheet.Cells[1, i].Style.Font.Bold = true;
                worksheet.Cells[1, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(backgroundColor);
            }
        }
        private void FillWorksheetWithData(List<Dictionary<string, object>> data, ExcelWorksheet worksheet, string[] columns)
        {
            for (int row = 2; row < data.Count + 2; row++)
            {
                int columnIndex = 1;
                foreach (string column in columns)
                {
                    worksheet.Cells[row, columnIndex++].Value = data[row - 2][column];
                }
            }
        }
        public List<Dictionary<string, object>> GenerateRequiredDataTypeForList<T>(List<T> dataList)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            foreach (var item in dataList)
            {
                Dictionary<string, object> currentRow = new Dictionary<string, object>();
                foreach (PropertyInfo property in item.GetType().GetProperties())
                {
                    currentRow.Add(property.Name, property.GetValue(item));
                }
                result.Add(currentRow);
            }
            return result;
        }
    }
}
