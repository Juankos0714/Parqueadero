using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar el usuario por email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(model);
            }

            // Verificar si el usuario tiene el rol seleccionado
            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains(model.TipoUsuario))
            {
                ModelState.AddModelError("TipoUsuario", "No tienes permisos para acceder como " + model.TipoUsuario);
                return View(model);
            }

            // Intentar hacer login
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
            
            if (result.Succeeded)
            {
                // Redirigir según el rol
                if (model.TipoUsuario == "Funcionario")
                {
                    return RedirectToAction("Dashboard", "Funcionario");
                }
                else
                {
                    return RedirectToAction("Dashboard", "Aprendiz");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar si el email ya está registrado
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Este email ya está registrado en el sistema.");
                return View(model);
            }

            // Crear el usuario
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                // Asignar el rol seleccionado
                await _userManager.AddToRoleAsync(user, model.TipoUsuario);

                // Hacer login automáticamente después del registro
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Redirigir según el rol
                if (model.TipoUsuario == "Funcionario")
                {
                    return RedirectToAction("Dashboard", "Funcionario");
                }
                else
                {
                    return RedirectToAction("Dashboard", "Aprendiz");
                }
            }
            else
            {
                // Agregar errores al modelo
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                        ModelState.AddModelError("Email", "Este email ya está en uso.");
                    }
                    else if (error.Code == "PasswordTooShort")
                    {
                        ModelState.AddModelError("Password", "La contraseña debe tener al menos 6 caracteres.");
                    }
                    else if (error.Code == "PasswordRequiresDigit")
                    {
                        ModelState.AddModelError("Password", "La contraseña debe contener al menos un número.");
                    }
                    else if (error.Code == "PasswordRequiresUpper")
                    {
                        ModelState.AddModelError("Password", "La contraseña debe contener al menos una letra mayúscula.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Code} - {error.Description}");
                }

                return View(model);
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
