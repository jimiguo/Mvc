// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Mvc;

namespace ModelBindingWebSite.Models
{
    public class PhoneRecord
    {
        public int Number { get; set; }

        [FromBody]
        [Required]
        public User User { get; set; }
    }
}