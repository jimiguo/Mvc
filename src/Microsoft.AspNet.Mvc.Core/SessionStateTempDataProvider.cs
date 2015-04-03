// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Provides session-state data to the current <see cref="ITempDataDictionary"/> object.
    /// </summary>
    public class SessionStateTempDataProvider : ITempDataProvider
    {
        private const string TempDataSessionStateKey = "__ControllerTempData";
        private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

        /// <inheritdoc />
        public virtual IDictionary<string, object> LoadTempData([NotNull] HttpContext context)
        {
            if (!IsSessionEnabled(context))
            {
                // Session middleware is not enabled. No-op
                return null;
            }

            var tempDataDictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var session = context.Session;
            byte[] value;

            if (session != null && session.TryGetValue(TempDataSessionStateKey, out value))
            {
                using (var memoryStream = new MemoryStream(value))
                using (var writer = new BsonReader(memoryStream))
                {
                    tempDataDictionary = _jsonSerializer.Deserialize<Dictionary<string, object>>(writer);
                }

                // If we got it from Session, remove it so that no other request gets it
                session.Remove(TempDataSessionStateKey);
            }
            else
            {
                // Since we call Save() after the response has been sent, we need to initialize an empty session
                // so that it is established before the headers are sent.
                session[TempDataSessionStateKey] = new byte[] { };
            }

            return tempDataDictionary;
        }

        /// <inheritdoc />
        public virtual void SaveTempData([NotNull] HttpContext context, IDictionary<string, object> values)
        {
            var hasValues = (values != null && values.Count > 0);
            if (hasValues)
            {
                foreach (var item in values.Values)
                {
                    // We want to allow only simple types to be serialized in session.
                    EnsureObjectCanBeSerialized(item);
                }

                // Accessing Session property will throw if the session middleware is not enabled.
                var session = context.Session;
                
                using (var memoryStream = new MemoryStream())
                using (var writer = new BsonWriter(memoryStream))
                {
                    _jsonSerializer.Serialize(writer, values);
                    session[TempDataSessionStateKey] = memoryStream.ToArray();
                }
            }
            else if (IsSessionEnabled(context))
            {
                var session = context.Session;
                session.Remove(TempDataSessionStateKey);
            }
        }

        private static bool IsSessionEnabled(HttpContext context)
        {
            return context.GetFeature<ISessionFeature>() != null;
        }

        internal void EnsureObjectCanBeSerialized(object item)
        {
            var itemType = item.GetType();
            Type[] actualTypes = null;

            if (itemType.IsArray)
            {
                itemType = itemType.GetElementType();
            }
            else if (itemType.GetTypeInfo().IsGenericType)
            {
                if (itemType.ExtractGenericInterface(typeof(IList<>)) != null ||
                    itemType.ExtractGenericInterface(typeof(IDictionary<,>)) != null)
                {
                    actualTypes = itemType.GetGenericArguments();
                }
            }

            actualTypes = actualTypes ?? new Type[] { itemType };

            foreach (var actualType in actualTypes)
            {
                var underlyingType = Nullable.GetUnderlyingType(actualType) ?? actualType;
                if (!TypeHelper.IsSimpleType(actualType))
                {
                    var message = Resources.FormatTempData_CannotSerializeToSession(underlyingType,
                        typeof(SessionStateTempDataProvider).FullName);
                    throw new InvalidOperationException(message);
                }
            }
        }
    }
}