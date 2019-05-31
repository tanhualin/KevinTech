using Dapper;
using Project.WebAPI.Models.SystemManage;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Project.WebAPI.DAL.SystemManage
{
    public class SmartRole : BaseDAL
    {
        public static List<string> GetRolesByUserIdx(int userIdx)
        {
            using (IDbConnection sqlconn = new SqlConnection(SqlConnString))
            {
                return sqlconn.Query<string>(@"SELECT r.RoleCode
                                            FROM dbo.SmartRole r
                                            JOIN dbo.SmartUserInRole ur ON r.Idx = ur.RoleIdx
                                            WHERE ur.UserIdx = @userIdx", new { UserIdx = userIdx }).AsList();
            }
        }
    }
}
