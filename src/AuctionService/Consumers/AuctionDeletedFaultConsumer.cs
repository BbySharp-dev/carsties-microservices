using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionDeletedFaultConsumer : IConsumer<Fault<AuctionDeleted>>
{
    public Task Consume(ConsumeContext<Fault<AuctionDeleted>> context)
    {
        try
        {
            Console.WriteLine("--> Consuming faulty deletion");
            var exception = context.Message.Exceptions.First();
            return Task.CompletedTask;
        }
        catch (Exception exception1)
        {
            return Task.FromException(exception1);
        }
    }
}