using EventBooking.Models;
using EventBooking.ViewModels;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventBooking.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // ------------------------
        // Register
        // ------------------------
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,  // <-- take from user input
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                    ? "0000000000"   // 👈 default value if left blank
                    : model.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // Assign to "User" role (matches your system's role structure)
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["SuccessMessage"] = "Registration successful! Welcome to EventBooking.";
            return RedirectToAction("Index", "Home");
        }

        // ------------------------
        // Login
        // ------------------------
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!User.IsInRole("Admin")) // ✅ Terms check for customers only
            {
                if (!model.AgreeTerms)
                {
                    ModelState.AddModelError("AgreeTerms", "You must agree to the Terms & Conditions.");
                }
            }
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // Check user role and redirect accordingly
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin") || roles.Contains("Staff"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        // ------------------------
        // Logout
        // ------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        // ------------------------
        // Google Login
        // ------------------------
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "Error logging in with Google.";
                return RedirectToAction("Login");
            }

            var email = info.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = info.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Could not get email information from Google.";
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                string firstName = name;
                string lastName = string.Empty;

                if (!string.IsNullOrEmpty(name) && name.Contains(' '))
                {
                    var nameParts = name.Split(' ', 2);
                    firstName = nameParts[0];
                    lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
                }

                user = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                       Address = "Not provided" // <-- default value
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return RedirectToAction("Login");
                }

                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole("User"));

                await _userManager.AddToRoleAsync(user, "User");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["SuccessMessage"] = "Logged in successfully with Google!";
            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}