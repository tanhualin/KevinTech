using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.WebAPI.Common.Mvc;

namespace Project.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBaseEx
    {
        [HttpGet]
        public IActionResult LoadMenu()
        {
            SmartHttpResult result = new SmartHttpResult();
            try
            {
                var entityList = DAL.SystemManage.SmartMenu.getMenuByUserName(HttpContext.User.Identity.Name);
                List<Models.JsonTreeNode> menuList = new List<Models.JsonTreeNode>();
                foreach (var entity in entityList)
                {
                    if (entity.ParentIdx == null)
                    {
                        Models.JsonTreeNode node = new Models.JsonTreeNode();
                        //node.Idx = entity.Idx;
                        node.text = entity.ModuleName;
                        node.link = entity.Link;
                        node.icon = entity.Icon;
                        Common.Helper.SmartMenuTreeHelper.LoadTree(entityList.ToList(), node, entity.Idx);
                        menuList.Add(node);
                    }
                }
                result.Set(true, menuList);
            }
            catch (Exception err)
            {
                result.Set(false, err.Message);
            }
            return JsonEx(result);
        }
    }
}