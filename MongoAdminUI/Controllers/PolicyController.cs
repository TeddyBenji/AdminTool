using Microsoft.AspNetCore.Mvc;
using MongoAdminUI.Services;
using System.Threading.Tasks;
using MongoAdminUI.Models.PolicyModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MongoAdminUI.Controllers
{
    [Authorize(Policy = "SecurityAdminAccess")]
    public class PolicyController : Controller
    {
        private readonly PolicyService _policyService;
        private readonly RoleService _roleService;

        // Dependency injection of PolicyService
        public PolicyController(PolicyService policyService, RoleService roleService)
        {
            _policyService = policyService;
            _roleService = roleService;
        }

        // Display list of policies
        public async Task<IActionResult> Index()
        {
            var policies = await _policyService.GetPoliciesAsync();
            return View(policies);
        }

        // Display policy for editing
        public async Task<IActionResult> GetPolicy(string policyName)
        {
            var policy = await _policyService.GetPolicyByNameAsync(policyName);
            if (policy == null)
            {
                return NotFound();
            }
            return View(policy);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string policyName)
        {
            // Fetch policy and all roles
            var policy = await _policyService.GetPolicyByNameAsync(policyName);
            var roles = await _roleService.GetRolesAsync();

            if (policy == null)
            {
                return NotFound("Policy not found.");
            }

            ViewBag.Roles = new SelectList(roles, "Name", "Name", policy.Roles);
            return View(policy);
        }



        // Handle policy update
        [HttpPost]
        [Authorize(Policy = "SecurityAdminAccess")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PolicyModel policyModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the current policy to get existing roles
                    var currentPolicy = await _policyService.GetPolicyByNameAsync(policyModel.Name);
                    if (currentPolicy == null)
                    {
                        throw new InvalidOperationException("Policy not found.");
                    }

                    // Combine existing roles with new roles
                    var updatedRolesList = currentPolicy.Roles.Union(policyModel.Roles).Distinct().ToList();

                    // Update the policy with combined roles
                    await _policyService.UpdatePolicyAsync(policyModel.Name, updatedRolesList);

                    
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Repopulate the Roles SelectList for the view if ModelState is not valid or an exception occurred
            var allRoles = await _roleService.GetRolesAsync();
            ViewBag.Roles = new SelectList(allRoles, "Name", "Name", policyModel.Roles);

            return View(policyModel);
        }








        // Display policy for creation
        public async Task<IActionResult> Create()
        {
            var allRoles = await _roleService.GetRolesAsync();
            ViewBag.Roles = new SelectList(allRoles, "Name", "Name");
            return View();
        }


        // Handle new policy creation
        [HttpPost]
        [Authorize(Policy = "SecurityAdminAccess")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PolicyModel policyModel)
        {
            if (ModelState.IsValid)
            {
                await _policyService.AddPolicyAsync(policyModel);
                return RedirectToAction("Index");
            }

            
            var allRoles = await _roleService.GetRolesAsync();
            ViewBag.Roles = new SelectList(allRoles, "Name", "Name");

            return View(policyModel);
        }




        // Handle policy deletion
        [HttpPost]
        [Authorize(Policy = "SecurityAdminAccess")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string policyName)
        {
            await _policyService.DeletePolicyAsync(policyName);
            return RedirectToAction("Index");
        }
    }
}

