﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FormGenerator.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamProject.Models.FormGeneratorModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using TeamProject.Models;

namespace TeamProject.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FieldAPIController : Controller
    {
        private readonly FormGeneratorContext _context;
        private readonly UserManager<MyUser> _userManager;

        public FieldAPIController(FormGeneratorContext context, UserManager<MyUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/FieldAPI
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Field>>> GetField()
        {
            var fields = await _context.Field
                .Where(f => f.Name != null)
                .ToListAsync();
            return fields;
        }
        public async Task<MyUser> GetUser()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }
        [HttpPost]       
        public async Task<ActionResult> addNewLog(Logs log)
        {
            var user = await GetUser();       
            log.date = DateTime.Now;
            log.UserID = user.CustomID;                      
            var oldlog = await _context.Logs.Where(l => l.FieldID == log.FieldID && l.FormID == log.FormID && l.AnswerValue == log.AnswerValue).LastOrDefaultAsync();
            if (oldlog != null)
            {
                TimeSpan timeSpan = log.date - oldlog.date;
                if (timeSpan.TotalMinutes < 5) return Json("log already exists");
            }
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
            return Json(log);
        }
    }
}