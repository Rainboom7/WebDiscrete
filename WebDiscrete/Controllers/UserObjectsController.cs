using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebDiscrete.Data.Entity;
using WebDiscrete.Models;
using WebDiscrete.Services;

namespace WebDiscrete.Controllers
{
    public class UserObjectsController : Controller
    {
        private readonly ILogger<UserObjectsController> _logger;
        private UserService _userService;
        private const string UsernameClaim = "username";
        private const string IdClaim = "id";

        public UserObjectsController(ILogger<UserObjectsController> logger, UserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                var user = _userService.TryLogin(username, password);
                Authenticate(user);
                return RedirectToAction("GetObjects");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new {description = "Ошибка авторизации"});
            }
        }

        [HttpPost]
        public IActionResult SetRights()
        {
            if (GetCurrentUserId(out var userId, out var actionResult)) return actionResult;
            _userService.SetRights(userId);
            return RedirectToAction("GetObjects");
        }

        [HttpPost]
        public IActionResult SetUserRights(int targetUserId, AccessRightType accessRightType, int objId)
        {
            if (GetCurrentUserId(out var userId, out var actionResult)) return actionResult;
            _userService.SetUserAccessType(userId, objId, targetUserId, accessRightType);
            return RedirectToAction("GetObjectUserAccessTypes", new {objectId = objId});
        }

        [HttpGet]
        public IActionResult GetObjectUserAccessTypes(int objectId)
        {
            if (GetCurrentUserId(out var userId, out var actionResult)) return actionResult;
            var obj = _userService.GetObject(objectId);
            var usersWithAccess = _userService.GetUsersForObject(objectId, userId);
            return View(new GetObjectAccessTypesResponse() {Users = usersWithAccess, ObjectDto = obj});
        }

        public IActionResult GetObjects()
        {
            if (GetCurrentUserId(out var userId, out var actionResult)) return actionResult;
            var objects = _userService.GetObjects(Convert.ToInt32(userId));
            return View(objects);
        }

        private bool GetCurrentUserId(out int userId, out IActionResult actionResult)
        {
            var idString = HttpContext.User.Claims.FirstOrDefault(c => c.Type == IdClaim)?.Value;
            if (idString == null)
            {
                {
                    actionResult = RedirectToAction("Error", new {description = "Вы не авторизованы"});
                    userId = -1;
                    return true;
                }
            }

            userId = Convert.ToInt32(idString);
            actionResult = null;
            return false;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string description = "")
        {
            return View(new ErrorViewModel
                {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Description = description});
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        private async Task Authenticate(UserDto userDto)
        {
            var claims = new List<Claim>
            {
                new Claim(UsernameClaim, userDto.Username),
                new Claim(IdClaim, userDto.Id.ToString()),
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}