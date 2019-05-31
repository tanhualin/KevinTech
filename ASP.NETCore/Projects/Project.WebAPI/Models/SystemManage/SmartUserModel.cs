using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.WebAPI.Models.SystemManage
{
    public partial class SmartUserModel
    {
        public virtual int Idx { get; set; }
        /// <summary>用户名</summary>
        public virtual string UserName { get; set; }

        /// <summary>密码</summary>
        public virtual string PassWord { get; set; }

        /// <summary>密钥盐</summary>
        public virtual string Salt { get; set; }

        /// <summary>[Email]</summary>
        public virtual string Email { get; set; }

        /// <summary>[Phone]</summary>
        public virtual string Phone { get; set; }

        /// <summary>图片地址</summary>
        public virtual string Avatar { get; set; }

        /// <summary>[State]</summary>
        public virtual int State { get; set; }

        /// <summary>[CreateOn]</summary>
        public virtual DateTime CreateOn { get; set; }

        /// <summary>[CreateBy]</summary>
        public virtual string CreateBy { get; set; }

        /// <summary>[LastModifyOn]</summary>
        public virtual DateTime LastModifyOn { get; set; }

        /// <summary>[LastModifyBy]</summary>
        public virtual string LastModifyBy { get; set; }

    }
}
