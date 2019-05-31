using Mvc.JwtBearer.Models.SystemManage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Mvc.JwtBearer.DAL
{
    public class SmartUser : BaseDAL
    {
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
    }
}
