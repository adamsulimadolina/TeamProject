using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using FormGenerator.Models;
using TeamProject.Models;
using Microsoft.EntityFrameworkCore;
using TeamProject.Data;
using TeamProject.Models.FieldFieldDependencyModels;
using TeamProject.Models.FieldDependencyModels;
using TeamProject.Infrastructure.Enums;

namespace TeamProject
{
    public class Seed
    {
        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Admin";
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }
            if (!roleManager.RoleExistsAsync("Technik").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Technik";
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }
        }
        public static void SeedUsers(UserManager<MyUser> userManager)
        {
            if (userManager.FindByNameAsync("admin@admin.pl").Result == null)
            {
                MyUser user = new MyUser();
                user.UserName = "admin@admin.pl";
                user.Email = "admin@admin.pl";
                user.FirstName = "admin";
                user.LastName = "admin";

                string UserPassword = "Admin.1";
                IdentityResult result = userManager.CreateAsync
                (user, UserPassword).Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }
            }

        }

        public static void SeedDependencies(IFieldDependenciesRepository repository, FormGeneratorContext context)
        {
            Field korzen = context.Field.FirstOrDefault(f => f.Id == 1);
            var fields = context.Field.Where(f=>f.Id!=1).ToList();
            FieldFieldDependency dep1 = new FieldFieldDependency(korzen, FieldFieldDependencyType.FieldDuplication, "22");
            fields.ForEach(f => dep1.RelatedFields.Add(f));
            var test = repository.GetAllDependFields();
            repository.SaveDependency(dep1);
        }
    }
}

