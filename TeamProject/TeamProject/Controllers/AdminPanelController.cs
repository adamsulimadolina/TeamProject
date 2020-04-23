using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FormGenerator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeamProject.Models;
using PagedList;

namespace TeamProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelController : Controller
    {
        private readonly FormGeneratorContext _context;
        private readonly UserManager<MyUser> _userManager;
        public AdminPanelController(FormGeneratorContext context, UserManager<MyUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }     
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Logi()
        {
            return View();
        }
        public ViewResult Users(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";           
            searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;
            var users = from u in _userManager.Users
                        select u;
            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    users = users.OrderBy(s => s.CustomID);
                    break;
                case "date_desc":
                    users = users.OrderByDescending(s => s.CustomID);
                    break;
                default:  // Name ascending 
                    users = users.OrderBy(s => s.LastName);
                    break;
            }

            
            return View(users);
        }
        public async Task<IActionResult> manageUser(int id)
        {
            return View();
        }

    }
}