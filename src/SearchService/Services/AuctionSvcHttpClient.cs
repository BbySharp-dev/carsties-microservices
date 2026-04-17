using System.Globalization;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
{
    private readonly IConfiguration _config = config;
    private readonly HttpClient _httpClient = httpClient;

    // public async Task<List<Item>> GetItemsForSearchDb()
    // {
    //     var lastUpdated = await DB.Find<Item, string>()
    //         .Sort(s => s.Descending(item => item.UpdatedAt))
    //         .Project(p => p.UpdatedAt.ToString(CultureInfo.InvariantCulture))
    //         .ExecuteFirstAsync();
    //
    //     return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"] + "/api/auctions?date=" +
    //                                                           lastUpdated);
    // }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdatedDate = await DB.Find<Item, DateTime>()
            .Sort(s => s.Descending(item => item.UpdatedAt))
            .Project(p => p.UpdatedAt)
            .ExecuteFirstAsync();

        var lastUpdated = lastUpdatedDate.ToString(CultureInfo.InvariantCulture);

        return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"] + "/api/auctions?date=" +
                                                              lastUpdated);
    }
}