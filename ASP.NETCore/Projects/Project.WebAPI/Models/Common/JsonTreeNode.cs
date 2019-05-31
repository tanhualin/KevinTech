using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.WebAPI.Models
{
    public class JsonTreeNode
    {
        /// <summary>[ModuleName]</summary>
        public virtual string text { get; set; }

        ///// <summary>[Idx]</summary>
        //public virtual int Idx { get; set; }

        /// <summary>[Link]</summary>
        public virtual string link { get; set; }

        /// <summary>[ICON]</summary>
        public virtual string icon { get; set; }
        public virtual bool group { get; set; }

        public List<JsonTreeNode> children { get; set; }
    }
}
