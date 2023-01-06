using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace H4_Poker_Engine.Authentication
{
    public class TokenGenerator
    {
        #region fields
        private readonly IConfiguration _configuration;
        private Claim _username;
        private Claim _role;
        private DateTime _expires;
        #endregion

        #region Constructors
        public TokenGenerator(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the fields for the JWT token
        /// Sets Name to Guest since it's only an authentication token.
        /// Sets Gives the role "LoginRole" to the token.
        /// And adds a live time of 10 minuetes for the token.
        /// </summary>
        /// <returns>A string based JWT token.</returns>
        public string GenerateLoginToken()
        {
            _username = new Claim(ClaimTypes.Name, "Guest");
            _role = new Claim(ClaimTypes.Role, "LoginRole");
            _expires = DateTime.Now.AddMinutes(10);
            return GenerateToken();
        }

        /// <summary>
        /// Sets the fields for the JWT token.
        /// Sets _name to the userinputted name from the app.
        /// Sets _role "UserRole" for the token.
        /// And adds a live time of 1 day for the token,
        /// to not disconnect the user, in the middle of a game.
        /// </summary>
        /// <param name="username">The username the user has inputted in the app's login form.</param>
        /// <returns>A string based JWT token.</returns>
        public string GenerateUserToken(string username)
        {
            _username = new Claim(ClaimTypes.Name, username);
            _role = new Claim(ClaimTypes.Role, "UserRole");
            _expires = DateTime.Now.AddDays(1);
            return GenerateToken();
        }

        /// <summary>
        /// Generates a JWT token used for authentication.
        /// Encrypted with a HMAC SHA512 algorithms.
        /// </summary>
        /// <returns>A string based JWT token.</returns>
        private string GenerateToken()
        {
            List<Claim> claims = new List<Claim>
            {
                _username,
                _role
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            //Creates the token with the credentials, the key and the roles etc we have made earlier.
            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: credentials,
                expires: _expires
                );
            //Converts the token to a string we can return.
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        #endregion
    }
}
