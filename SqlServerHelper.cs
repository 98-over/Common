using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
namespace Common
{
    public static class SqlServerHelper
    {

        public static string conStr = "Data Source=localhost;Initial Catalog=mooc;Persist Security Info=True;User ID=sa;Password=123456";

        public static DataSet GetDataSet( )
        {
            SqlConnection conn = new SqlConnection(conStr);
            
            try
            {
                //打开数据库
                conn.Open();
                SqlDataAdapter myDa = new SqlDataAdapter("select course_id,course_name,course_picpath,course_introduce from course ", conn);
                DataSet myDs = new DataSet();
                myDa.Fill(myDs);
                return myDs;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public static int Insert(string sql, params SqlParameter[] param)
        {


            using (SqlConnection conn = new SqlConnection(conStr))
            {

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (param != null && param.Length > 0)
                    {

                        cmd.Parameters.AddRange(param);
                    }

                    conn.Open();

                    int row = cmd.ExecuteNonQuery();

                    return row;
                }


            }


        }

        public static SqlDataReader GetReader(string sql, params SqlParameter[] param)
        {
            SqlConnection conn = new SqlConnection(conStr);
            SqlCommand cmd = new SqlCommand(sql, conn);
            if (param != null && param.Length > 0)
            {
                //添加参数
                cmd.Parameters.AddRange(param);
            }
            try
            {
                //打开数据库
                conn.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public static DataSet GetDataSetById(string sql,string id)
        {
            SqlConnection conn = new SqlConnection(conStr);

            try
            {
                //打开数据库
                conn.Open();
                SqlDataAdapter myDa = new SqlDataAdapter(sql+id, conn);
                DataSet myDs = new DataSet();
                myDa.Fill(myDs);
                return myDs;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

    }

}