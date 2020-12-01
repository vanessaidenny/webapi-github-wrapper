using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace WebApiWrapper.Tests
{
    public class ModelValidationTest
    {
        
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
    }
}