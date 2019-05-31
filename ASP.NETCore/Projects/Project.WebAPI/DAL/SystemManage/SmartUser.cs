

using Project.WebAPI.Models.SystemManage;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace Project.WebAPI.DAL.SystemManage
{
    public class SmartUser : BaseDAL
    {
        public static SmartUserModel GetEntityByName(string userName)
        {
            using (IDbConnection sqlconn = new SqlConnection(SqlConnString))
            {
                return sqlconn.QueryFirst<SmartUserModel>("SELECT * FROM dbo.SmartUser WHERE UserName=@userName", new { UserName = userName });
            }
        }

        public static void utlSmartUserByName(string userName, string passWord, string salt)
        {
            using (var sqlconn = new SqlConnection(SqlConnString))
            {
                sqlconn.Execute("UPDATE dbo.SmartUser SET [PassWord]=@PassWord,Salt=@Salt WHERE UserName=@UserName", new { PassWord = passWord, Salt = salt, UserName = userName });
            }
        }

        public static List<SmartUserModel> getUserList()
        {
            using (var sqlconn = new SqlConnection(SqlConnString))
            {
                return sqlconn.Query<SmartUserModel>("SELECT * FROM dbo.SmartUser").AsList();
            }
        }

        /*  使用传统的ADO.NET 
        public static SmartUserModel GetEntityByName(string userName)
        {
            using (var sqlconn = new SqlConnection(SqlConnString))
            {
                sqlconn.Open();
                var cmd = sqlconn.CreateCommand();
                cmd.CommandText = "dbo.spAPP_GetSmartUserByName";
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
                if (table != null && table.Rows.Count > 0)
                {
                    return Common.Helper.SmartCommonHelper.GetEntity<SmartUserModel>(table);
                }
                return null;
            }
        }

        public static void utlSmartUserByName(string userName, string passWord, string Salt)
        {
            using (var sqlconn = new SqlConnection(SqlConnString))
            {
                sqlconn.Open();
                var cmd = sqlconn.CreateCommand();
                cmd.CommandText = "dbo.spAPP_utlSmartUserByName";
                cmd.CommandType = CommandType.StoredProcedure;

                var param = cmd.CreateParameter();
                param.ParameterName = "@userName";
                param.SqlDbType = SqlDbType.NVarChar;
                param.Size = 50;
                param.Direction = ParameterDirection.Input;
                param.Value = userName;
                cmd.Parameters.Add(param);

                param = cmd.CreateParameter();
                param.ParameterName = "@passWord";
                param.SqlDbType = SqlDbType.NVarChar;
                param.Size = 50;
                param.Direction = ParameterDirection.Input;
                param.Value = passWord;
                cmd.Parameters.Add(param);

                param = cmd.CreateParameter();
                param.ParameterName = "@Salt";
                param.SqlDbType = SqlDbType.NVarChar;
                param.Size = 32;
                param.Direction = ParameterDirection.Input;
                param.Value = Salt;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
        }

        public static IList<SmartUserModel> getUserList()
        {
            using (var sqlconn = new SqlConnection(SqlConnString))
            {
                sqlconn.Open();
                var cmd = sqlconn.CreateCommand();
                cmd.CommandText = "dbo.spAPP_GetSmartUserList";
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adp = new SqlDataAdapter();
                adp.SelectCommand = cmd;
                DataTable table = new DataTable();
                adp.Fill(table);

                return Common.Helper.SmartCommonHelper.GetEntities<SmartUserModel>(table);
            }
        }
        */
    }
}
