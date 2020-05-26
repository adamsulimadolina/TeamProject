using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeamProject.Models;

namespace TeamProject.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Admin")]
    public class ManageUserDataModel : PageModel
    {
        private readonly UserManager<MyUser> _userManager;
        private readonly SignInManager<MyUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public ManageUserDataModel(
            UserManager<MyUser> userManager,
            SignInManager<MyUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required, DataType(DataType.Text), Display(Name = "Typ")]
            public string Role { get; set; }
            [Required, DataType(DataType.Text), Display(Name = "Imiê")]
            public string FirstName { get; set; }

            [Required, DataType(DataType.Text), Display(Name = "Nazwisko")]
            public string LastName { get; set; }
            [Display(Name = "ID Technika")]
            public int UserID { get; set; }
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
            public string ID { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var usercurrent = await _userManager.GetUserAsync(User);
            if (usercurrent == null)
            {
                return NotFound($"Wyst¹pi³ nieoczekiwany problem '{_userManager.GetUserId(User)}'.");
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Problem ze znalezieniem u¿ytkownika '{_userManager.GetUserId(User)}'.");
            }
            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var role = await _userManager.GetRolesAsync(user);
            Username = userName;

            Input = new InputModel
            {
                Email = email,
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserID = user.CustomID,
                Role = role.FirstOrDefault(),
                ID = id
            };
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usercurrent = await _userManager.GetUserAsync(User);
            if (usercurrent == null)
            {
                return NotFound($"Wyst¹pi³ nieoczekiwany problem '{_userManager.GetUserId(User)}'.");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByIdAsync(Input.ID);
            if (user == null)
            {
                return NotFound($"Problem ze znalezieniem u¿ytkownika '{_userManager.GetUserId(User)}'.");
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting email for user with ID '{userId}'.");
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
                }
            }
            if (Input.FirstName != user.FirstName) user.FirstName = Input.FirstName;
            if (Input.LastName != user.LastName) user.LastName = Input.LastName;
            if (Input.UserID != user.CustomID) user.CustomID = Input.UserID;
            var role = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRoleAsync(user, role[0]);
            await _userManager.AddToRoleAsync(user, Input.Role);
            await _userManager.UpdateAsync(user);
            StatusMessage = "Zaktualizowano profil";
            return RedirectToAction("Users", "AdminPanel", new { message = "U¿ytkownik zosta³ zaktualizowany" });
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }


            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return RedirectToAction("Users", "AdminPanel", new { message = "E-Mail zosta³ wys³any" });
        }
    }
}

