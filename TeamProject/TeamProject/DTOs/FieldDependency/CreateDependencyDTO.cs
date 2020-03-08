﻿using FormGenerator.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamProject.Infrastructure.Enums;
using TeamProject.Models.FieldFieldDependencyModels;

namespace TeamProject.DTOs.FieldDependency
{
    public class CreateDependencyDTO
    {
        public string SuperiorFieldName { get; set; }
        public string DependencyType { get; set; }
        public string ActivationValue { get; set; }
        public List<Field> RelatedFields { get; set; } = new List<Field>();
        public string CurrentFieldName { get; set; }
        public string CurrentFieldType { get; set; }
        public string AllIndependentFieldsName { get; set; }

        public void UpdateIndependentFieldsList(IFieldDependenciesRepository repository, FormGeneratorContext context)
        {
            var allDependFields = repository.GetAllDependFields();
            var allIndependedFields = context.Field.AsNoTracking()
                .ToList()
                .Where(f=> {
                    return (!allDependFields.Contains(f)) && (f.Name!=SuperiorFieldName) 
                        && (!RelatedFields.Contains(f));
                });

            AllIndependentFieldsName = JsonConvert.SerializeObject(allIndependedFields.Select(f => f.Name).ToList());
        }

        public void AddRelatedField(Field field)
        {
            if (!RelatedFields.Contains(field))
            {
                RelatedFields.Add(field);
            }
        }

        public string Valid(FormGeneratorContext _context)
        {
            if(DependencyType=="FieldDuplication" && !int.TryParse(ActivationValue,out _))
            {
                return "Maksymalna wartość musi być liczbą! Popraw błędy";
            }

            if (RelatedFields.FirstOrDefault(f => f.Name == SuperiorFieldName) != null)
            {
                return "Pole nadrzędne nie może być polem podrzędnym w jednej relacji!";
            }
            if (_context.Field.AsNoTracking().FirstOrDefault(f => f.Name == this.CurrentFieldName) == null
                && this.DependencyType== "FieldVisibly")
            {
                return "Wpisane pole zależne nie istnieje w systemie!";
            }
            if (_context.Field.AsNoTracking().FirstOrDefault(f => f.Name == this.SuperiorFieldName) == null)
            {
                return "Wpisane pole nadrzędne nie istnieje";
            }
            if(this.RelatedFields?.Count!=0 && this.DependencyType== "FieldDuplication")
            {
                return "Niezany błąd. Spróbuj jeszcze raz";
            }
            //nie można edytować zależności ilościowej
            string test= FieldFieldDependencyType.FieldDuplication.ToString();
            var dependencyExist = _context.Dependencies
                .AsNoTracking()
                .FirstOrDefault(dep => (dep.DependencyType.ToString() == this.DependencyType)
                    && (dep.ThisField.Name == this.CurrentFieldName));
            if (dependencyExist != null)
            {
                return "Zależność ilościowa już została zdefiniowana na tym polu.";
            }

            if (_context.Field.FirstOrDefault(f => f.Name == this.SuperiorFieldName).Type != "number")
            {
                return "Tylko pole typu 'number' może być polem nadrzędnym w relacji ilościowej";
            }
            return null;
        }
    }
}
