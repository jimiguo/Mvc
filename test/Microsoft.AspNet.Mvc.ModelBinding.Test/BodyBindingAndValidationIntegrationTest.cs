// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class BodyBindingAndIntegrationTests
    {
        private class Person
        {
            [FromBody]
            [Required]
            public Address Address { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
        }

        [Fact]
        public async Task BodyBoundOnProperty_RequiredOnProperty()
        {
            var argumentBinder = GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo()
                {
                    BinderModelName = "CustomParameter",
                },
                ParameterType = typeof(Person)
            };

            var operationContext = new OperationBindingContext()
            {
                BodyBindingState = BodyBindingState.NotBodyBased,
                HttpContext = GetHttpContext(string.Empty),
                MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                ValidatorProvider = TestModelValidatorProvider.CreateDefaultProvider(),
                ValueProvider = new TestValueProvider(new Dictionary<string, object>()),
                ModelBinder  = TestModelBinderProvider.CreateDefaultModelBinder()
            };

            var modelState = new ModelStateDictionary();
            var model = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            Assert.Equal("", modelState["Address"].Errors.Single().ErrorMessage);
        }

        private HttpContext GetHttpContext(string jsonContent)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            httpContext.Request.ContentType = "application/json";

            var services = new Mock<IServiceProvider>(MockBehavior.Strict);
            httpContext.RequestServices = services.Object;
            var mockActionContextAccessor = new Mock<IScopedInstance<ActionContext>>();
            mockActionContextAccessor.SetupGet(o => o.Value)
                .Returns(GetActionContext());
            services.Setup(s => s.GetService(typeof(IScopedInstance<ActionContext>)))
               .Returns(mockActionContextAccessor.Object);

            var mockActionBindingContextAccessor = new Mock<IScopedInstance<ActionBindingContext>>();
            mockActionBindingContextAccessor.SetupGet(o => o.Value)
                .Returns(GetActionBindingContext());
            services.Setup(s => s.GetService(typeof(IScopedInstance<ActionBindingContext>)))
              .Returns(mockActionBindingContextAccessor.Object);
            return httpContext;
        }

        private static ActionContext GetActionContext(ActionDescriptor descriptor = null)
        {
            return new ActionContext(
                 new DefaultHttpContext(),
                 new RouteData(),
                 descriptor ?? GetActionDescriptor());
        }

        private static ActionDescriptor GetActionDescriptor()
        {
            Func<object, int> method = foo => 1;
            return new ControllerActionDescriptor
            {
                MethodInfo = method.Method,
                ControllerTypeInfo = typeof(BodyBindingAndIntegrationTests).GetTypeInfo(),
                BoundProperties = new List<ParameterDescriptor>(),
                Parameters = new List<ParameterDescriptor>()
            };
        }

        private static ActionBindingContext GetActionBindingContext()
        {
            var binder = new Mock<IModelBinder>();
            binder
                .Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns(Task.FromResult(
                    result: new ModelBindingResult(model: "Hello", key: string.Empty, isModelSet: true)));
            var options = new TestMockMvcOptionsAccessor();
            options.Options.InputFormatters.Add(new JsonInputFormatter());
            options.Options.InputFormatters.Add(new JsonPatchInputFormatter());
            return new ActionBindingContext()
            {
                InputFormatters = options.Options.InputFormatters
            };
        }

        public DefaultControllerActionArgumentBinder GetArgumentBinder(IObjectModelValidator validator = null)
        {
            var options = new TestMockMvcOptionsAccessor();
            options.Options.MaxModelValidationErrors = 5;

            if (validator == null)
            {
                var mockValidator = new Mock<IObjectModelValidator>(MockBehavior.Strict);
                mockValidator.Setup(o => o.Validate(It.IsAny<ModelValidationContext>()));
                validator = mockValidator.Object;
            }

            return new DefaultControllerActionArgumentBinder(
                TestModelMetadataProvider.CreateDefaultProvider(),
                validator,
                options);
        }
    }
}