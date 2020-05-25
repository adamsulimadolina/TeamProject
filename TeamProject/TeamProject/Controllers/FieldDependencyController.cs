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

        public IActionResult ListOfDependenciesForField (int? ide)
        {
            var dependenciesForField = _fieldDependenciesRepo.Dependencies.Where(x => x.Id == ide)?.ToList();

            if (dependenciesForField.Count == 0)
            {
                TempData["isDependency"] = 0;
                TempData["Field"] = _context.Field.FirstOrDefault(x => x.Id == ide).Name;
               
                

                return RedirectToAction(nameof(Index), new { id = ide });
            }
                

            else
            {
                return View(dependenciesForField);
            }


        }


        //public IActionResult Index()
        //{
        //    CreateDependencyDTO createDependency = new CreateDependencyDTO();
        //    createDependency.UpdateIndependentFieldsList(_fieldDependenciesRepo,_context);
        //    return View(createDependency);
        //}
        public IActionResult Index(int? id, int? idDep)
        {
            CreateDependencyDTO createDependency = new CreateDependencyDTO();
            if (idDep != null)
            {
                var dep = _fieldDependenciesRepo.Dependencies.FirstOrDefault(y => y.IdDependency == idDep);
                createDependency.SuperiorFieldId = id;
                //createDependency.SuperiorFieldName
                createDependency.RelatedFields = dep.RelatedFields;
                createDependency.ActivationValue = dep.ActivationValue;
                createDependency.DependencyType = dep.DependencyType.ToString();
                createDependency.IdDependency = dep.IdDependency;
            }
            else
                createDependency.SuperiorFieldId = id;
            //createDependency.UpdateIndependentFieldsList(_fieldDependenciesRepo, _context);
            return View(createDependency);
        }


        public IActionResult DeleteFromDependency (int? idField, int? idDep)
        {
            var pom = _fieldDependenciesRepo.Dependencies.FirstOrDefault(x => x.IdDependency == idDep);
            pom.RelatedFields = pom.RelatedFields.Where(p => p.Id != idField).ToList();
            _fieldDependenciesRepo.SaveDependency(pom);



            return RedirectToAction(nameof(Index), new {id=pom.Id, idDep=pom.IdDependency });

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
            //createDependency.UpdateIndependentFieldsList(_fieldDependenciesRepo, _context);
            TempData.Put<CreateDependencyDTO>("CreateDependencyFromPostToGet", createDependency);
            return RedirectToAction(nameof(AddFieldToRelatedListGet));
        }

        [HttpPost]
        public IActionResult addTest(CreateDependencyDTO createDependency)
        {
            ViewBag.Error = createDependency.Valid(_context);
            //if (ViewBag.Error != null)
            //{
            //    return View("Index", createDependency);
            //}
            Field field = new Field
            {
                Name = createDependency.CurrentFieldName,
                Type = createDependency.CurrentFieldType
            };
            createDependency.AddRelatedField(field);
            //createDependency.UpdateIndependentFieldsList(_fieldDependenciesRepo, _context);
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
            //ViewBag.Error = createDependency.Valid(_context);
            //if (ViewBag.Error != null)
            //{
            //    return View("Index", createDependency);
            //}
            var dependency = _mapper.Map<FieldFieldDependency>(createDependency);
            dependency.Build(createDependency,_context);
            //foreach(Field f in dependency.RelatedFields)
            //{
            //    if(f.Id == -1)
            //    {
            //        _context.Add(f);
            //        _context.SaveChangesAsync();
            //    }
            //}
            _fieldDependenciesRepo.SaveDependency(dependency);
            return RedirectToAction("Index", "Forms");
        }
    }
}