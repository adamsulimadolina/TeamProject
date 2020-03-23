using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FormGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamProject.DTOs.FieldDependency;
using TeamProject.ExtensionMethods;
using TeamProject.Models.FieldDependencyModels;
using TeamProject.Models.FieldFieldDependencyModels;

namespace TeamProject.Controllers
{
    public class FieldDependencyController : Controller
    {
        private readonly IFieldDependenciesRepository _fieldDependenciesRepo;
        private readonly FormGeneratorContext _context;
        private readonly IMapper _mapper;

        public FieldDependencyController(IFieldDependenciesRepository repo, FormGeneratorContext ctx, IMapper mapper)
        {
            _fieldDependenciesRepo = repo;
            _context = ctx;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            CreateDependencyDTO createDependency = new CreateDependencyDTO();
            createDependency.UpdateIndependentFieldsList(_fieldDependenciesRepo,_context);
            return View(createDependency);
        }

        [HttpPost]
        public IActionResult AddFieldToRelatedListPOST(CreateDependencyDTO createDependency)
        {
            ViewBag.Error = createDependency.Valid(_context);
            if (ViewBag.Error != null)
            {
                return View("Index",createDependency);
            }
            createDependency.AddRelatedField(_context.Field.AsNoTracking().FirstOrDefault<Field>(f => f.Name == createDependency.CurrentFieldName));
            createDependency.UpdateIndependentFieldsList(_fieldDependenciesRepo, _context);
            TempData.Put<CreateDependencyDTO>("CreateDependencyFromPostToGet", createDependency);
            return RedirectToAction(nameof(AddFieldToRelatedListGet));
        }

        public IActionResult AddFieldToRelatedListGet()
        {
            var createDependency = TempData.Get<CreateDependencyDTO>("CreateDependencyFromPostToGet");
            return View("Index",createDependency);
        }

        [HttpPost]
        public IActionResult CreateDependency(CreateDependencyDTO createDependency)
        {
            ViewBag.Error = createDependency.Valid(_context);
            if (ViewBag.Error != null)
            {
                return View("Index", createDependency);
            }
            var dependency = _mapper.Map<FieldFieldDependency>(createDependency);
            dependency.Build(createDependency,_context);
            _fieldDependenciesRepo.SaveDependency(dependency);
            return RedirectToAction("Index", "Forms");
        }
    }
}