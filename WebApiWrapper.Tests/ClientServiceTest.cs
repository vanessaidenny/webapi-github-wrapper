using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using webapi_github_wrapper.Services;
using Xunit;

namespace WebApiWrapper.Tests
{
    public class ClientServiceTest
    {
        [Fact]
        public async void ClientRequest_ShouldReturnResponse()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var assembly = Assembly.GetExecutingAssembly();
            var jsonData = assembly.GetManifestResourceStream("WebApiWrapper.Tests.repos.json");
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(jsonData),
            };
            
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var clientService = new ClientService(httpClient);

            var retrievedPosts = await clientService.ClientRequest("dotnet");

            Assert.NotNull(retrievedPosts);
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
               ItExpr.IsAny<CancellationToken>());
        }
    }
}
