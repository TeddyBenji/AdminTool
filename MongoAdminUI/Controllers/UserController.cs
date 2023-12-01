using Microsoft.AspNetCore.Mvc;
using MongoAdminUI.Services;
using MongoAdminUI.Models;
using System.Threading.Tasks;
using MongoDB.Bson.IO;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using MongoAdminUI.Filters;

namespace MongoAdminUI.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;

        
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // Display list of users
        public async Task<IActionResult> Index()
        {
            var userViewModels = await _userService.GetUserViewModelsAsync();
            return View(userViewModels);
        }


        // Display user for editing
        public async Task<IActionResult> Edit(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // Handle user update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                await _userService.UpdateUserAsync(userModel.UserName, userModel);
                return RedirectToAction("Index");
            }
            return View(userModel);
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
            public IActionResult CreateUser()
            {
                return View(new CreateUserModel()); // Returns the view with an empty model
            }

            // This action processes the form submission
            [HttpPost]
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
                            // Assuming "UserCreated" is an action that shows a success message or the created user details
                            return RedirectToAction("UserCreated"); // Redirect to an action that shows user creation was successful
                        }
                        else
                        {
                            var errorResponse = await response.Content.ReadAsStringAsync();
                            // Log the error response and add it to the ModelState
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
    }

}

