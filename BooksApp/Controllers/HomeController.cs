using BooksApp.Models;
using BooksApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Net.Http;
using static System.Reflection.Metadata.BlobBuilder;

namespace BooksApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _client;
        private readonly ITokenReader _reader;

        public HomeController(ILogger<HomeController> logger, HttpClient client, ITokenReader reader)
        {
            _logger = logger;
            _client = client;
            _reader = reader;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var apiUrl = "https://localhost:7159/api/Books";
            var response = await _client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode) {
                var stringBooks = await response.Content.ReadAsStringAsync();
                var books = JsonConvert.DeserializeObject<IEnumerable<BookToReceive>>(stringBooks);
                ViewData["Books"] = books;
            }
            ViewData["Login"] = ((TokenReader)_reader).GetName();
            return View();
        }

        public async Task<IActionResult> GetBookDetails(int id)
        {
            var apiUrl = "https://localhost:7159/api/Books/" + id;
            string token = _reader.GetToken();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await _client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var stringBooks = await response.Content.ReadAsStringAsync();
                var book = JsonConvert.DeserializeObject<BookToReceive>(stringBooks);
                ViewData["Book"] = book;
                return View("BookDetails");
            }
            else
            {
                return RedirectToAction("Index", "Home"); ;
            }
        }

        public  IActionResult SignIn()
        {
            ViewData["User"] = ((TokenReader)_reader).GetName();
            ViewData["Login"] = ((TokenReader)_reader).GetName();
            return View();
        }

        public IActionResult SignUp(int status = 0)
        {
            ViewData["Status"] = status;
            return View();
        }

        [BindProperty]
        public LogPass LogPass { get; set; }

        public IActionResult OnSignOut()
        {
            ((TokenReader)_reader).SetName();
            ((TokenReader)_reader).SetToken();

            return RedirectToAction("SignIn", "Home");
        }

        public async Task<IActionResult> OnSignInAsync()
        {
            //var request = Request.Form;
            var result = new ContentResult();
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
                result.StatusCode = 200;
                return RedirectToAction("Index", "Home");
            }

            result.StatusCode = 400;
            return RedirectToAction("SignIn", "Home");
        }

        public async Task<IActionResult> OnSignUpAsync()
        {
            var result = new ContentResult();
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
                result.StatusCode = (int)response.StatusCode;
                return RedirectToAction("SignUp", "Home", new { status = 1});
            }

            result.StatusCode = (int)response.StatusCode;
            Console.WriteLine(response);
            return RedirectToAction("SignUp", "Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}