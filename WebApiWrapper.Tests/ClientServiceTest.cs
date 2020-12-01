using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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

        [Fact]
        public void ClientRequest_ModelValidation_ReturnsTrue()
        {
            var model = new webapi_github_wrapper.Models.Repository
            {
                Name = "",
                GitHubHomeUrl = null,
            };
            var results = ValidateModel(model);
            Assert.True(results.Any(v => v.ErrorMessage == "Required field"));
        }

        private List<ValidationResult> ValidateModel<T>(T model)
        {
            var context = new ValidationContext(model, null, null);
            var result = new List<ValidationResult>();
            var valid = Validator.TryValidateObject(model, context, result, true);
            return result;
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