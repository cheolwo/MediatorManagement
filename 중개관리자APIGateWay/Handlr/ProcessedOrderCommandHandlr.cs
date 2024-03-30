using MediatR;
using OrderCommon.Model;
using 주문Common.DTO;

namespace 중개관리자APIGateWay.Handlr
{
    public class ProcessedOrderCommandHandler : IRequestHandler<ProcessedOrderCommand, bool>
    {
        private readonly 주문DbContext _dbContext;
        private readonly ILogger<ProcessedOrderCommandHandler> _logger;

        public ProcessedOrderCommandHandler(주문DbContext dbContext, ILogger<ProcessedOrderCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public Task<bool> Handle(ProcessedOrderCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
