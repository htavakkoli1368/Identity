using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using security.Models;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace security.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IEmailSender emailSender;

        public AccountController(UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager, IEmailSender emailSender,RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.roleManager = roleManager;
        }       

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous] 
        public async Task<IActionResult> Register(string returnurl=null)
        {
          
            if(! await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                await roleManager.CreateAsync(new IdentityRole("Operator"));
            }
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem()
            {
                Value = "Admin",
                Text = "Admin",
            }                
            );
            listItems.Add(new SelectListItem()
            {
                Value = "Operator",
                Text = "Operator",
            }                
            );
            ViewData["Returnurl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            RegisterViewModel registerViewModel = new RegisterViewModel()
            {
                RoleList = listItems
            };
            
            return View(registerViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model,string? returnurl )
        {
            ViewData["Returnurl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser() { UserName=model.Email, Email = model.Email,Name=model.Name };
                var result = await userManager.CreateAsync(user,model.Password);
                if(result.Succeeded)
                { 
                    if(model.RoleSelected != null && model.RoleSelected.Length>0 && model.RoleSelected=="Admin")
                    {
                        await userManager.AddToRoleAsync(user,"Admin");
                    }
                    else
                    {
                        await userManager.AddToRoleAsync(user, "Operator");

                    }
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnurl);
                }
                AddErrors(result);
                
            }
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem()
            {
                Value = "Admin",
                Text = "Admin",
            }
            );
            listItems.Add(new SelectListItem()
            {
                Value = "Operator",
                Text = "Operator",
            }
            );
            model.RoleList = listItems;
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required");

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return Ok(); // برای امنیت، لو نده کاربر وجود داره یا نه

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { token, email = user.Email },
                Request.Scheme
            );

            await emailSender.SendEmailAsync(
                user.Email,
                "Reset Password",
                $"برای تغییر پسورد روی لینک زیر کلیک کنید:\n{resetLink}"
            );

            return Ok("Reset password link sent.");

        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var result = await userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.NewPassword
            );

            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {             
           await signInManager.SignOutAsync();
           return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnurl=null)
        {
            ViewData["Returnurl"] = returnurl;

            var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
            ViewBag.ExternalLogins = schemes.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                new { returnUrl });

            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                provider, redirectUrl);

            return Challenge(properties, provider);
        }
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ModelState.AddModelError("", $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            var result = await signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (result.Succeeded)
                return LocalRedirect(returnUrl);

            // user does not exist yet → create it
            var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var createResult = await userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                AddErrors(createResult);
                return RedirectToAction(nameof(Login));
            }

            await userManager.AddLoginAsync(user, info);
            await signInManager.SignInAsync(user, isPersistent: false);

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model,string returnurl=null)
        {
            ViewData["Returnurl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email,model.Password,model.RememberMe, lockoutOnFailure:true);
                if (result.IsLockedOut)
                {
                    return View("LockOut");
                }
                if (result.Succeeded)
                {
                    return LocalRedirect(returnurl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty,"invalid login attempt");
                     return View(model);
                }
            }
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
