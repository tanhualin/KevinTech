using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.WebAPI.Common.Mvc
{
    public class SmartHttpResult
    {
        public bool status { get; set; }
        public string msg { get; set; }
        public object data { get; set; }

        public void Set(bool status, string message, object data)
        {
            this.status = status;
            this.msg = message;
            this.data = data;
        }

        public void Set(bool status, string message)
        {
            this.status = status;
            this.msg = message;
        }

        public void Set(bool status, object data)
        {
            this.status = status;
            this.data = data;
        }

        public void Set(bool status)
        {
            this.status = status;
        }
    }
}
