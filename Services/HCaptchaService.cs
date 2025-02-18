using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FinalMarzo.net.Models;

namespace FinalMarzo.net.Services
{
    public class HCaptchaService
    {
        private readonly HttpClient _httpClient;
        private const string HCAPTCHA_VERIFY_URL = "https://hcaptcha.com/siteverify";
        private readonly string _secret;

        public HCaptchaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _secret = "ES_1cbcaba00cb1484b9afa84a18eaacb78"; // Reemplaza esto con tu clave secreta real
        }

        public async Task<bool> ValidateHCaptchaAsync(string hcaptchaResponse)
        {
            var request = new Dictionary<string, string>
            {
                { "response", hcaptchaResponse },
                { "secret", _secret },
            };

            var content = new FormUrlEncodedContent(request);
            var response = await _httpClient.PostAsync(HCAPTCHA_VERIFY_URL, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                if (doc.RootElement.TryGetProperty("success", out JsonElement successElement))
                {
                    return successElement.GetBoolean(); // ✅ Obtiene el valor correcto
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing hCaptcha response: {ex.Message}");
            }

            return false; // ❌ Si algo falla, devuelve false
        }
    }
}
