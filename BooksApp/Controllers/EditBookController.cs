using BooksApp.Models;
using BooksApp.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestProject.Models;
using System.Diagnostics;

namespace BooksApp.Controllers
{
    public class EditBookController : Controller
    {
        private readonly ILogger<EditBookController> _logger;
        private readonly HttpClient _client;
        private readonly ITokenReader _reader;

        [BindProperty]
        public BookToEdit? BookToEdit { get; set; }

        public EditBookController(ILogger<EditBookController> logger, HttpClient client, ITokenReader reader)
        {
            _logger = logger;
            _client = client;
            _reader = reader;
        }

        public async Task<IActionResult> EditBookAsync(string editBook)
        {
            var login = ((TokenReader)_reader).GetName();
            if (login != null)
            {
                var book = JsonConvert.DeserializeObject<BookToReceive>(editBook);

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

                            ViewData["Languages"] = languages;
                            ViewData["Publishers"] = publishers;
                            ViewData["Authors"] = authors;
                            ViewData["BookToEdit"] = book;
                            ViewData["Login"] = login;

                            return View("BookEdit");
                        }

                        return BadRequest();
                    }

                    return BadRequest();
                }

                return BadRequest();
            }

            return Unauthorized();
        }

        public async Task<IActionResult> OnEditAsync(string book)
        {
            var oldBook = JsonConvert.DeserializeObject<BookToReceive>(book);
            var authorBooksApiUrl = "https://localhost:7159/api/AuthorBooks/";
            var editApiUrl = "https://localhost:7159/api/Books/" + oldBook.Book_id;
            string token = _reader.GetToken();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            HttpResponseMessage? response;

            List<int> oldAuthorsId = new List<int>();
            oldBook.Authors.ForEach(author =>
            {
                oldAuthorsId.Add(author.Author_id);
            });

            for (int i = 0; i < BookToEdit.AuthorsId.Count; ++i)
            {
                if (!oldAuthorsId.Contains(BookToEdit.AuthorsId[i]))
                {
                    response = await _client.PostAsJsonAsync(authorBooksApiUrl,
                        new { book_id = oldBook.Book_id, author_id = BookToEdit.AuthorsId[i] });
                    if (!response.IsSuccessStatusCode) BadRequest();
                }
            }

            for (int i = 0; i < oldBook.Authors.Count; ++i)
            {
                if (!BookToEdit.AuthorsId.Contains(oldAuthorsId[i]))
                {
                    response = await _client.DeleteAsync(authorBooksApiUrl + oldBook.Book_id + " & " + oldAuthorsId[i]);
                    if (!response.IsSuccessStatusCode) BadRequest();
                }
            }

            var multiFormData = new MultipartFormDataContent();
            multiFormData.Add(new StringContent("" + oldBook.Book_id), "Book_id");
            multiFormData.Add(new StringContent(BookToEdit.Title), "Title");
            multiFormData.Add(new StringContent(BookToEdit.Isbn13), "Isbn13");
            multiFormData.Add(new StringContent("" + BookToEdit.Num_pages), "Num_pages");
            multiFormData.Add(new StringContent("" + BookToEdit.Publication_date), "Publication_date");
            multiFormData.Add(new StringContent("" + BookToEdit.Publisher_id), "Publisher_id");
            multiFormData.Add(new StringContent("" + BookToEdit.Language_id), "Language_id");

            if (BookToEdit.FileUri == null)
                multiFormData.Add(new StringContent(""), "FileUri");
            else multiFormData.Add(new StreamContent(BookToEdit.FileUri.OpenReadStream()), "FileUri", BookToEdit.FileUri.FileName);

            response = await _client.PutAsync(editApiUrl, multiFormData);
            if (response.IsSuccessStatusCode)
            {
                var userResp = await _client.GetAsync("https://localhost:7159/api/Users/" + ((TokenReader)_reader).GetName());
                var user = JsonConvert.DeserializeObject<User>(await userResp.Content.ReadAsStringAsync());

                ViewData["Role"] = user.Role;
                return RedirectToAction("GetBookDetails", "BookDetails", new { id = oldBook.Book_id });
            }

            return BadRequest();
        }

        public async Task<IActionResult> OnDeleteAsync(int bookId)
        {
            var apiUrl = "https://localhost:7159/api/Books/" + bookId;
            string token = _reader.GetToken();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await _client.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index", "Home");

            return BadRequest();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}