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
        private static readonly HttpClient _client = new HttpClient();
        
        public async Task<List<Repository>> ClientRequest(string organizationName)
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            _client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var login = $"https://api.github.com/orgs/{organizationName}/repos";
            var streamTask = _client.GetStreamAsync(login);
            var repositories = await JsonSerializer.DeserializeAsync<List<Repository>>(await streamTask);

            return repositories;
        }
    }

    public interface IClientService
    {
        Task<List<Repository>> ClientRequest(string organizationName);
    }
}