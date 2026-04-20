using System.Globalization;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
{
    // public async Task<List<Item>> GetItemsForSearchDb()
    // {
    //     var lastUpdated = await DB.Find<Item, string>()
    //         .Sort(s => s.Descending(item => item.UpdatedAt))
    //         .Project(p => p.UpdatedAt.ToString(CultureInfo.InvariantCulture))
    //         .ExecuteFirstAsync();
    //
    //     return await httpClient.GetFromJsonAsync<List<Item>>(config["AuctionServiceUrl"] + "/api/auctions?date=" +
    //                                                           lastUpdated);
    // }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdatedDate = await DB.Find<Item, DateTime>()
            .Sort(s => s.Descending(item => item.UpdatedAt))
            .Project(p => p.UpdatedAt)
            .ExecuteFirstAsync();

        var lastUpdated = lastUpdatedDate.ToString(CultureInfo.InvariantCulture);

        return await httpClient.GetFromJsonAsync<List<Item>>(config["AuctionServiceUrl"] + "/api/auctions?date=" +
                                                             lastUpdated);
    }
}