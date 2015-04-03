// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;
using ModelBindingWebSite.Models;

namespace ModelBindingWebSite.Controllers
{
    [Route("Validation/[Action]")]
    public class ValidationController : Controller
    {
        [FromServices]
        public ITestService ControllerService { get; set; }

        public async Task<string> CreatePhoneRecord(int id)
        {
            var phoneRecord = new PhoneRecord()
            {
               Number = id
            };

            await TryUpdateModelAsync(phoneRecord);
            if (ModelState.IsValid)
            {
                return phoneRecord.Number.ToString();
            }

            return ModelState["User"].Errors.Single().ErrorMessage;
        }

        public string GetPhoneRecord(WrapperPhoneRecord wrapperRecord)
        {
            return ModelState["Record.User"].Errors.Single().ErrorMessage;
        }

        public IEnumerable<string> GetUser(WrapperUser wrapperUser)
        {
	     return GetModelStateErrorMessages(ModelState);
	     
            //return ModelState[""].Errors.Single().ErrorMessage;
        }

        public bool SkipValidation(Resident resident)
        {
            return ModelState.IsValid;
        }

        public bool AvoidRecursive(SelfishPerson selfishPerson)
        {
            return ModelState.IsValid;
        }

        public bool DoNotValidateParameter([FromServices] ITestService service)
        {
            return ModelState.IsValid;
        }

        public IActionResult CreateRectangle([FromBody] Rectangle rectangle)
        {
            if (!ModelState.IsValid)
            {
                return new ObjectResult(GetModelStateErrorMessages(ModelState)) { StatusCode = 400 };
            }

            return new ObjectResult(rectangle);
        }

        private IEnumerable<string> GetModelStateErrorMessages(ModelStateDictionary modelStateDictionary)
        {
            var allErrorMessages = new List<string>();
            foreach (var keyModelStatePair in modelStateDictionary)
            {
                var key = keyModelStatePair.Key;
                var errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    string errorMessage = null;
                    foreach (var modelError in errors)
                    {
                        if (string.IsNullOrEmpty(modelError.ErrorMessage))
                        {
                            if (modelError.Exception != null)
                            {
                                errorMessage = modelError.Exception.Message;
                            }
                        }
                        else
                        {
                            errorMessage = modelError.ErrorMessage;
                        }

                        if (errorMessage != null)
                        {
                            allErrorMessages.Add(string.Format("{0}:{1}", key, errorMessage));
                        }
                    }
                }
            }

            return allErrorMessages;
        }
    }

    public class WrapperPhoneRecord
    {
        public int Id { get; set; }
         
        public PhoneRecord Record { get; set; }
    }

    public class WrapperUser
    {
        [FromBody]
        public int IntU { get; set; }
        public TestUser User { get; set; }
    }

    public class TestUser
    {
        [Required]
        public int IntProp { get; set; }

        [Required]
        public string StringProp { get; set; }

        public int Id { get; set; }

        [Required]
        public int MyProperty { get; set; }
    }
    
    public class SelfishPerson
    {
        public string Name { get; set; }
        public SelfishPerson MySelf { get { return this; } }
    }
}