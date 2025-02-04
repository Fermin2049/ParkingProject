using System.Threading.Tasks;
using FinalMarzo.net.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinalMarzo.net.Controllers
{
    [Route("api/email-test")]
    [ApiController]
    public class EmailTestController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailTestController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendTestEmail([FromBody] EmailRequest request)
        {
            if (string.IsNullOrEmpty(request.To))
            {
                return BadRequest("The 'To' field is required.");
            }

            await _emailService.SendEmailAsync(
                request.To,
                request.Subject ?? string.Empty,
                request.Body ?? string.Empty
            );
            return Ok("Correo enviado exitosamente.");
        }
    }

    public class EmailRequest
    {
        public string? To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
}
