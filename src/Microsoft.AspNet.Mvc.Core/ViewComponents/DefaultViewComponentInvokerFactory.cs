// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Internal;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.ViewComponents
{
    public class DefaultViewComponentInvokerFactory : IViewComponentInvokerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITypeActivatorCache _typeActivatorCache;
        private readonly IViewComponentActivator _viewComponentActivator;

        public DefaultViewComponentInvokerFactory(
            IServiceProvider serviceProvider,
            ITypeActivatorCache typeActivatorCache,
            IViewComponentActivator viewComponentActivator,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _typeActivatorCache = typeActivatorCache;
            _viewComponentActivator = viewComponentActivator;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        // We don't currently make use of the descriptor or the arguments here (they are available on the context).
        // We might do this some day to cache which method we select, so resist the urge to 'clean' this without
        // considering that possibility.
        public IViewComponentInvoker CreateInstance(
            [NotNull] ViewComponentDescriptor viewComponentDescriptor, 
            object[] args)
        {
            return new DefaultViewComponentInvoker(
                _serviceProvider,
                _typeActivatorCache,
                _viewComponentActivator,
                _loggerFactory);
        }
    }
}
