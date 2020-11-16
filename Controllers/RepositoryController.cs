using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using webapi_github_wrapper.Models;

namespace webapi_github_wrapper.Controllers
{
    [ApiController]
    [Route("v1/github_wrapper")]
    
    public class RepositoryController : ControllerBase
    {
        private HttpClient client;
        private IMemoryCache cache;

        public RepositoryController(HttpClient client, IMemoryCache memoryCache)
        {
            this.client = client;
            this.cache = memoryCache;
        }

        [HttpGet]
        [Route("{organizationName}/repos")]

        public async Task<ActionResult<List<Repository>>> CacheGetOrCreate(string organizationName)
        {
            var cacheEntry = await
                cache.GetOrCreateAsync(organizationName, entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                    return ProcessRepositories(organizationName);
                });
            return cacheEntry;
        }
        
        private async Task<List<Repository>> ProcessRepositories(string organizationName)
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