﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;

namespace InlineConstraintsWebSite.Constraints
{
    public class IsbnDigitScheme13Constraint : IRouteConstraint
    {
        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            IDictionary<string, object> values,
            RouteDirection routeDirection)
        {
            object value;

            if (!values.TryGetValue(routeKey, out value))
            {
                return false;
            }

            var isbnNumber = value as string;

            if (isbnNumber == null
                || isbnNumber.Length != 13
                || isbnNumber.Any(n => !char.IsDigit(n)))
            {
                return false;
            }

            var sum = 0;
            var multipliedBy = new int[] { 1, 3 };
            Func<char, int> convertToInt = (char n) => (int)char.GetNumericValue(n);

            for (int i = 0; i < isbnNumber.Length - 1; ++i)
            {
                sum +=
                    convertToInt(isbnNumber[i]) * multipliedBy[i % 2];
            }

            var checkSum = 10 - sum % 10;

            if (checkSum == convertToInt(isbnNumber.Last()))
            {
                return true;
            }

            return false;
        }
    }
}