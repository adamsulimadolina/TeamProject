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
using FormGenerator.Models;
using TeamProject.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

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
            worksheet.Cells.AutoFitColumns();
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

        public List<Dictionary<string, object>> GenerateRequiredDataTypeForUserAnswers(List<UserAnswers> userAnswers, FormGeneratorContext context)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            Dictionary<string, object> headersTemplate = new Dictionary<string, object>
            {
                [nameof(UserAnswers.IdPatient)] = "",
                [nameof(UserAnswers.IdTest)] = ""
            };
            AddKeysBasedOnFieldId(userAnswers, headersTemplate);

            int[] allTestIds = userAnswers.Select(s => s.IdTest).Distinct().ToArray();
            int[] allPatientIds = userAnswers.Select(s => s.IdPatient).Distinct().ToArray();

            foreach (int testId in allTestIds)
            {
                foreach (int patientId in allPatientIds)
                {
                    Dictionary<string, object> currentRow = GenerateCurrentRow(userAnswers, headersTemplate, testId, patientId);
                    if (currentRow != null)
                        result.Add(currentRow);
                }
            }
            ReplaceKeyIdToFieldName(result, context);
            return result;
        }

        private static Dictionary<string, object> GenerateCurrentRow(List<UserAnswers> userAnswers, Dictionary<string, object> headersTemplate, int testId, int patientId)
        {
            Dictionary<string, object> currentRow = new Dictionary<string, object>(headersTemplate);
            bool answerExists = false;

            foreach (string key in headersTemplate.Keys)
            {
                currentRow[key] = userAnswers.FirstOrDefault(f => f.IdPatient == patientId && f.IdTest == testId && f.IdField.ToString() == key)?.Answer;
                if (currentRow[key] != null)
                    answerExists = true;
            }

            currentRow[nameof(UserAnswers.IdPatient)] = patientId;
            currentRow[nameof(UserAnswers.IdTest)] = testId;
            return answerExists ? currentRow : null;
        }

        private void AddKeysBasedOnFieldId(List<UserAnswers> userAnswers, Dictionary<string, object> headersTemplate)
        {
            int examplePatientId = userAnswers.FirstOrDefault().IdPatient;
            int[] allFieldIds = userAnswers.Where(w => w.IdPatient == examplePatientId).Select(s => s.IdField).Distinct().ToArray();
            foreach (int fieldId in allFieldIds)
                headersTemplate.Add(fieldId.ToString(), new object());
        }

        private void ReplaceKeyIdToFieldName(List<Dictionary<string, object>> data, FormGeneratorContext context)
        {
            var fieldNamesToReplace = context.Field.AsNoTracking().Where(w => data[0].ContainsKey(w.Id.ToString())).Select(s => new { s.Id, s.Name }).ToList();
            List<string> oldKeys = new List<string>(data.First().Keys);

            foreach (Dictionary<string, object> item in data)
            {
                foreach (string key in oldKeys)
                {
                    string fieldName = fieldNamesToReplace.FirstOrDefault(f => f.Id.ToString() == key)?.Name;
                    if (fieldName != null)
                        item.ChangeKey(key, fieldName);
                }
            }
        }
    }
}
