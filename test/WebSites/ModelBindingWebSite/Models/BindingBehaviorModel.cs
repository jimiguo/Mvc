// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ModelBinding;

namespace ModelBindingWebSite
{
    public class BindingBehaviorModel
    {
        [BindingBehavior(BindingBehavior.Never)]
        public string Property1 { get; set; }

        [BindingBehavior(BindingBehavior.Optional)]
        public string Property2 { get; set; }

        [BindingBehavior(BindingBehavior.Required)]
        public string Property3 { get; set; }

        [BindRequired]
        public string Property4 { get; set; }

        [BindNever]
        public string Property5 { get; set; }
    }
}
