using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FormGenerator.Models;
using FormGenerator.Models.Modele_pomocnicze;
using TeamProject.Models.Modele_pomocnicze;
using Microsoft.AspNetCore.Authorization;
using TeamProject.Models.FormGeneratorModels;



namespace TeamProject.Controllers
{
    [Authorize]
    public class UserAnswerListsController : Controller
    {
        private readonly FormGeneratorContext _context;

        public UserAnswerListsController(FormGeneratorContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AnswerList()
        {
            List<SelectListItem> formsList = new List<SelectListItem>();
            /*var usersAnswers = _context.UserAnswers
                .Where(m => m.IdForm.Equals(id))
                .ToList();*/
            var forms = _context.Forms
                .ToList();

            foreach (var elem in forms)
            {
                SelectListItem tmp = new SelectListItem();
                tmp.Text = elem.Name;
                tmp.Value = elem.Id.ToString();
                formsList.Add(tmp);
            }


            ViewBag.List = formsList;
            var usersAnswers = _context.UserAnswers
                .ToList();
            List<UserAnswerList> answersList = new List<UserAnswerList>();
            bool exist = false;
            bool doubled = false;
            int idx = -1;
            UserAnswerList newUAL;
            foreach (var elem in usersAnswers)
            {
                exist = false;
                doubled = false;
                for (int i = 0; i < answersList.Count; i++)
                {
                    for (int j = 0; j < answersList[i].user_answer_list.Count; j++)
                    {
                        if (answersList[i].user_answer_list[j].IdForm == elem.IdForm &&
                            answersList[i].user_answer_list[j].IdField == elem.IdField &&
                            answersList[i].user_answer_list[j].IdUser == elem.IdUser)
                        {
                            doubled = true;
                        }
                    }
                    if (answersList[i].Id_User.Equals(elem.IdUser))
                    {
                        idx = i;
                        exist = true;
                    }
                }
                if (!exist)
                {
                    newUAL = new UserAnswerList();
                    newUAL.Id_User = elem.IdUser;
                    newUAL.user_answer_list.Add(elem);
                    answersList.Add(newUAL);
                }
                else
                {
                    if (!doubled) answersList[idx].user_answer_list.Add(elem);

                }
            }

            /*var _form = _context.FormField
                    .Where(m => m.IdForm.Equals(id))
                    .Select(m => m.IdField)
                    .ToList();

            var _fields = _context.Field
                 .Where(m => _form.Contains(m.Id))
                 .ToList();*/

            var _form = _context.FormField
                    .Select(m => m.IdField)
                    .ToList();
            var _fields = _context.Field
                 .Where(m => _form.Contains(m.Id))
                 .ToList();
            ViewBag.Fields = _fields;
            ViewBag.FormId = 0;

            var xD = await _context.UserAnswerList
                .Where(m => m.user_answer_list.Count > 0)
                .ToListAsync();

            return View(answersList);
        }

        public async Task<IActionResult> AnswerListPost(int id)
        {
            var translations = _context.TableNameTranslations.OrderBy(x => x.DatabaseName).ToList();
            List<SelectListItem> formsList = new List<SelectListItem>();
            var forms = _context.Forms
                .ToList();

            foreach (var elem in forms)
            {
                SelectListItem tmp = new SelectListItem();
                tmp.Text = elem.Name;
                tmp.Value = elem.Id.ToString();
                formsList.Add(tmp);
            }


            ViewBag.List = formsList;

            var usersAnswers = _context.UserAnswers
                .Where(m => m.IdForm.Equals(id))
                .ToList();

            List<UserAnswerList> answersList = new List<UserAnswerList>();
            bool exist = false;
            bool doubled = false;
            int idx = -1;
            UserAnswerList newUAL;
            foreach (var elem in usersAnswers)
            {
                exist = false;
                doubled = false;
                for (int i = 0; i < answersList.Count; i++)
                {
                    for (int j = 0; j < answersList[i].user_answer_list.Count; j++)
                    {
                        if (answersList[i].user_answer_list[j].IdForm == elem.IdForm &&
                            answersList[i].user_answer_list[j].IdField == elem.IdField &&
                            answersList[i].user_answer_list[j].IdPatient == elem.IdPatient)
                        {
                            doubled = true;
                        }
                    }
                    if (answersList[i].Id_User.Equals(elem.IdPatient))
                    {
                        idx = i;
                        exist = true;
                    }
                }
                if (!exist)
                {
                    newUAL = new UserAnswerList();
                    newUAL.Id_User = elem.IdPatient;
                    newUAL.user_answer_list.Add(elem);
                    answersList.Add(newUAL);
                }
                else
                {
                    if (!doubled) answersList[idx].user_answer_list.Add(elem);

                }
            }

            var _form = _context.FormField
                    .Where(m => m.IdForm.Equals(id))
                    .Select(m => m.IdField)
                    .ToList();



            List<int> tmp_list = new List<int>();

            foreach (var elem in usersAnswers)
            {
                tmp_list.Add(elem.IdField);
            }

            var _fields = _context.Field
                 .Where(m => tmp_list.Contains(m.Id))
                 .ToList();
            //zrobić lepsze przuszukiwanie !!!
            foreach(var field in _fields)
            {
                for(int i=0;i<translations.Count;i++)
                {
                    if(translations[i].DatabaseName==field.Name)
                    {
                        field.Name = translations[i].DisplayedName;
                        break;
                    }
                }
            }
            ViewBag.Fields = _fields;
            ViewBag.FormId = id;



            return View(answersList);
        }
        //DANIEL 02.04 - translacja tabel
        [HttpPost]
        public async Task SaveTranslation()
        {
            string basename = Request.Form["baseName"];
            string newname = Request.Form["newName"];
            var trans = await _context.TableNameTranslations.FirstOrDefaultAsync(item => item.DisplayedName == basename);
            if(trans!=null)
            {
                trans.DisplayedName = newname;
                _context.TableNameTranslations.Update(trans);
                await _context.SaveChangesAsync();

            }
            else
            {
                TableNameTranslation translation = new TableNameTranslation()
                {
                    DatabaseName = basename,
                    DisplayedName = newname
                };
                _context.TableNameTranslations.Add(translation);
                await _context.SaveChangesAsync();
            }          
        }
    }
}