// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.Core
{
    public class TestMockMvcOptionsAccessor : IOptions<MvcOptions>
    {
        public TestMockMvcOptionsAccessor()
        {
            Options = new MvcOptions();
        }

        public MvcOptions Options { get; }

        public MvcOptions GetNamedOptions(string name)
        {
            throw new NotImplementedException();
        }
    }
}