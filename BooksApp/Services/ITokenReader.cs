using System.Text.Json.Nodes;

namespace BooksApp.Services
{
    public interface ITokenReader
    {
        public string GetToken();
    }
}