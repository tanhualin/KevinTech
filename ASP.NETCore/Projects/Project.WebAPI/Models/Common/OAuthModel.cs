using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project.WebAPI.Models
{
    public class OAuthModel
    {
        [Required(ErrorMessage = "用户不能为空")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        public string PassWord { get; set; }
    }
}
