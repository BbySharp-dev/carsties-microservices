using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionUpdatedFaultConsumer : IConsumer<Fault<AuctionUpdated>>
{
    public Task Consume(ConsumeContext<Fault<AuctionUpdated>> context)
    {
        try
        {
            Console.WriteLine("--> Consuming faulty update");
            var exception = context.Message.Exceptions.First();
            return Task.CompletedTask;
        }
        catch (Exception exception1)
        {
            return Task.FromException(exception1);
        }
    }
}