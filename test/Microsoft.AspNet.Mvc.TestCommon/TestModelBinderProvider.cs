// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    internal class TestModelBinderProvider
    {
        // Creates a provider with all the defaults.
        public static IModelBinder CreateDefaultModelBinder()
        {
            return new CompositeModelBinder(new IModelBinder[]
            {
                new BinderTypeBasedModelBinder(),
                new ServicesModelBinder(),
                new BodyModelBinder(),
                new HeaderModelBinder(),
                new TypeConverterModelBinder(),
                new TypeMatchModelBinder(),
                new CancellationTokenModelBinder(),
                new ByteArrayModelBinder(),
                new FormFileModelBinder(),
                new FormCollectionModelBinder(),
                new GenericModelBinder(),
                new MutableObjectModelBinder(),
                new ComplexModelDtoModelBinder(),
            });
        }
    }
}