using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FinalMarzo.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsStatusController : ControllerBase
    {
        // Reemplaza con tu Access Token de producción o de sandbox según corresponda
        private const string AccessToken =
            "APP_USR-346340649101918-030309-5fc24a22c8ed17a3577500749e6a6e9b-190814641";

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPaymentStatus(string paymentId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new System.Uri("https://api.mercadopago.com/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    AccessToken
                );

                // Llama al endpoint de Mercado Pago para obtener el pago
                HttpResponseMessage response = await client.GetAsync($"v1/payments/{paymentId}");
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Devuelve la respuesta JSON completa
                    return Ok(content);
                }
                else
                {
                    // Devuelve el error para que puedas inspeccionarlo
                    return StatusCode((int)response.StatusCode, content);
                }
            }
        }
    }
}
