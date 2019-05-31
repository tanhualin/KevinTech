using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.WebAPI.Models.SystemManage
{
    public partial class SmartModule
    {

        /// <summary>[ModuleName]</summary>
        public virtual string ModuleName { get; set; }

        /// <summary>[Idx]</summary>
        public virtual int Idx { get; set; }

        /// <summary>[ParentIdx]</summary>
        public virtual int ParentIdx { get; set; }

        /// <summary>[Description]</summary>
        public virtual string Description { get; set; }

        /// <summary>[ICON]</summary>
        public virtual string Icon { get; set; }

        /// <summary>[SortOrder]</summary>
        public virtual int SortOrder { get; set; }

        /// <summary>[State]</summary>
        public virtual int State { get; set; }

        /// <summary>[CreateOn]</summary>
        public virtual System.DateTime CreateOn { get; set; }

        /// <summary>[CreateBy]</summary>
        public virtual string CreateBy { get; set; }

        /// <summary>[LastModifyOn]</summary>
        public virtual System.DateTime LastModifyOn { get; set; }

        /// <summary>[LastModifyBy]</summary>
        public virtual string LastModifyBy { get; set; }

        /// <summary>[PermissionIdx]</summary>
        public virtual int PermissionIdx { get; set; }

    }

    public partial class SmartMenuModel
    {
        /// <summary>[ModuleName]</summary>
        public virtual string ModuleName { get; set; }

        /// <summary>[Idx]</summary>
        public virtual int Idx { get; set; }

        /// <summary>[ParentIdx]</summary>
        public virtual int? ParentIdx { get; set; }

        /// <summary>[Link]</summary>
        public virtual string Link { get; set; }

        public virtual int OpIdx { get; set; }
        /// <summary>[ICON]</summary>
        public virtual string Icon { get; set; }

        /// <summary>[SortOrder]</summary>
        public virtual int SortOrder { get; set; }
    }
}
