// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

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
                TypeNameHandling = TypeNameHandling.None
            });

        private static readonly MethodInfo _convertArrayMethodInfo = typeof(SessionStateTempDataProvider).GetMethod(
            nameof(ConvertArray), BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(JArray) }, null);

        private readonly ConcurrentDictionary<Type, Func<JArray, object>> _arrayConverters =
            new ConcurrentDictionary<Type, Func<JArray, object>>();

        private static Dictionary<JTokenType, Type> _arrayTypeLookup = new Dictionary<JTokenType, Type>
        {
            { JTokenType.String, typeof(string) },
            { JTokenType.Integer, typeof(int) },
            { JTokenType.Boolean, typeof(bool) },
            { JTokenType.Float, typeof(float) },
            { JTokenType.Guid, typeof(Guid) },
            { JTokenType.Object, typeof(object) },
            { JTokenType.Date, typeof(DateTime) },
            { JTokenType.TimeSpan, typeof(TimeSpan) },
            { JTokenType.Uri, typeof(Uri) },
        };

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
                foreach (var item in tempDataDictionary.ToList())
                {
                    var jArrayValue = item.Value as JArray;
                    if (jArrayValue != null && jArrayValue.Count > 0)
                    {
                        Type returnType = null;
                        _arrayTypeLookup.TryGetValue(jArrayValue[0].Type, out returnType);
                        if (returnType != null)
                        {
                            var arrayConverter = _arrayConverters.GetOrAdd(returnType, type =>
                            {
                                return (Func<JArray, object>)Delegate.CreateDelegate(typeof(Func<JArray, object>),
                                    _convertArrayMethodInfo.MakeGenericMethod(type));
                            });
                            var result = arrayConverter(jArrayValue);

                            tempDataDictionary[item.Key] = result;
                        }
                    }
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
                if (itemType.ExtractGenericInterface(typeof(IList<>)) != null)
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

        private static IList<TVal> ConvertArray<TVal>(JArray array)
        {
            return array.Values<TVal>().ToArray();
        }
    }
}