using CephissusBackend.Contracts;
using CephissusBackend.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CephissusBackend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
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
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var userId = HttpContext.User.Claims.First(c => c.Type == "UserId").Value;
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

            var claims = new List<Claim> {
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.Name, userId.ToString()),
                new Claim(ClaimTypes.Role, "user"),
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });

            return Redirect("https://localhost:4242");
        }
    }
}