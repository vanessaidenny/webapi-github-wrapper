using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.FeatureManagement;
using webapi_github_wrapper.Models;

namespace webapi_github_wrapper.Controllers
{
    [ApiController]
    [Route("v1/github_wrapper")]
    
    public class RepositoryController : ControllerBase
    {
        private HttpClient client;
        private IMemoryCache cache;
        private IFeatureManager featureManager;

        public RepositoryController(HttpClient client, IMemoryCache memoryCache, IFeatureManager featureManager)
        {
            this.client = client;
            cache = memoryCache;
            this.featureManager = featureManager;
        }

        public static class FeatureFlags
        {
            public const string MemoryCache = "MemoryCache";
        }
        
        [HttpGet]
        [Route("{organizationName}/repos")]
        public async Task<ActionResult<List<Repository>>> GetRepositories(string organizationName)
        {
            var cacheIsEnabled = featureManager.IsEnabledAsync(FeatureFlags.MemoryCache);
            
            if(!await cacheIsEnabled) {
		        cache.Remove(organizationName);
            }
            return await ProcessRepositories(organizationName);
        }

        private async Task<List<Repository>> ProcessRepositories(string organizationName)
        {
            var cacheEntry = await
                cache.GetOrCreateAsync(organizationName, async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromDays(1);
                    entry.SetPriority(CacheItemPriority.High);
                    return await ClientRequest(organizationName);
                });
            return cacheEntry;
        }

        private async Task<List<Repository>> ClientRequest(string organizationName)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var login = $"https://api.github.com/orgs/{organizationName}/repos";
            var streamTask = client.GetStreamAsync(login);
            var repositories = await JsonSerializer.DeserializeAsync<List<Repository>>(await streamTask);

            return repositories;
        }
    }
}