using H4_Poker_Engine.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace H4_Poker_Engine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        #region fields
        private readonly TokenGenerator _tokenGenerator;
        #endregion

        #region Constructors
        public LoginController(TokenGenerator tokenGenerator)
        {
            _tokenGenerator = tokenGenerator;
        }
        #endregion

        #region Methods
        /// <summary>
        /// HttpGet method that returns a JWT Login Token.
        /// </summary>
        /// <returns>Status of 200, and a login token, created from the TokenGenerator class.</returns>
        [HttpGet("guesttoken")]
        public async Task<ActionResult> GetLoginToken()
        {
            return Ok(_tokenGenerator.GenerateLoginToken());
        }

        /// <summary>
        /// HttpPost method that returns a JWT User Token.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>Status of 200, and a user token, created from the TokenGenerator class.</returns>
        [HttpPost("usertoken"), Authorize]
        public async Task<ActionResult> GetUserToken(string username)
        {
            if (username == string.Empty || username == "")
            {
                return BadRequest("Invalid or empty username");
            }
            else
            {
                return Ok(_tokenGenerator.GenerateUserToken(username));
            }
        }
        #endregion
    }
}
