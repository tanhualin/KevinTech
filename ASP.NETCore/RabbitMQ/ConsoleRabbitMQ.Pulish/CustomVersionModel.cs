using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRabbitMQ.Pulish
{
    public class CustomVersionModel
    {
        public string version { get; set; }
        public string preVersion { get; set; }
        public string verDescription { get; set; }
        public string verAddress { get; set; }
        public bool isNeed { get; set; }
    }
}
