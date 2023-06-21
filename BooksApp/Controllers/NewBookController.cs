using BooksApp.Models;
using BooksApp.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestProject.Models;
using System.Diagnostics;

namespace BooksApp.Controllers
{
    public class NewBookController : Controller
    {
        private readonly ILogger<NewBookController> _logger;
        private readonly HttpClient _client;
        private readonly ITokenReader _reader;

        [BindProperty]
        public BookToEdit? BookToEdit { get; set; }

        public NewBookController(ILogger<NewBookController> logger, HttpClient client, ITokenReader reader)
        {
            _logger = logger;
            _client = client;
            _reader = reader;
        }

        public async Task<IActionResult> PostBookAsync()
        {
            var login = ((TokenReader)_reader).GetName();
            if (login != null)
            {
                var apiUrl = "https://localhost:7159/api/Authors";
                string token = _reader.GetToken();
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var response = await _client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var stringAuthors = await response.Content.ReadAsStringAsync();
                    var authors = JsonConvert.DeserializeObject<IEnumerable<AuthorToReceive>>(stringAuthors);

                    apiUrl = "https://localhost:7159/api/Publishers";
                    response = await _client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var stringPublishers = await response.Content.ReadAsStringAsync();
                        var publishers = JsonConvert.DeserializeObject<IEnumerable<PublisherToReceive>>(stringPublishers);

                        apiUrl = "https://localhost:7159/api/BookLanguages";
                        response = await _client.GetAsync(apiUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            var stringLanguages = await response.Content.ReadAsStringAsync();
                            var languages = JsonConvert.DeserializeObject<IEnumerable<BookLanguageToReceive>>(stringLanguages);

                            apiUrl = "https://localhost:7159/api/Books";
                            response = await _client.GetAsync(apiUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                var stringBooks = await response.Content.ReadAsStringAsync();
                                var books = JsonConvert.DeserializeObject<List<BookToReceive>>(stringBooks);
                                ViewData["NewBookId"] = books[books.Count - 1].Book_id + 1;
                                ViewData["Languages"] = languages;
                                ViewData["Publishers"] = publishers;
                                ViewData["Authors"] = authors;
                                ViewData["Login"] = login;

                                return View("BookAdd");
                            }

                            return BadRequest();
                        }

                        return BadRequest();
                    }

                    return BadRequest();
                }

                return BadRequest();
            }

            return Unauthorized();
        }

        public async Task<IActionResult> OnPostAsync(int newBookId)
        {
            var authorBooksApiUrl = "https://localhost:7159/api/AuthorBooks/";
            var postApiUrl = "https://localhost:7159/api/Books";

            string token = _reader.GetToken();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            HttpResponseMessage? response;



            var multiFormData = new MultipartFormDataContent();
            multiFormData.Add(new StringContent("" + newBookId), "Book_id");
            multiFormData.Add(new StringContent(BookToEdit.Title), "Title");
            multiFormData.Add(new StringContent(BookToEdit.Isbn13), "Isbn13");
            multiFormData.Add(new StringContent("" + BookToEdit.Num_pages), "Num_pages");
            multiFormData.Add(new StringContent("" + BookToEdit.Publication_date), "Publication_date");
            multiFormData.Add(new StringContent("" + BookToEdit.Publisher_id), "Publisher_id");
            multiFormData.Add(new StringContent("" + BookToEdit.Language_id), "Language_id");

            if (BookToEdit.FileUri == null)
                multiFormData.Add(new StringContent(""), "FileUri");
            else multiFormData.Add(new StreamContent(BookToEdit.FileUri.OpenReadStream()), "FileUri", BookToEdit.FileUri.FileName);

            response = await _client.PostAsync(postApiUrl, multiFormData);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest();
            }

            for (int i = 0; i < BookToEdit.AuthorsId.Count; ++i)
            {
                response = await _client.PostAsJsonAsync(authorBooksApiUrl,
                    new { book_id = newBookId, author_id = BookToEdit.AuthorsId[i] });
                if (!response.IsSuccessStatusCode) BadRequest();
            }

            var userResp = await _client.GetAsync("https://localhost:7159/api/Users/" + ((TokenReader)_reader).GetName());
            var user = JsonConvert.DeserializeObject<User>(await userResp.Content.ReadAsStringAsync());

            ViewData["Role"] = user.Role;
            return RedirectToAction("Index", "Home");

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}