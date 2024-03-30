using Quartz;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using 주문Common.DTO;
using MediatR;

namespace 중개관리자APIGateWay.MessageReceiver
{
    public class RabbitMQMessageReceiverJob : IJob
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RabbitMQMessageReceiverJob> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        public RabbitMQMessageReceiverJob(IDistributedCache distributedCache, 
            ILogger<RabbitMQMessageReceiverJob> logger, 
            IConfiguration configuration,
            IMediator mediator)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _configuration = configuration;
            _mediator = mediator;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var rabbitMqHostName = _configuration["RabbitMQ:HostName"];
            var processedOrdersQueueName = _configuration["RabbitMQ:ProcessedOrdersQueueName"];

            var factory = new ConnectionFactory() { HostName = rabbitMqHostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: processedOrdersQueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var cacheKey = Encoding.UTF8.GetString(body);
                    var processedOrderJson = await _distributedCache.GetStringAsync(cacheKey);
                    if (processedOrderJson != null)
                    {
                        var processedOrder = JsonSerializer.Deserialize<ProcessedOrder>(processedOrderJson);
                        var command = new ProcessedOrderCommand(processedOrder);
                        await _mediator.Send(command);
                        _logger.LogInformation($"Processed order received from RabbitMQ: {cacheKey}");
                    }
                };
                channel.BasicConsume(queue: processedOrdersQueueName,
                                     autoAck: true,
                                     consumer: consumer);

                return Task.CompletedTask;
            }
        }
    }
}
