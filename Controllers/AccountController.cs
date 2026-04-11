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
        private IEmailSender _emailSender;

        public AccountController(
            UserManager<AppUser> userManager, 
            RoleManager<AppRole> roleManager, 
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager= signInManager;
            _emailSender = emailSender;
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
                    // 1. ADIM: Önce Email Doğrulama Kontrolü
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "Lütfen email adresinizi doğrulayınız.");
                        return View(model); // Burada hemen dönerek diğer kontrolleri atlıyoruz.
                    }

                    // 2. ADIM: Giriş İşlemi
                    // Önceki oturumları temizle
                    await _signInManager.SignOutAsync();

                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        return RedirectToAction("Index", "Home");
                    }

                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError("", "Hesabınız kilitlendi.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Hatalı email ya da parola.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Hatalı email ya da parola.");
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


                    await _emailSender.SendEmailAsync(user.Email, "Hesap Onayı", $"Lütfen email hesabınızı onaylamak ıcın linke <a href='https://localhost:7244{url}'>tıklayınız.</a>");

                    TempData["message"] = "Kayıt başarılı! Lütfen email adresinize gönderilen doğrulama linkine tıklayarak hesabınızı doğrulayınız.";
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
                TempData["message"] = "Geçersiz token veya kullanıcı ID'si.";
                return View();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    TempData["message"] = "Email adresiniz başarıyla doğrulandı.";
                    return View();
                }
               
            }
            TempData["message"] = "Kullanıcı bulunamadı.";
            return View();
        }
    }
}