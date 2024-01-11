using Microsoft.AspNetCore.Mvc;
using MongoAdminUI.Services;
using System.Threading.Tasks;
using MongoDB.Bson.IO;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using MongoAdminUI.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoAdminUI.Models.UserModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MongoAdminUI.Controllers
{
    [Authorize(Policy = "SecurityAdminAccess")]
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly RoleService _roleService;


        public UserController(UserService userService, RoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        // Display list of users
        public async Task<IActionResult> Index()
        {
            var userViewModels = await _userService.GetUserViewModelsAsync();
            return View(userViewModels);
        }


        // Display user for editing
        [HttpGet]
        public async Task<IActionResult> Edit(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);

            if (user == null)
            {
                return NotFound();
            }

            var updateUserModel = new UpdateUser
            {
                Name = user.Name,
                Email = user.Email,
                Username = user.UserName
                // Map other necessary properties if there are any
            };

            return View(updateUserModel);
        }


        [HttpPost]
        [Authorize(Policy = "SecurityAdminAccess")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUser updateUser)
        {
            if (ModelState.IsValid)
            {
                var token = HttpContext.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Index", "Login");
                }

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var response = await httpClient.PutAsJsonAsync("https://data-platform-test.chemometec.com/Identity/api/User/update/user", updateUser);
                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            var errorResponse = await response.Content.ReadAsStringAsync();
                            ModelState.AddModelError("", $"Error updating user: {errorResponse}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception if needed
                    ModelState.AddModelError("", $"Error updating user: {ex.Message}");
                }
            }

            return View(updateUser);
        }



        // Handle user deletion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string username)
        {
            await _userService.DeleteUserAsync(username);
            return RedirectToAction("Index");
        }

            
        [HttpGet]
        [Authorize(Policy = "SecurityAdminAccess")]
        public IActionResult CreateUser()
        {
        return View(new CreateUserModel()); 
        }

            
        [HttpPost]
        [Authorize(Policy = "SecurityAdminAccess")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserModel model)
        {
        if (ModelState.IsValid)
        {
        var token = HttpContext.Session.GetString("AccessToken");
        if (string.IsNullOrEmpty(token))
        {
        return RedirectToAction("Index", "Login");
        }

        using (var httpClient = new HttpClient())
        {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.PostAsJsonAsync("https://data-platform-test.chemometec.com/Identity/api/user/creating/new/user", model);
        if (response.IsSuccessStatusCode)
        {                   
        return RedirectToAction("UserCreated"); 
        }
        else
        {
        var errorResponse = await response.Content.ReadAsStringAsync();
                           
        ModelState.AddModelError(string.Empty, $"User creation failed: {errorResponse}");
        }
        }
        }

                
        return View(model);
        }

            
        [HttpGet]
        public IActionResult UserCreated()
        {
        
        return View();
        }

        [HttpGet]
        [Authorize(Policy = "SecurityAdminAccess")]
        public async Task<IActionResult> AssignRole()
        {
            var roles = await _roleService.GetRolesAsync();
            ViewBag.Roles = new SelectList(roles, "Name", "Name"); 

            return View(new AssignUserRole());
        }


        [HttpPost]
        [Authorize(Policy = "SecurityAdminAccess")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(AssignUserRole model)
        {
            if (ModelState.IsValid)
            {
                var token = HttpContext.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Index", "Login");
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await httpClient.PostAsJsonAsync("https://data-platform-test.chemometec.com/Identity/api/userroles/assign-role", model);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "User"); 
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Role assignment failed: {errorResponse}");
                    }
                }
                
                return View(model);
            }
            
            return View(model);
        }
        /////////////////////////////////////////////////////////////////////////////////////////////

        [HttpGet]
        [Authorize(Policy = "SecurityAdminAccess")]
        public async Task<IActionResult> UnAssignRole(string username)
        {
            var userRoles = await _userService.GetUserRolesAsync(username); 
            ViewBag.Roles = new SelectList(userRoles);

            var model = new AssignUserRole
            {
                UserName = username 
            };

            return View(model);
        }




        [HttpPost]
        [Authorize(Policy = "SecurityAdminAccess")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnAssignRole(AssignUserRole model)
        {
            if (ModelState.IsValid)
            {
                var token = HttpContext.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Index", "Login");
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await httpClient.PostAsJsonAsync("https://data-platform-test.chemometec.com/Identity/api/userroles/remove-role", model);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "User"); 
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Role assignment failed: {errorResponse}");
                    }
                }
                
                return View(model);
            }
            
            return View(model);
        }

    }

}

