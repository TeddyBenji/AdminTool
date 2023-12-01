using Microsoft.AspNetCore.Mvc;
using MongoAdminUI.Services;
using MongoAdminUI.Models;
using System.Threading.Tasks;

namespace MongoAdminUI.Controllers
{
    public class PolicyController : Controller
    {
        private readonly PolicyService _policyService;

        // Dependency injection of PolicyService
        public PolicyController(PolicyService policyService)
        {
            _policyService = policyService;
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

        // Handle policy update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PolicyModel policyModel)
        {
            if (ModelState.IsValid)
            {
                await _policyService.UpdatePolicyAsync(policyModel.Name, policyModel);
                return RedirectToAction("Index");
            }
            return View(policyModel);
        }

        // Display policy for creation
        public IActionResult Create()
        {
            return View();
        }

        // Handle new policy creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PolicyModel policyModel)
        {
            if (ModelState.IsValid)
            {
                await _policyService.AddPolicyAsync(policyModel);
                return RedirectToAction("Index");
            }
            return View(policyModel);
        }

        // Handle policy deletion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string policyName)
        {
            await _policyService.DeletePolicyAsync(policyName);
            return RedirectToAction("Index");
        }
    }
}

