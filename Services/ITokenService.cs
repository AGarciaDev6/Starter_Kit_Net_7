using Microsoft.IdentityModel.Tokens;
using Starter_NET_7.Interfaces;
using Starter_NET_7.Database.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Starter_NET_7.Services
{
  public class ITokenService : IToken
  {
    private SymmetricSecurityKey _keyToken;
    private SymmetricSecurityKey _refreshKey;
    private readonly IHttpContextAccessor _httpContext;

    public ITokenService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
    {
      _keyToken = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("Jwt:Key").Value!));
      _refreshKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("Jwt:Refresh").Value!));
      _httpContext = httpContextAccessor;
    }

    public string CreateToken(int idUser, DateTime expires)
    {
      List<Claim> claims = new List<Claim>()
            {
                new Claim("IdUser", idUser.ToString())
            };

      var credenciales = new SigningCredentials(_keyToken, SecurityAlgorithms.HmacSha512Signature);

      var tokenDescription = new JwtSecurityToken(
              claims: claims,
              expires: expires,
              signingCredentials: credenciales
          );

      return new JwtSecurityTokenHandler().WriteToken(tokenDescription);
    }

    public string CreateRefreshToken(DateTime expires)
    {
      var credenciales = new SigningCredentials(_refreshKey, SecurityAlgorithms.HmacSha512Signature);

      var tokenDescription = new JwtSecurityToken(
              expires: expires,
              signingCredentials: credenciales
          );

      return new JwtSecurityTokenHandler().WriteToken(tokenDescription);
    }

    public ClaimsPrincipal DecodeToken(string token, bool isToken = true)
    {
      var handler = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters()
      {
        IssuerSigningKey = isToken ? _keyToken : _refreshKey,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
      }, out SecurityToken securityToken);

      return handler;
    }

    public int GetIdUserOfToken()
    {
      try
      {
        var request = _httpContext.HttpContext!.Request;
        var bearer = request.Headers.Authorization.FirstOrDefault(x => x!.StartsWith("Bearer "));
        string token = bearer!.Replace("Bearer ", "");
        var handler = DecodeToken(token, true);
        var user = handler.Claims.FirstOrDefault(x => x.Type == "IdUser")!.Value;

        return int.Parse(user);
      }
      catch
      {
        return -1;
      }
    }
  }
}
