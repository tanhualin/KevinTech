using Microsoft.AspNetCore.Mvc;

namespace Project.WebAPI.Common.Mvc
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
