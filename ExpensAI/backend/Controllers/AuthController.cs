using Microsoft.AspNetCore.Mvc;

namespace ExpensAI.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login()
        {
            // Simulação de geração de JWT e Refresh Token
            var jwtToken = "simulated_jwt_token_header.payload.signature";
            
            // Regra Crítica: NUNCA enviar o token no corpo para o client salvar em LocalStorage
            // Enviar os tokens como cookies HttpOnly, Secure e SameSite=Strict
            
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,        // JavaScript (XSS) não pode ler o cookie
                Secure = true,          // Apenas transmitido sobre HTTPS
                SameSite = SameSiteMode.Strict, // Previne CSRF
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            Response.Cookies.Append("access_token", jwtToken, cookieOptions);

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth/refresh", // Apenas o endpoint de refresh terá acesso a este cookie
                Expires = DateTime.UtcNow.AddDays(7)
            };
            
            Response.Cookies.Append("refresh_token", "simulated_refresh_token", refreshCookieOptions);

            return Ok(new { message = "Autenticação realizada com sucesso. Tokens seguros enviados." });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Para limpar o cookie no lado do servidor, o sobrescrevemos expirado
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/api/auth/refresh" });
            
            return Ok(new { message = "Sessão encerrada com segurança." });
        }
    }
}
