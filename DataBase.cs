using System;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Data;
using System.Web.Configuration;

public class DataBase : IDisposable
{
    #region 两层架构：操作数据库
    // ExecSQL函数用来执行sql语句
    public Boolean ExecSQL(string sQueryString)
    {
        string conString = WebConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
        SqlConnection con = new SqlConnection(conString);

        con.Open();
        SqlCommand dbCommand = new SqlCommand(sQueryString, con);
        try
        {
            dbCommand.ExecuteNonQuery();
            con.Close();
        }
        catch
        {
            con.Close();
            return false;
        }
        return true;
    }

    //GetDataSet函数返回数据源的数据集    
    public System.Data.DataSet GetDataSet(string sQueryString, string TableName)
    {
        string conString = WebConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
        SqlConnection con = new SqlConnection(conString);

        con.Open();
        SqlDataAdapter dbAdapter = new SqlDataAdapter(sQueryString, con);
        DataSet dataset = new DataSet();
        dbAdapter.Fill(dataset, TableName);
        con.Close();
        return dataset;
    }
    #endregion

    #region 三层架构：建立数据库连接、关闭数据库连接、释放数据库连接资源
    private string conString = WebConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
    private SqlConnection con;
    //打开数据库连接
    private void Open()
    {
        if (con == null)
        {
            con = new SqlConnection(conString);

        }
        if (con.State == System.Data.ConnectionState.Closed)
            con.Open();
    }

    //关闭数据库连接
    private void Close()
    {
        if (con != null)
            con.Close();
    }

    //释放资源
    public void Dispose()
    {
        //确认连接是否已经关闭，释放资源
        if (con != null)
        {
            con.Dispose();
            con = null;
        }
    }
    #endregion
    #region 传入参数并转换为SqlParameter类型
    //转换参数
    //<param name="ParamName">存储过程名称或命令文本</param>
    //<param name="DbType">参数类型</param>
    //<param name="Size">参数大小</param>
    //<param name="Value">参数值</param>
    //<returns>新的parameter对象</returns>
    public SqlParameter MakeInParam(string ParamName, SqlDbType DbType, int Size, object Value)
    {
        return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
    }

    //初始化参数值
    //<param name="ParamName">存储过程名称或命令文本</param>
    //<param name="DbType">参数类型</param>
    //<param name="Size">参数大小</param>
    //<param name="Direction">参数方向</param>
    //<param name="Value">参数值</param>
    //<returns>新的parameter对象</returns>
    public SqlParameter MakeParam(string ParamName, SqlDbType DbType, int Size, ParameterDirection Direction, object Value)
    {
        SqlParameter param;
        if (Size > 0)
            param = new SqlParameter(ParamName, DbType, Size);
        else
            param = new SqlParameter(ParamName, DbType);
        param.Direction = Direction;
        if (!(Direction == ParameterDirection.Output && Value == null))
            param.Value = Value;
        return param;
    }
    #endregion
    #region 执行参数命令文本（无数据库中数据返回）
    //执行命令，可执行添加、修改与删除
    //<param name="procName">命令文本</param>
    //<param name="prams">参数对象</param>
    public int RunProc(string procName, SqlParameter[] prams)
    {
        SqlCommand cmd = CreateCommand(procName, prams);
        cmd.ExecuteNonQuery();
        this.Close();
        //得到执行成功返回值
        return (int)cmd.Parameters["ReturnValue"].Value;
    }

    //直接执行SQL语句，可用于数据库备份与恢复
    //<param name="procName">命令文本</param>
    public int RunProc(string procName)
    {
        this.Open();
        SqlCommand cmd = new SqlCommand(procName, con);
        cmd.ExecuteNonQuery();
        this.Close();
        return 1;
    }
    #endregion
    #region 执行参数命令文本（有返回值）
    //执行查询命令文本，并返回DataSet数据集
    //<param name="procName">命令文本</param>
    //<param name="prams">参数对象</param>
    //<param name="tbName">数据表名称</param>
    //<returns>DataSet<returns>
    public DataSet RunProcReturn(string procName, SqlParameter[] prams, string tbName)
    {
        SqlDataAdapter dap = CreateDataAdapter(procName, prams);
        DataSet ds = new DataSet();
        dap.Fill(ds, tbName);
        this.Close();
        //返回数据集
        return ds;
    }

    //执行命令文本，并返回DataSet数据集
    //<param name="procName">命令文本</param>    
    //<param name="tbName">数据表名称</param>
    //<returns>DataSet<returns>
    public DataSet RunProcReturn(string procName, string tbName)
    {
        SqlDataAdapter dap = CreateDataAdapter(procName, null);
        DataSet ds = new DataSet();
        dap.Fill(ds, tbName);
        this.Close();
        //返回数据集
        return ds;
    }
    #endregion
    #region 将命令文本添加到SqlDataAdapter
    //创建一个SqlDataAdapter对象来执行命令文本
    //<param name="procName">命令文本</param>    
    //<param name="prams">参数对象</param>
    //<returns>SqlDataAdapter对象<returns>
    private SqlDataAdapter CreateDataAdapter(string procName, SqlParameter[] prams)
    {
        this.Open();
        SqlDataAdapter dap = new SqlDataAdapter(procName, con);
        dap.SelectCommand.CommandType = CommandType.Text; //执行类型：命令文本
        if (prams != null)
        {
            foreach (SqlParameter parameter in prams)
                dap.SelectCommand.Parameters.Add(parameter);
        }
        //加入返回参数
        dap.SelectCommand.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));

        return dap;
    }
    #endregion
    #region 将命令文本添加到SqlCommand
    ////创建一个SqlCommand对象来执行命令文本
    //<param name="procName">命令文本</param>    
    //<param name="prams">命令文本所需参数</param>
    //<returns>SqlCommand对象<returns>
    private SqlCommand CreateCommand(string procName, SqlParameter[] prams)
    {
        this.Open();
        SqlCommand cmd = new SqlCommand(procName, con);
        cmd.CommandType = CommandType.Text;   //执行类型：命令文本
                                              //依次把参数传入命令文本
        if (prams != null)
        {
            foreach (SqlParameter parameter in prams)
                cmd.Parameters.Add(parameter);
        }
        //加入返回参数
        cmd.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, ParameterDirection.ReturnValue, false, 0, 0, string.Empty, DataRowVersion.Default, null));

        return cmd;
    }
    #endregion
}
