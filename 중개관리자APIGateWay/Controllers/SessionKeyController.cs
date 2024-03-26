using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace 중개관리자APIGateWay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionKeyController : ControllerBase
    {
        // GET: api/SessionKey
        [HttpGet]
        public IActionResult GetSessionKey()
        {
            // 256비트(32바이트) 랜덤 키 생성
            byte[] key = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }

            // 생성된 키를 Base64 인코딩하여 반환
            string base64Key = Convert.ToBase64String(key);
            return Ok(new { SessionKey = base64Key });
        }
    }
}
