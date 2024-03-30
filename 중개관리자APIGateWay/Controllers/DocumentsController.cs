using Common.DTO;
using Common.Services.NotificationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using RabbitMQ.Client;
using System.Text;

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
        [HttpPost("send")]
        public async Task<IActionResult> SendDocument([FromBody] CreateEncryptDocumentDto documentDto)
        {
            // HMAC 검증
            // 이 예제에서는 HMAC 검증을 단순화합니다. 실제 구현에서는 송신자의 공개키를 사용하여 HMAC을 검증해야 합니다.
            // 예를 들어, 송신자의 공개키를 이용하여 세션키를 복호화하고,
            // 이 세션키를 사용하여 문서에 대한 HMAC을 계산하여 전송된 HMAC과 비교합니다.
            // HMAC 검증 로직 구현 필요...

            bool isHmacValid = true; // HMAC 검증 결과를 표시하는 부울 변수. 실제 구현에서는 동적으로 계산됩니다.

            if (!isHmacValid)
            {
                return BadRequest("HMAC 검증 실패.");
            }

            // 수신자의 RabbitMQ 큐에 문서 Enqueue
            // RabbitMQ 설정 값들은 애플리케이션의 구성 설정에서 가져와야 합니다.
            var factory = new ConnectionFactory() { HostName = "your_rabbitmq_server" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var queueName = $"documentQueue_{documentDto.RecipientId}";
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var messageBody = Encoding.UTF8.GetBytes(documentDto.EncryptedDocument);
                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: messageBody);
            }

            return Ok(new { Message = "문서가 성공적으로 전송되었습니다." });
        }

        [HttpGet("{recipientId}")]
        public IActionResult RetrieveDocument(string recipientId)
        {
            var factory = new ConnectionFactory() { HostName = "your_rabbitmq_server", VirtualHost = "/", Port = 5672 };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var queueName = $"documentQueue_{recipientId}";

                // 큐에서 하나의 메시지를 Dequeue합니다.
                var result = channel.BasicGet(queue: queueName, autoAck: true);

                if (result == null)
                {
                    // 큐에 메시지가 없는 경우
                    return NotFound("No documents found for the given recipient ID.");
                }
                else
                {
                    var documentContent = Encoding.UTF8.GetString(result.Body.ToArray());
                    return Ok(new { Document = documentContent });
                }
            }
        }
    }

}
