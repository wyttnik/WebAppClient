
namespace BooksApp.Services
{
    public class TokenReader : ITokenReader
    {
        private string? _token;
        private string? _userName;
        public string GetToken()
        {
            return _token;
        }

        public void SetToken(string? token = null)
        {
            _token = token;
        }

        public string? GetName()
        {
            return _userName;
        }

        public void SetName(string? userName = null)
        {
            _userName = userName;
        }

    }
}
