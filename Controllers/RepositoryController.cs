using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using webapi_github_wrapper.Models;
using webapi_github_wrapper.Services;

namespace webapi_github_wrapper.Controllers
{
    [ApiController]
    [Route("v1/github_wrapper")]
    
    public class RepositoryController : ControllerBase
    {
       
        private IMemoryCache cache;
        private IFeatureManager featureManager;
        private readonly IConfiguration configuration;
        private IClientService service;

        public RepositoryController(IMemoryCache memoryCache, IFeatureManager featureManager, 
            IConfiguration configuration)
        {
            cache = memoryCache;
            this.featureManager = featureManager;
            this.configuration = configuration;
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
            
            if(!await cacheIsEnabled)
            {
                cache.Remove(organizationName);
            }
            return await ProcessByCache(organizationName);
        }

        private async Task<List<Repository>> ProcessByCache(string organizationName)
        {
            var cacheEntry = await
                cache.GetOrCreateAsync(organizationName, async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(configuration.
                        GetValue<double>("CacheManagement:SeatingSeconds"));
                    entry.SetPriority(CacheItemPriority.High);
                    return await service.ClientRequest(organizationName);
                });
            return cacheEntry;
        }
    }
}