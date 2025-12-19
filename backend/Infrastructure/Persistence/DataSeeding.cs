using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class DataSeeding(SaasDbContext _saasDbContext) : IDataSeeding
    {
        public async Task DataSeedAsync()
        {
            //convert to async 
            try
            {
                var pendingMigrations = await _saasDbContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                   await _saasDbContext.Database.MigrateAsync();
                }

                //var store =await File.ReadAllTextAsync(@"..\Infrastructure\Persistence\DataSeed\store.json");
                
                var store = File.OpenRead(@"..\Infrastructure\Persistence\DataSeed\store.json");

                // Convert JSON to object

                var storeData = await JsonSerializer.DeserializeAsync<List<Store>>(store);
                if (storeData is not null && storeData.Count > 0)
                {
                   await _saasDbContext.Stores.AddRangeAsync(storeData!);
                    //await _saasDbContext.SaveChangesAsync();
                }   
            }
            catch (Exception)
            {

                throw;
            }
            // === Campaign Seeding ===
            try
            {
                var campaignFile = File.OpenRead(@"..\Infrastructure\Persistence\DataSeed\campaign.json");
                var campaignData = await JsonSerializer.DeserializeAsync<List<Campaign>>(campaignFile);

                if (campaignData is not null && campaignData.Count > 0)
                {
                    await _saasDbContext.Campaigns.AddRangeAsync(campaignData);
                    //await _saasDbContext.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Campaign seed error: " + ex.Message);
            }

            // === CampaignPost Seeding ===
            try
            {
                var campaignPostFile = File.OpenRead(@"..\Infrastructure\Persistence\DataSeed\campaignPost.json");
                var campaignPostData = await JsonSerializer.DeserializeAsync<List<CampaignPost>>(campaignPostFile);

                if (campaignPostData is not null && campaignPostData.Count > 0)
                {
                    await _saasDbContext.CampaignPosts.AddRangeAsync(campaignPostData);
                    //await _saasDbContext.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CampaignPost seed error: " + ex.Message);
            }

        }
    }
}
