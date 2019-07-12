using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSight.CustomService.Models
{
    public class CustomVersionModel
    {
        public string version { get; set; }
        public string verFilePath { get; set; }
        public string verFileExt { get; set; }
        public bool isNeed { get; set; }
        public int verIdx { get; set; }
        public object Path { get; set; }

        protected bool Equals(CustomVersionModel other)
        {
            return this.version == other.version;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return this.version.GetHashCode();
            }
        }
    }
}
