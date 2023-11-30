using Microsoft.AspNetCore.Mvc;
using MongoAdminUI.Services;
using MongoAdminUI.Models;
using System.Threading.Tasks;

namespace MongoAdminUI.Controllers
{
    public class RoleController : Controller
    {
        private readonly RoleService _roleService;

        // Dependency injection of RoleService
        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        // Display list of roles
        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetRolesAsync();
            return View(roles);
        }

        // Display role for editing
        public async Task<IActionResult> Edit(string roleName)
        {
            var role = await _roleService.GetRoleByNameAsync(roleName);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // Handle role update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleModel roleModel)
        {
            if (ModelState.IsValid)
            {
                await _roleService.UpdateRoleAsync(roleModel.Name, roleModel);
                return RedirectToAction("Index");
            }
            return View(roleModel);
        }

        // Display role for creation
        public IActionResult Create()
        {
            return View();
        }

        // Handle new role creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleModel roleModel)
        {
            if (ModelState.IsValid)
            {
                await _roleService.AddRoleAsync(roleModel);
                return RedirectToAction("Index");
            }
            return View(roleModel);
        }

        // Handle role deletion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string roleName)
        {
            await _roleService.DeleteRoleAsync(roleName);
            return RedirectToAction("Index");
        }
    }
}

