using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using webapi_github_wrapper.Models;

namespace webapi_github_wrapper.Services
{
    public class ClientService : IClientService
    {
        private readonly HttpClient _client;

        public ClientService(HttpClient httpClient)
        {
            _client = httpClient;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            _client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
        }

        public async Task<List<Repository>> ClientRequest(string organizationName)
        {
            var login = $"https://api.github.com/orgs/{organizationName}/repos";
            var streamTask = _client.GetStreamAsync(login);
            return await JsonSerializer.DeserializeAsync<List<Repository>>(await streamTask);
        }
    }

    public interface IClientService
    {
        Task<List<Repository>> ClientRequest(string organizationName);
    }
}