// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Internal;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents the default <see cref="IRazorViewFactory"/> implementation that creates
    /// <see cref="RazorView"/> instances with a given <see cref="IRazorPage"/>.
    /// </summary>
    public class RazorViewFactory : IRazorViewFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IRazorPageActivator _pageActivator;
        private readonly IViewStartProvider _viewStartProvider;

        /// <summary>
        /// Initializes a new instance of RazorViewFactory
        /// </summary>
        /// <param name="pageActivator">The <see cref="IRazorPageActivator"/> used to activate pages.</param>
        /// <param name="viewStartProvider">The <see cref="IViewStartProvider"/> used for discovery of _ViewStart
        /// pages</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to log data.</param>
        public RazorViewFactory(IRazorPageActivator pageActivator,
                                IViewStartProvider viewStartProvider,
                                ILoggerFactory loggerFactory)
        {
            _pageActivator = pageActivator;
            _viewStartProvider = viewStartProvider;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public IView GetView([NotNull] IRazorViewEngine viewEngine,
                             [NotNull] IRazorPage page,
                             bool isPartial)
        {
            var razorView = new RazorView(
                viewEngine, 
                _pageActivator, 
                _viewStartProvider, 
                page, 
                _loggerFactory, 
                isPartial);

            return razorView;
        }
    }
}