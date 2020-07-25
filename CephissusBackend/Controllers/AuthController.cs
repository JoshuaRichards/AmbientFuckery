using CephissusBackend.Contracts;
using CephissusBackend.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CephissusBackend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public AuthController(
            IAuthService authService,
            IUserRepository userRepository
        )
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        [HttpPost]
#if DEBUG
        [HttpGet]
#endif
        public ActionResult<AuthResponse> Authenticate()
        {
            if (Request.Cookies.TryGetValue("UserId", out var userId))
            {
                var user = _userRepository.GetById(Guid.Parse(userId));

                return new AuthResponse
                {
                    Authenticated = true,
                    User = user,
                };
            }
            var redirect = _authService.GetAuthRedirect();

            return new AuthResponse
            {
                Authenticated = false,
                RedirectUrl = redirect,
            };
        }

        [HttpGet]
        [Route("oauthcallback")]
        public async Task<ActionResult> OauthCallback(string code)
        {
            var userId = await _authService.OauthCallbackAsync(code);

            Response.Cookies.Append("UserId", userId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
            });

            return Redirect("https://localhost:4242");
        }
    }
}