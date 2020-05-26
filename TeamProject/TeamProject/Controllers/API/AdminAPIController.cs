using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormGenerator.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeamProject.Models;
using System.Web;
using TeamProject.Models.FormGeneratorModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace TeamProject.Controllers.API
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAPIController : Controller
    {
        private readonly FormGeneratorContext _context;
        private readonly UserManager<MyUser> _userManager;
        public AdminAPIController(FormGeneratorContext context, UserManager<MyUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet("user")]
        public ActionResult getLatestUser()
        {
            var log = _context.Logs.LastOrDefault();
           
            return Json(log);
        }
        [HttpGet("test")]
        public ActionResult getLatestTest()
        {
            var test = _context.Tests.OrderByDescending(u => u.DateOfTest).FirstOrDefault();
           
            return Json(test);
        }
        [HttpGet("logs/{number}")]
        //[Route("/logs/{date}")]
        public async Task<ActionResult> getLogs(int number)
        {
            DateTime dateNow = DateTime.Now;
            List<Logs> logs = new List<Logs>();
            switch(number)
            {
                case 1:
                    dateNow = dateNow.AddHours(-24);
                    logs = await _context.Logs.Where(l => l.date >= dateNow).ToListAsync();
                    break;
                case 7:
                    dateNow = dateNow.AddDays(-7);
                    logs = await _context.Logs.Where(l => l.date >= dateNow).ToListAsync();
                    break;
                case 30:
                    dateNow = dateNow.AddMonths(-1);
                    logs = await _context.Logs.Where(l => l.date >= dateNow).ToListAsync();
                    break;
                case 999:
                    logs = await _context.Logs.ToListAsync();
                    break;
            }
                    
            return Json(logs);
        }
        [HttpGet("tests/{number}")]
        public async Task<ActionResult> getTests(int number)
        {
            DateTime dateNow = DateTime.Now;
            int tests=0;
            switch (number)
            {
                case 1:
                    dateNow = dateNow.AddHours(-24);
                    tests = await _context.Tests.Where(l => l.DateOfTest >= dateNow).CountAsync();
                    break;
                case 7:
                    dateNow = dateNow.AddDays(-7);
                    tests = await _context.Tests.Where(l => l.DateOfTest >= dateNow).CountAsync();
                    break;
                case 30:
                    dateNow = dateNow.AddMonths(-1);
                    tests = await _context.Tests.Where(l => l.DateOfTest >= dateNow).CountAsync();
                    break;
                case 999:
                    tests = await _context.Tests.CountAsync();
                    break;
            }

            return Json(tests);
        }
        [HttpGet("periodTests/{number}")]
        public async Task<ActionResult> getPeriodTests(int number)
        {
            DateTime dateNow = DateTime.Now;
            List<Test> tests = new List<Test>();
            switch(number)
            {
                case 1:
                    dateNow = dateNow.AddMonths(-1);
                    tests = await _context.Tests.Where(l => l.DateOfTest >= dateNow).ToListAsync();
                    break;
                case 6:
                    dateNow = dateNow.AddMonths(-6);
                    tests = await _context.Tests.Where(l => l.DateOfTest >= dateNow).ToListAsync();
                    break;
                case 12:
                    dateNow = dateNow.AddMonths(-12);
                    tests = await _context.Tests.Where(l => l.DateOfTest >= dateNow).ToListAsync();
                    break;
            }
            return Json(tests);
        }
       
    }
}