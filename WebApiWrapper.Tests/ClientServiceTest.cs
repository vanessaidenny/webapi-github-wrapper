using System.IO;
using System.Net;
using System.Net.Http;
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
        private static Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>();
        private ClientService clientService = new ClientService(
            new HttpClient(handlerMock.Object)
        );
        
        [Fact]
        public async void ClientRequest_ShouldReturnResponse()
        {
            GetFakeHttpClient(HttpStatusCode.OK);
            var retrievedPosts = await clientService.ClientRequest("dotnet");

            Assert.NotNull(retrievedPosts);
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
               ItExpr.IsAny<CancellationToken>());
        }

        public static Mock<HttpMessageHandler> GetFakeHttpClient(HttpStatusCode statusCode)
        {
            string jsondata = File.ReadAllText("../../../Data/repos.json");
            
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(jsondata),
            });

            return handlerMock;
        }
    }
}