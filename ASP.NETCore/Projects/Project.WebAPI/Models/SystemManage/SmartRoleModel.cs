using System;
using System.Collections.Generic;

namespace Project.WebAPI.Models.SystemManage
{

    /// <summary>Table: SmartRole</summary>
    public partial class SmartRoleModel
    {

        /// <summary>角色</summary>
        public virtual string RoleName { get; set; }

        /// <summary>[Idx]</summary>
        public virtual int Idx { get; set; }

        /// <summary>上级角色Id</summary>
        public virtual int ParentIdx { get; set; }

        /// <summary>角色类型代码</summary>
        public virtual string RoleCode { get; set; }

        /// <summary>[pgRole]</summary>
        public virtual bool PgRole { get; set; }

        /// <summary>[Description]</summary>
        public virtual string Description { get; set; }

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

    }
}