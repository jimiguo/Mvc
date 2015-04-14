// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.Serialization;

namespace ModelBindingWebSite
{
    [DataContract]
    public class DataMemberRequiredModel
    {
        [DataMember]
        public string Property1 { get; set; }

        [DataMember(IsRequired = false)]
        public string Property2 { get; set; }

        [DataMember(IsRequired = true)]
        public string Property3 { get; set; }
    }
}
