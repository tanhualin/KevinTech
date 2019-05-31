using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.JwtBearer.Common.Mvc
{
    public class ControllerBaseEx : ControllerBase
    {
        public ControllerBaseEx() : base()
        {
        }
        public JsonResult JsonEx(object data)
        {
            return new JsonResult(data);
        }
    }
}
