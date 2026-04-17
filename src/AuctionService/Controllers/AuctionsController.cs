using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController(AuctionDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AuctionDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(q => q.Item.Make)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(date))
        {
            if (!DateTime.TryParse(
                    date,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsedDate))
            {
                return BadRequest("Invalid 'date' format. Please use ISO 8601 format, e.g. 2026-04-16T04:13:30Z");
            }

            query = query.Where(q => q.UpdatedAt > parsedDate);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auctions = await _context.Auctions.Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (auctions == null) return NotFound();
        return _mapper.Map<AuctionDto>(auctions);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
    {
        var auction = _mapper.Map<Auction>(createAuctionDto);
        auction.Seller = "test";
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not save changes to the DB");
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions.Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null) return NotFound();

        // TODO: check seller == username
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Problem saving changes to the DB");
        return Ok("");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null) return NotFound();

        //TODO: Check seller == username
        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not save changes to the DB");
        return Ok();
    }
}