using Microsoft.AspNetCore.Mvc;
using MongoAdminUI.Services;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using MongoAdminUI.Models.RoleModels;
using Microsoft.AspNetCore.Authorization;

namespace MongoAdminUI.Controllers
{
    [Authorize(Policy = "SecurityAdminAccess")]
    public class RoleController : Controller
    {
        private readonly RoleService _roleService;

        
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
        [Authorize(Policy = "SecurityAdminAccess")]
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
        [Authorize(Policy = "SecurityAdminAccess")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Convert comma-separated claims into a List
                var claimsList = model.Claims?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(c => c.Trim())
                                              .ToList() ?? new List<string>();

                var createRoleModel = new CreateRole
                {
                    Name = model.Name,
                    Description = model.Description,
                    Claims = claimsList
                };

                var token = HttpContext.Session.GetString("AccessToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Index", "Login");
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await httpClient.PostAsJsonAsync("https://data-platform-test.chemometec.com/Identity/api/Role/Create/New/Role", createRoleModel);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Role creation failed: {errorResponse}");
                    }
                }
            }

            return View(model);
        }

        // Handle role deletion
        [HttpPost]
        [Authorize(Policy = "SecurityAdminAccess")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string roleName)
        {
            await _roleService.DeleteRoleAsync(roleName);
            return RedirectToAction("Index");
        }
    }
}

