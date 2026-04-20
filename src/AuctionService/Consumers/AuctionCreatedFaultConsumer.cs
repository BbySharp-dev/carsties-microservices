using AutoMapper;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        try
        {
            Console.WriteLine("--> Consuming faulty creation");
            var exception = context.Message.Exceptions.First();
            return Task.CompletedTask;
        }
        catch (Exception exception1)
        {
            return Task.FromException(exception1);
        }
    }
}