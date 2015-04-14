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
    public class ModelBindingBindingBehaviorTest
    {
        private const string SiteName = nameof(ModelBindingWebSite);
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;
        private readonly Action<IServiceCollection> _configureServices = new Startup().ConfigureServices;

        [Fact]
        public async Task BindingBehavior_MissingRequiredProperties_ValidationErrors()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var url = "http://localhost/BindingBehavior/EchoModelValues";
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

            Assert.Equal(2, errors.Count);

            var error = Assert.Single(errors, kvp => kvp.Key == "model.Property3" );
            Assert.Equal("The 'Property3' property is required.", ((JArray)error.Value)[0].Value<string>());

            error = Assert.Single(errors, kvp => kvp.Key == "model.Property4");
            Assert.Equal("The 'Property4' property is required.", ((JArray)error.Value)[0].Value<string>());
        }

        [Fact]
        public async Task BindingBehavior_OptionalIsOptional()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var url = "http://localhost/BindingBehavior/EchoModelValues";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("model.Property3", "Hello"),
                new KeyValuePair<string, string>("model.Property4", "World!"),
            };

            request.Content = new FormUrlEncodedContent(formData);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<BindingBehaviorModel>(body);

            Assert.Null(model.Property1);
            Assert.Null(model.Property2);
            Assert.Equal("Hello", model.Property3);
            Assert.Equal("World!", model.Property4);
            Assert.Null(model.Property5);
        }

        [Fact]
        public async Task BindingBehavior_Never_IsNotBound()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var url = "http://localhost/BindingBehavior/EchoModelValues";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var formData = new List<KeyValuePair<string, string>>
            {

                new KeyValuePair<string, string>("model.Property1", "Ignored"),
                new KeyValuePair<string, string>("model.Property2", "Optional"),
                new KeyValuePair<string, string>("model.Property3", "Hello"),
                new KeyValuePair<string, string>("model.Property4", "World!"),
                new KeyValuePair<string, string>("model.Property5", "Ignored"),
            };

            request.Content = new FormUrlEncodedContent(formData);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<BindingBehaviorModel>(body);

            Assert.Null(model.Property1);
            Assert.Equal("Optional", model.Property2);
            Assert.Equal("Hello", model.Property3);
            Assert.Equal("World!", model.Property4);
            Assert.Null(model.Property5);
        }
    }
}