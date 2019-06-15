using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Helpers
{
    public class UnprocessableEntityObjectResult : ObjectResult
    {
        public UnprocessableEntityObjectResult(ModelStateDictionary modelstate) : base(new SerializableError(modelstate))
        {
            if (modelstate == null)
            {
                throw new ArgumentNullException(nameof(modelstate));
            }
            StatusCode = 422;

        }
    }
}
