using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            query.Match(Search.Full, searchParams.SearchTerm)
                .SortByTextScore();
        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(m => m.Ascending(a => a.Make)),
            "new" => query.Sort(n => n.Descending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
        };

        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(f => f.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(e =>
                e.AuctionEnd < DateTime.UtcNow.AddDays(6) && e.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller)) query.Match(se => se.Seller == searchParams.Seller);
        if (!string.IsNullOrEmpty(searchParams.Winner)) query.Match(wi => wi.Winner == searchParams.Winner);

        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();

        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}