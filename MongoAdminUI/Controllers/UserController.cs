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

                        var response = await httpClient.PutAsJsonAsync("https://localhost:7042/api/User/update/user", updateUser);
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

            // This action displays the form for creating a new user
        [HttpGet]
        [Authorize(Policy = "SecurityAdminAccess")]
        public IActionResult CreateUser()
        {
        return View(new CreateUserModel()); // Returns the view with an empty model
        }

            // This action processes the form submission
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

        var response = await httpClient.PostAsJsonAsync("https://localhost:7042/api/user/creating/new/user", model);
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

                // If model state is not valid or user creation failed, return to the view with the model and error messages
        return View(model);
        }

            // This action shows a confirmation of user creation
        [HttpGet]
        public IActionResult UserCreated()
        {
        // Implement this method to display a confirmation message or the details of the created user
        return View();
        }

        [HttpGet]
        [Authorize(Policy = "SecurityAdminAccess")]
        public async Task<IActionResult> AssignRole()
        {
            var roles = await _roleService.GetRolesAsync();
            ViewBag.Roles = new SelectList(roles, "Name", "Name"); // Adjust the properties if necessary

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
                    var response = await httpClient.PostAsJsonAsync("https://localhost:7042/api/userroles/assign-role", model);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "User"); // Redirects back to the User List view
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Role assignment failed: {errorResponse}");
                    }
                }
                // After the using block, this code path needs to return a value.
                return View(model);
            }
            // If ModelState is not valid, return the view with the model to show validation errors
            return View(model);
        }
        /////////////////////////////////////////////////////////////////////////////////////////////

        [HttpGet]
        [Authorize(Policy = "SecurityAdminAccess")]
        public async Task<IActionResult> UnAssignRole(string username)
        {
            var userRoles = await _userService.GetUserRolesAsync(username); // Assuming this is your new method
            ViewBag.Roles = new SelectList(userRoles);

            var model = new AssignUserRole
            {
                UserName = username // Preset the username in the model
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
                    var response = await httpClient.PostAsJsonAsync("https://localhost:7042/api/userroles/remove-role", model);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "User"); // Redirects back to the User List view
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Role assignment failed: {errorResponse}");
                    }
                }
                // After the using block, this code path needs to return a value.
                return View(model);
            }
            // If ModelState is not valid, return the view with the model to show validation errors
            return View(model);
        }

    }

}

