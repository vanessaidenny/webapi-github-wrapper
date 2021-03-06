﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using WebApiWrapper.Models;
using WebApiWrapper.Services;

namespace WebApiWrapper.Controllers
{
    [ApiController]
    [Route("v1/github_wrapper")]
    
    public class RepositoryController : ControllerBase
    {       
        private IMemoryCache _cache;
        private IFeatureManager _featureManager;
        private readonly IConfiguration _configuration;
        private readonly IClientService _service;

        public RepositoryController(
            IMemoryCache cache,
            IFeatureManager featureManager,
            IConfiguration configuration,
            IClientService service
        )
        {
            _cache = cache;
            _featureManager = featureManager;
            _configuration = configuration;
            _service = service;
        }

        public static class FeatureFlags
        {
            public const string MemoryCache = "MemoryCache";
        }

        [HttpGet]
        [Route("{organizationName}/repos")]
        public async Task<ActionResult<List<Repository>>> GetRepositories(string organizationName)
        {
            var cacheIsEnabled = _featureManager.IsEnabledAsync(FeatureFlags.MemoryCache);
            
            if(!await cacheIsEnabled)
            {
                _cache.Remove(organizationName);
            }
            return await _cache.GetOrCreateAsync(organizationName, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(_configuration.
                    GetValue<double>("CacheManagement:SeatingSeconds"));
                entry.SetPriority(CacheItemPriority.High);
                return await _service.ClientRequest(organizationName);
            });
        }
    }
}