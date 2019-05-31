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
    [Authorize(Roles = "Developer,Admin,ss") ]
    [Route("api/[controller]")]
    [ApiController]
    public class SmartModuleController : ControllerBaseEx
    {
        // GET: api/SmartModule
        [HttpGet]
        public IActionResult Get()
        {
            SmartHttpResult result = new SmartHttpResult();
            try
            {
                var entityList = DAL.SystemManage.SmartMenu.getModule();
                List<Models.JsonTreeNode> menuList = new List<Models.JsonTreeNode>();
                foreach (var entity in entityList)
                {
                    if (entity.ParentIdx == 0)
                    {
                        Models.JsonTreeNode node = new Models.JsonTreeNode();
                        //node.Idx = entity.Idx;
                        node.text = entity.ModuleName;
                        node.icon = entity.Icon;
                        Common.Helper.SmartMenuTreeHelper.LoadModuleTree(entityList.ToList(), node, entity.Idx);
                        menuList.Add(node);
                    }
                }
                result.Set(true, menuList);
            }
            catch (Exception err)
            {
                result.Set(false, err.Message);
            }
            return new JsonResult(result);
        }
        [Authorize(Roles = "ss")]
        // GET: api/SmartModule/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/SmartModule
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/SmartModule/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
