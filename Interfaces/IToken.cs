using System.Security.Claims;

namespace Starter_NET_7.Interfaces
{
    public interface IToken
    {
        public string CreateToken(int idUser, DateTime expires);
        public string CreateRefreshToken(DateTime expires);
        public ClaimsPrincipal DecodeToken(string token, bool isToken);
        public int GetIdUserOfToken();
    }
}
