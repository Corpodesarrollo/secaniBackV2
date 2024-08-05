using System.IdentityModel.Tokens.Jwt;

namespace MSAuthentication.Api.Utilities
{
    public class JWTUtilities
    {
        public static DataToken? Decode(string pToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var tokenClaims = tokenHandler.ReadToken(pToken.Replace("Bearer ", "")) as JwtSecurityToken;
            int businessId = 0;

            if (tokenClaims != null)
            {
                return new DataToken()
                {
                    Sub = tokenClaims.Claims.First(x => x.Type == "sub").Value,
                    Jti = tokenClaims.Claims.First(x => x.Type == "jti").Value,
                    Exp = tokenClaims.Claims.First(x => x.Type == "exp").Value
                };
            }
            else
            {
                return null;
            }

            
        }
    }
}
