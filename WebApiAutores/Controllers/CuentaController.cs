using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers
{

    [ApiController]
    [Route("api/cuenta")]
    public class CuentaController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;

        public SignInManager<IdentityUser> SignInManager;

        public CuentaController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            SignInManager = signInManager;
        }

        [HttpPost("registro")]
        public async Task<ActionResult<RespuestaAutenticacion>> Registro(CredencialesUsuario credencialesUsuario)
        {
            // Crear un nuevo usuario basado en las credenciales proporcionadas.
            var usuario = new IdentityUser() { UserName = credencialesUsuario.Email, Email = credencialesUsuario.Email };

            // Intentar crear el usuario en la base de datos.
            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);

            // Devolver una respuesta según el resultado.
            if (resultado.Succeeded)
                return Ok("El usuario fue agregado");
            else
                return BadRequest(resultado.Errors);
        }

        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuario credencialesUsuario)
        {
            // Intentar iniciar sesión con las credenciales proporcionadas.
            var resultado = await SignInManager.PasswordSignInAsync(credencialesUsuario.Email, credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            // Devolver una respuesta según el resultado.
            if (resultado.Succeeded)
                return GenerarToken(credencialesUsuario);
            else
                return BadRequest("Usuario o contraseña incorrecta");
        }

        // Generar un token JWT para el usuario autenticado.
        private RespuestaAutenticacion GenerarToken(CredencialesUsuario credencialesUsuario)
        {
            // Definir los claims (reclamaciones) que contendrá el token.
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuario.Email), // Claim de email
                new Claim("Claim1", "Claim2") // Otras reclamaciones personalizadas
            };

            // Generar una clave simétrica utilizando la llave JWT proporcionada en la configuración.
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llaveJwt"]));

            // Crear las credenciales para firmar el token.
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            // Establecer la fecha de expiración del token (1 año a partir de la fecha actual).
            var expiracion = DateTime.UtcNow.AddYears(1);

            // Crear un token JWT con los claims, la fecha de expiración y las credenciales de firma.
            var securityToken = new JwtSecurityToken(issuer: null, claims: claims, expires: expiracion, signingCredentials: credenciales);

            // Crear una respuesta que incluye el token JWT y la fecha de expiración.
            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken), // Convertir el token a su representación en string
                Expiracion = expiracion // Incluir la fecha de expiración en la respuesta
            };
        }

    }

}
