// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using ModelBindingWebSite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ModelBindingDataMemberRequiredTest
    {
        private const string SiteName = nameof(ModelBindingWebSite);
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;
        private readonly Action<IServiceCollection> _configureServices = new Startup().ConfigureServices;

        [Fact]
        public async Task DataMember_MissingRequiredProperty_ValidationError()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var url = "http://localhost/DataMemberRequired/EchoModelValues";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model.Property2", "Hi"),
            };

            request.Content = new FormUrlEncodedContent(formData);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<SerializableError>(body);

            Assert.Equal(1, errors.Count);

            var error = Assert.Single(errors, kvp => kvp.Key == "model.Property3");
            Assert.Equal("The 'Property3' property is required.", ((JArray)error.Value)[0].Value<string>());
        }

        [Fact]
        public async Task DataMember_RequiredPropertyProvided_Success()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var url = "http://localhost/DataMemberRequired/EchoModelValues";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model.Property1", "Hello"),
                new KeyValuePair<string, string>("model.Property2", "World!"),
                new KeyValuePair<string, string>("model.Property3", "Required!"),
            };

            request.Content = new FormUrlEncodedContent(formData);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<DataMemberRequiredModel>(body);

            Assert.Equal("Hello", model.Property1);
            Assert.Equal("World!", model.Property2);
            Assert.Equal("Required!", model.Property3);
        }
    }
}