using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Project.WebAPI.DAL.SystemManage
{
    public class SmartMenu:BaseDAL
    {
        public static List<Models.SystemManage.SmartMenuModel> getMenuByUserName(string userName)
        {
            using (IDbConnection sqlconn = new SqlConnection(SqlConnString))
            {
                var param = new DynamicParameters();
                param.Add("@UserName", userName);

                return sqlconn.Query<Models.SystemManage.SmartMenuModel>("[dbo].[spAPP_getMenuByUserName]", param, null, true, null, CommandType.StoredProcedure).AsList();
            }
        }
        public static IList<Models.SystemManage.SmartMenuModel> getModule()
        {
            using (var sqlconn = new SqlConnection(SqlConnString))
            {
                return sqlconn.Query<Models.SystemManage.SmartMenuModel>("SELECT * FROM dbo.SmartModule").AsList();
            }
        }

        /*  使用传统的ADO.NET 
        public static IList<Models.SystemManage.SmartMenu> getMenuByUserName(string userName)
        {
            using (var sqlconn = new SqlConnection(SqlConnString))
            {
                sqlconn.Open();
                var cmd = sqlconn.CreateCommand();
                cmd.CommandText = "dbo.spAPP_getMenuByUserName";
                cmd.CommandType = CommandType.StoredProcedure;

                var param = cmd.CreateParameter();
                param.ParameterName = "@userName";
                param.SqlDbType = SqlDbType.NVarChar;
                param.Size = 50;
                param.Direction = ParameterDirection.Input;
                param.Value = userName;
                cmd.Parameters.Add(param);

                SqlDataAdapter adp = new SqlDataAdapter();
                adp.SelectCommand = cmd;
                DataTable table = new DataTable();
                adp.Fill(table);
                return Common.Helper.SmartCommonHelper.GetEntities<Models.SystemManage.SmartMenu>(table);
            }
        }

        public static IList<Models.SystemManage.SmartMenu> getModule()
        {
            using (var sqlconn = new SqlConnection(SqlConnString))
            {
                sqlconn.Open();
                var cmd = sqlconn.CreateCommand();
                cmd.CommandText = "dbo.spAPP_getModuleList";
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adp = new SqlDataAdapter();
                adp.SelectCommand = cmd;
                DataTable table = new DataTable();
                adp.Fill(table);
                return Common.Helper.SmartCommonHelper.GetEntities<Models.SystemManage.SmartMenu>(table);
            }
        }
        */
    }
}
