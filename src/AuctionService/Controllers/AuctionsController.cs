using System.Globalization;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController(
    AuctionDbContext context,
    IMapper mapper,
    IPublishEndpoint publishEndpoint)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var query = context.Auctions.OrderBy(q => q.Item.Make)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(date))
        {
            if (!DateTime.TryParse(
                    date,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsedDate))
                return BadRequest("Invalid 'date' format. Please use ISO 8601 format, e.g. 2026-04-16T04:13:30Z");

            query = query.Where(q => q.UpdatedAt > parsedDate);
        }

        return await query.ProjectTo<AuctionDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auctions = await context.Auctions.Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (auctions == null) return NotFound();
        return mapper.Map<AuctionDto>(auctions);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {
        var auction = mapper.Map<Auction>(createAuctionDto);
        auction.Seller = "test";

        context.Auctions.Add(auction);

        var newAuction = mapper.Map<AuctionDto>(auction);

        await publishEndpoint.Publish(mapper.Map<AuctionCreated>(newAuction));

        var result = await context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to the DB");
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await context.Auctions.Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null) return NotFound();

        // TODO: check seller == username

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;

        await publishEndpoint.Publish(mapper.Map<AuctionUpdated>(auction));

        var result = await context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Problem saving changes to the DB");
        return Ok("");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await context.Auctions.FindAsync(id);
        if (auction == null) return NotFound();

        //TODO: Check seller == username
        context.Auctions.Remove(auction);
        await publishEndpoint.Publish(new AuctionDeleted { Id = auction.Id.ToString() });
        var result = await context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not save changes to the DB");
        return Ok();
    }
}