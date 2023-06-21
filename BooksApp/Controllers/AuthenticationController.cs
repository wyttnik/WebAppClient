using BooksApp.Models;
using BooksApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BooksApp.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly HttpClient _client;
        private readonly ITokenReader _reader;

        [BindProperty]
        public LogPass? LogPass { get; set; }

        public AuthenticationController(ILogger<AuthenticationController> logger, HttpClient client, ITokenReader reader)
        {
            _logger = logger;
            _client = client;
            _reader = reader;
        }

        public  IActionResult SignIn(string errorMessage = "")
        {
            ViewData["Login"] = ((TokenReader)_reader).GetName();
            ViewData["ErrorMessage"] = errorMessage;
            return View();
        }

        public IActionResult SignUp(string successMessage = "", string errorMessage = "")
        {
            ViewData["SuccessMessage"] = successMessage;
            ViewData["ErrorMessage"] = errorMessage;
            return View();
        }


        public IActionResult OnSignOut()
        {
            ((TokenReader)_reader).SetName();
            ((TokenReader)_reader).SetToken();

            return RedirectToAction("SignIn", "Authentication");
        }

        public async Task<IActionResult> OnSignInAsync()
        {
            //var request = Request.Form;
            var apiUrl = "https://localhost:7159/api/token";
            var data = new LogPass()
            {
                login = LogPass.login,
                password = LogPass.password
            };
            var response = await _client.PostAsJsonAsync(apiUrl, data);
            if (response.IsSuccessStatusCode)
            {
                ((TokenReader)_reader).SetToken(await response.Content.ReadAsStringAsync());
                ((TokenReader)_reader).SetName(data.login);
                return RedirectToAction("Index", "Home");
            }

            var message = await response.Content.ReadAsStringAsync();
            return RedirectToAction("SignIn", "Authentication", new { errorMessage = message });
        }

        public async Task<IActionResult> OnSignUpAsync()
        {
            var apiUrl = "https://localhost:7159/api/Users";
            var data = new User()
            {
                Login = LogPass.login,
                Password = LogPass.password,
                Role = "user"
            };
            var response = await _client.PostAsJsonAsync(apiUrl, data);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("SignUp", "Authentication", new { successMessage = "Your account was succesfully created" });
            }

            var message = await response.Content.ReadAsStringAsync();
            return RedirectToAction("SignUp", "Authentication", new { errorMessage = message });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}