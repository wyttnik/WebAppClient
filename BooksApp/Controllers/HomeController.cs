using BooksApp.Models;
using BooksApp.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

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

        public async Task<IActionResult> IndexAsync(string errorMessage = "")
        {
            var apiUrl = "https://localhost:7159/api/Books";
            var response = await _client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode) {
                var stringBooks = await response.Content.ReadAsStringAsync();
                var books = JsonConvert.DeserializeObject<IEnumerable<BookToReceive>>(stringBooks);

                string token = _reader.GetToken();
                if (token != null)
                {
                    _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    var userResp = await _client.GetAsync("https://localhost:7159/api/Users/" + ((TokenReader)_reader).GetName());
                    var user = JsonConvert.DeserializeObject<User>(await userResp.Content.ReadAsStringAsync());

                    ViewData["Role"] = user.Role;
                }
                else ViewData["Role"] = "";
                ViewData["Books"] = books;
            }
            ViewData["Login"] = ((TokenReader)_reader).GetName();
            ViewData["ErrorMessage"] = errorMessage;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}