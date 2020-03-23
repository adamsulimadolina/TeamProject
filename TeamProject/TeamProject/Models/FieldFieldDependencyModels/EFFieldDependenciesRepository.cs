using FormGenerator.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamProject.Models.FieldDependencyModels;

namespace TeamProject.Models.FieldFieldDependencyModels
{
    public class EFFieldDependenciesRepository : IFieldDependenciesRepository
    {
        private FormGeneratorContext _context;

        public EFFieldDependenciesRepository(FormGeneratorContext ctx)
        {
            _context = ctx;
        }

        public IQueryable<FieldFieldDependency> Dependencies => _context.Dependencies.Include(dep => dep.RelatedFields).Include(dep => dep.ThisField);

        public void SaveDependency(FieldFieldDependency dependency)
        {
            var fieldsToCreate = dependency.RelatedFields.Where(f => f.Id <= 0).ToList(); //jeśli jakieś pola zależne nie istnieją->tworzymy je
            fieldsToCreate.ForEach(currentField => _context.Field.Add(currentField));
            _context.SaveChanges();
            //bład-> pobranie na nowo wszystkich pól zależnych z contekstu(różne żądania, EF gubi tracking)
            var idsAllRelatedFields = dependency.RelatedFields.Select(f => f.Id).ToList();
            dependency.RelatedFields = _context.Field.Where(f => idsAllRelatedFields.Contains(f.Id)).ToList();
            dependency.ThisField = _context.Field.FirstOrDefault(f => f.Id == dependency.Id);
            //bład-> pobranie na nowo wszystkich pól zależnych z contekstu
            //czy zależność już istnieje
            FieldFieldDependency existingDependency = Dependencies
                .FirstOrDefault(dep => (dep.Id == dependency.Id) 
                && (dep.ActivationValue == dependency.ActivationValue)
                && (dep.DependencyType==dependency.DependencyType));
            if (existingDependency!=null) //czyli istnieje
            {
                existingDependency.RelatedFields.AddRange(dependency.RelatedFields);
                _context.Dependencies.Update(existingDependency);
            }
            else
            {
                _context.Dependencies.Add(dependency);
            }
            _context.SaveChanges();
        }
        
        public IEnumerable<Field> GetAllDependFields()
        {
            List<Field> result = new List<Field>();
            Dependencies.ToList().ForEach(dep =>
            {
                dep.RelatedFields.ForEach(field => result.Add(field));
            });
            return result;
        }
    }
}
