using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Library.IntergrationTest
{
    public class UnitTest1 : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public HttpClient client { get; set; }
        public UnitTest1(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }
        [Fact]
        public async Task Test1()
        {
            string _ContentType = "application/json";
            client = _factory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_ContentType));
            client.BaseAddress = new Uri("http://localhost:5000");

            // Act
            var response = await client.GetAsync(client.BaseAddress+ "/api/Authors");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json",
                response.Content.Headers.ContentType.ToString());
        }
    }
}
