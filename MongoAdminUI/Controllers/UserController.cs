using Microsoft.AspNetCore.Mvc;
using MongoAdminUI.Services;
using MongoAdminUI.Models;
using System.Threading.Tasks;

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


        [HttpGet]
        public IActionResult Create()
        {
            return View(); // No need to pass a new UserModel if you don't have default values to set
        }

        // POST: User/Create
        // This endpoint processes the submitted form for creating a new user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                // Add logic to handle the creation of the user using the UserService
                await _userService.AddUserAsync(userModel);
                // Redirect to the index action after successful creation
                return RedirectToAction(nameof(Index));
            }
            // If validation fails, show the form again with validation error messages
            return View(userModel);
        }



    }
}
