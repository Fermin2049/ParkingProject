using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalMarzo.net.Models
{
    public class HCaptchaVerificationRequest
    {
        public string Secret { get; set; } = "ES_1cbcaba00cb1484b9afa84a18eaacb78";
        public string? Response { get; set; }
    }
}
