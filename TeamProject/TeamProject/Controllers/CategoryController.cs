using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormGenerator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TeamProject.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {

        private readonly FormGeneratorContext _context;

        public CategoryController(FormGeneratorContext context)
        {
            _context = context;
        }

        public IActionResult Create(int id)
        {
            ViewBag.bag = id;
            return View();
        }

        // POST: Forms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Parent")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Forms");
            }
            return View();
        }
        public async Task<IActionResult> ListaKategorii()
        {
            return View(await _context.Categories.ToListAsync());
        }
        [HttpPost]
        public JsonResult GetList(string order)
        {
            return Json(_context.Categories.ToList());
        }
        [HttpPost]
        public JsonResult GetDzieci(string order)
        {
            int id = Convert.ToInt32(order);
            var result = _context.Categories.Where(m => m.Parent == id).ToList();
            return Json(result);
        }
        [HttpPost]
        public JsonResult ListOfCategories(string order)
        {
            int id = Convert.ToInt32(order);
            Category child = _context.Categories.FirstOrDefault(m=> m.Id==id);
            Boolean znaleziono = false;
            int i = 0;
            
            Stack<Category> categories = new Stack<Category>();
            categories.Push(child);
            while ( i < categories.Count + 1)
            {
                 child = _context.Categories.FirstOrDefault(m => m.Id == child.Parent);
                
                if (child != null)
                {
                    categories.Push(child);
                    if (child.Parent == null)
                        break;
                }
                i++;
            }

            return Json(categories);
        }
        public IActionResult CreateParentCategory()
        {
           
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateParentCategory([Bind("Id,Name,Parent")] Category category)
        {

            if (ModelState.IsValid)
            {
                category.Parent = null;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Forms", new { id = 1 });
            }
            return View();
        }
    }
}