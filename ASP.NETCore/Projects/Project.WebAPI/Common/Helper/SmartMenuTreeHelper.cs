using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.WebAPI.Common.Helper
{
    public class SmartMenuTreeHelper
    {
        public static void LoadTree(List<Models.SystemManage.SmartMenuModel> all, Models.JsonTreeNode menuNode, long Idx)
        {
            var linq = from child in all where child.ParentIdx == Idx orderby child.SortOrder select child;
            menuNode.children = new List<Models.JsonTreeNode>();
            foreach (var entity in linq)
            {
                Models.JsonTreeNode node = new Models.JsonTreeNode();
                //node.Idx = entity.Idx;
                node.text = entity.ModuleName;
                node.link = entity.Link;
                node.icon = entity.Icon;
                LoadTree(all, node, entity.Idx);
                menuNode.group = true;
                menuNode.children.Add(node);
            }
        }

        public static void LoadModuleTree(List<Models.SystemManage.SmartMenuModel> all, Models.JsonTreeNode menuNode, long Idx)
        {
            var linq = from child in all where child.ParentIdx == Idx orderby child.SortOrder select child;
            menuNode.children = new List<Models.JsonTreeNode>();
            foreach (var entity in linq)
            {
                Models.JsonTreeNode node = new Models.JsonTreeNode();
                //node.Idx = entity.Idx;
                node.text = entity.ModuleName;
                node.icon = entity.Icon;
                LoadTree(all, node, entity.Idx);
                menuNode.group = true;
                menuNode.children.Add(node);
            }
        }
    }
}
