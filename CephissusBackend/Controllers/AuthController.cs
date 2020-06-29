using CephissusBackend.Contracts;
using CephissusBackend.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CephissusBackend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public ActionResult<AuthResponse> Authenticate()
        {
            var redirect = _authService.GetAuthRedirect();

            return new AuthResponse
            {
                Authenticated = false,
                RedirectUrl = redirect,
                Token = null,
            };
        }

        [HttpGet]
        [Route("oauthcallback")]
        public async Task<ActionResult<object>> OauthCallback(string code)
        {
            return await _authService.OauthCallback(code);
        }
    }
}