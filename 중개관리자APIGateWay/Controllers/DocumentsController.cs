using Common.Actor.APIService;
using Common.Services.NotificationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace 중개관리자APIGateWay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDistributedCache _cache;
        private readonly INotificationService _notificationService;

        public DocumentsController(IDistributedCache cache,
            INotificationService notificationService)
        {
            _cache = cache;
            _notificationService = notificationService;
        }
        /// <summary>
        /// 송신자가 문서와 세션키를 중개Server에 업로드
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UploadEncryptedDocumentAndSessionKey(
            [FromBody] DocumentUploadModel model)
        {
            try
            {
                string cacheKey = $"document_{model.RecipientId}_{model.DocumentId}";
                var cacheData = new
                {
                    EncryptedDocument = model.EncryptedDocument,
                    EncryptedSessionKey = model.EncryptedSessionKey
                };

                // Redis에 JSON 형태로 데이터 저장
                string jsonData = JsonConvert.SerializeObject(cacheData);
                await _cache.SetStringAsync(cacheKey, jsonData, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) // 12시간 후 만료
                });

                // 사용자에게 안전한 알림 전송
                string notificationMessage = "새로운 문서가 도착했습니다. 안전하게 확인하세요.";
                await _notificationService.SendNotificationAsync(model.RecipientId, notificationMessage);

                return Ok(new { Message = "Document and session key uploaded successfully and notification sent." });
            }
            catch (Exception ex)
            {
                // 예외 로깅
                // Log.Error($"Error uploading document: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred while uploading the document." });
            }
        }

        /// <summary>
        /// 수신자가 문서와 세션키를 다운로드
        /// </summary>
        /// <param name="recipientId"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet("{recipientId}/{documentId}")]
        public async Task<IActionResult> GetDocumentAndSessionKey(
            string recipientId, string documentId)
        {
            // 캐시에서 문서 조회를 위한 키 생성
            string cacheKey = $"document_{recipientId}+{documentId}";

            // Redis에서 암호화된 문서와 세션키 검색
            string jsonData = await _cache.GetStringAsync(cacheKey);
            if (string.IsNullOrEmpty(jsonData))
            {
                return NotFound(new { Message = "Document or session key not found." });
            }

            // JSON 데이터를 객체로 역직렬화
            var cacheData = JsonConvert.DeserializeObject<dynamic>(jsonData);

            return Ok(new
            {
                EncryptedDocument = cacheData.EncryptedDocument,
                EncryptedSessionKey = cacheData.EncryptedSessionKey
            });
        }
    }

}
