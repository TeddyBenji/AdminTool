using Microsoft.AspNetCore.Mvc;
using MongoAdminUI.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace MongoAdminUI.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger; // Initialize the field
        }
        public IActionResult Index()
        {
            return View(new LoginModel()); // Render the login view with an empty model
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string token = await GetUserToken(model.Username, model.Password);

                    // Store the token in session
                    HttpContext.Session.SetString("AccessToken", token);

                    // Redirect to the 'Index' action of 'UserController'
                    return RedirectToAction("Index", "User");
                }
                catch (InvalidOperationException ex)
                {
                    // Log the error message
                    _logger.LogError(ex, "Error occurred while retrieving access token.");

                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(model); // Return with errors if ModelState is invalid
        }

        private async Task<string> GetUserToken(string username, string password)
        {
            using (var client = new HttpClient())
            {
                var tokenEndpoint = "https://localhost:7042/connect/token";
                var clientId = "adminui-client-id";
                var clientSecret = "adminui-client-secret";

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("scope", "read write delete admin")
                });

                var response = await client.PostAsync(tokenEndpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                    return responseObject.access_token;
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    // Log the error response for debugging
                    _logger.LogError($"Token request failed with response: {errorResponse}");

                    throw new InvalidOperationException("Unable to retrieve access token from IdentityServer.");
                }
            }
        }
    }
}

