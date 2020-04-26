using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TeamProject.Models;

namespace TeamProject.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Admin")]
    public class DeleteUserModel : PageModel
    {
        private readonly UserManager<MyUser> _userManager;
        private readonly SignInManager<MyUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeleteUserModel(
            UserManager<MyUser> userManager,
            SignInManager<MyUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            public string ID { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Wyst¹pi³ nieoczekiwany problem '{_userManager.GetUserId(User)}'.");
            }
            var currentUser = await _userManager.FindByIdAsync(id);
            if (currentUser == null)
            {
                return NotFound($"Problem ze znalezieniem u¿ytkownika'{_userManager.GetUserId(User)}'.");
            }
            RequirePassword = await _userManager.HasPasswordAsync(user);
            ViewData["userID"] = id;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Wyst¹pi³ nieoczekiwany problem '{_userManager.GetUserId(User)}'.");
            }
            var currentUser = await _userManager.FindByIdAsync(Input.ID);
            if (currentUser == null)
            {
                return NotFound($"Problem ze znalezieniem u¿ytkownika'{_userManager.GetUserId(User)}'.");
            }
            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Password not correct.");
                    return Page();
                }
            }

            var result = await _userManager.DeleteAsync(currentUser);
            var userId = await _userManager.GetUserIdAsync(currentUser);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleteing user with ID '{userId}'.");
            }
            _logger.LogInformation("User with ID '{UserId}' was deleted .", userId);

            return RedirectToAction("Users", "AdminPanel", new { message = "U¿ytkownik zosta³ usuniêty" });
        }
    }
}
