using BooksApp.Models;
using BooksApp.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestProject.Models;
using System.Diagnostics;

namespace BooksApp.Controllers
{
    public class BookDetailsController : Controller
    {
        private readonly ILogger<BookDetailsController> _logger;
        private readonly HttpClient _client;
        private readonly ITokenReader _reader;

        public BookDetailsController(ILogger<BookDetailsController> logger, HttpClient client, ITokenReader reader)
        {
            _logger = logger;
            _client = client;
            _reader = reader;
        }

        public async Task<IActionResult> GetBookDetails(int id)
        {
            var apiUrl = "https://localhost:7159/api/Books/" + id;
            string token = _reader.GetToken();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await _client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var stringBook = await response.Content.ReadAsStringAsync();
                var book = JsonConvert.DeserializeObject<BookToReceive>(stringBook);
                var userResp = await _client.GetAsync("https://localhost:7159/api/Users/" + ((TokenReader)_reader).GetName());
                var user = JsonConvert.DeserializeObject<User>(await userResp.Content.ReadAsStringAsync());
                ViewData["Role"] = user.Role;
                ViewData["Book"] = book;
                ViewData["Login"] = ((TokenReader)_reader).GetName();
                return View("BookDetails");
            }

            return RedirectToAction("Index", "Home",
                new { errorMessage = response.ReasonPhrase });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}