using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;
using SignalRApp.Models;
using System.Diagnostics;

namespace SignalRApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IServiceManager _manager;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IServiceManager manager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _manager = manager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] UserDTO userDto,IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                if(ProfileImage is not null)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot", "media", ProfileImage.FileName);
                    using(var stream= new FileStream(path, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    userDto.ProfileImage=string.Concat("/media/",ProfileImage.FileName);
                }
            }
            var result = await _manager.AuthService.CreateUserAsync(userDto);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code,error.Description);
                }
            }
            return RedirectToAction("Login", new { ReturnUrl = "/" });
        }

        [HttpGet]
        public async Task<IActionResult> Login([FromQuery(Name = "ReturnUrl")] string ReturnUrl = "/")
        {
            return View(new LoginModel()
            {
                ReturnUrl = ReturnUrl,
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.UserName);
            if(user is null)
            {
                ModelState.AddModelError(string.Empty, "There is no user with this UserName");
            }
            await _signInManager.SignOutAsync();
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Wrong password.");
            }
            user.IsOnline = true;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index", "Home", new { userName = user.UserName });
        }

        public async Task<IActionResult> Logout()
        {
            var user = _signInManager.Context.User.Identity.Name;
            var logoutUser= await _userManager.FindByNameAsync(user);
            await _signInManager.SignOutAsync();
            logoutUser.ConnectionId =null;
            logoutUser.LastLogin = DateTime.Now;
            logoutUser.IsOnline = false;
            await _userManager.UpdateAsync(logoutUser);
            return Redirect("/");
        }
    }
}
