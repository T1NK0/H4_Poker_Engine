using H4_Poker_Engine.Authentication;
using H4_Poker_Engine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace H4_Poker_Engine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        #region fields
        private readonly ITokenGenerator _tokenGenerator;
        #endregion

        #region Constructors
        public LoginController(ITokenGenerator tokenGenerator)
        {
            _tokenGenerator = tokenGenerator;
        }
        #endregion

        #region Methods
        /// <summary>
        /// HttpGet method that returns a JWT Login Token.
        /// </summary>
        /// <returns>JWT login token</returns>
        [HttpGet("guesttoken")]
        public async Task<ActionResult> GetLoginToken()
        {
            return Ok(_tokenGenerator.GenerateLoginToken());
        }

        /// <summary>
        /// HttpPost method that returns a JWT User Token.
        /// </summary>
        /// <param name="username">Login DTO containing users player name</param>
        /// <returns>JWT user token</returns>
        [HttpPost("usertoken"), Authorize]
        public async Task<ActionResult> GetUserToken(LoginDTO request)
        {
            if (request.Username == string.Empty || request.Username == "")
            {
                return BadRequest("Invalid or empty username");
            }
            else
            {
                return Ok(_tokenGenerator.GenerateUserToken(request.Username));
            }
        }
        #endregion
    }
}
