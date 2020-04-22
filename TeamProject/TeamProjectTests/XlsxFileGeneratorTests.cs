using System;
using System.Collections.Generic;
using System.Text;
using TeamProject.Generators;
using TeamProject.Models.FormGeneratorModels;
using Xunit;

namespace TeamProjectTests
{
    public class XlsxFileGeneratorTests
    {
        [Fact]
        public void XlsxFileGeneratorTest()
        {
            XlsxFileGenerator fileGenerator = new XlsxFileGenerator();
            var test = fileGenerator.GenerateRequiredDataTypeForList(GetExampleAnswers);
            fileGenerator.CreateXlsxFile(test, "goldus");

        }

        public List<Dictionary<string, object>> ExampleData = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                ["FormId"] = 2,
                ["PatientId"] = 3,
                ["Answer"] = "Valsol"
            },
            new Dictionary<string, object>
            {
                ["FormId"] = 3,
                ["PatientId"] = 3,
                ["Answer"] = "Yes"
            }
        };

        public List<Answers> GetExampleAnswers = new List<Answers>
        {
            new Answers{ Id = 1, Answer = "tak"},
            new Answers{Id =3, Answer= "golden"}
        };
    }
}
