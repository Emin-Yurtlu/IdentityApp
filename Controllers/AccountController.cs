using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(
            UserManager<AppUser> userManager, 
            RoleManager<AppRole> roleManager, 
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager= signInManager;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await _signInManager.SignOutAsync();

                    if(await _userManager.IsEmailConfirmedAsync(user)){ 
                        
                        ModelState.AddModelError("", "Lütfen email adresinizi doğrulayınız.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true );

                    if(result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, null);
                        return RedirectToAction("Index", "Home");
                    }
                    else if (result.IsLockedOut)
                    {
                        ModelState.AddModelError("", "Hesabınız bir süreliğine kilitlenmiştir. Lütfen daha sonra tekrar deneyiniz.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Hatalı emil ya da parola.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Hatalı emil ya da parola.");
                }
                   
                
               
            }
            return View(model);

        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);


                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);   
                    var url= Url.Action("ConfirmEmail", "Account", new { user.Id, token });
                    TempData["Success"] = "Kayıt başarılı! Lütfen email adresinize gönderilen doğrulama linkine tıklayarak hesabınızı doğrulayınız.";
                    return RedirectToAction("Login","Account");
                }

                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
                return View(model);
            }

            return View(model);

        }
        public async Task<IActionResult> ConfirmEmail(string Id, string token) 
        {
            if(Id == null || token == null)
            {
                TempData["Error"] = "Geçersiz token veya kullanıcı ID'si.";
                return View();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Email adresiniz başarıyla doğrulandı.";
                    return View();
                }
                else
                {
                    TempData["Error"] = "Email doğrulama başarısız oldu.";
                    return View();
                }
            }
            TempData["Error"] = "Kullanıcı bulunamadı.";
            return View();
        }
    }
}