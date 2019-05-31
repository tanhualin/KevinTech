using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.JwtBearer.Models.Common
{
    public class JwtTokenModel
    {
        public string Token { get; set; }
        public TokenUserModel User { get; set; }
    }

    public class TokenUserModel
    {
        /// <summary>用户名</summary>
        public virtual string UserName { get; set; }
        /// <summary>[Email]</summary>
        public virtual string Email { get; set; }
        /// <summary>[Phone]</summary>
        public virtual string Phone { get; set; }
        /// <summary>图片地址</summary>
        public virtual string Avatar { get; set; }
    }
}
