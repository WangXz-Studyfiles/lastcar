using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace CarProject
{
    class SQL
    {

        //public static readonly string SqlConnectionString = " Data Source=localhost;Initial Catalog=Car_data;user id=;password= ";

        public static readonly string SqlConnectionString = " Data Source=localhost;Initial Catalog=Car_data;Integrated Security=True ";
        /// <summary>
        ///  SQL语句，主要针对增、删、改三种.
        /// </summary>
        /// <param name="sql">增、删、改SQL语句,可以是一条，也可以是多条用；隔开的字符串</param>
        public  int  ExecuteNonQuery(string sql)
        {
            int re;
            SqlConnection sqlConn = new SqlConnection();
            sqlConn.ConnectionString = SqlConnectionString;
            sqlConn.Open();

            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Connection = sqlConn;
            sqlComm.CommandText = sql;

            try
            {
              re = sqlComm.ExecuteNonQuery();
                sqlConn.Close();
            }
            catch (Exception e)
            {
                sqlConn.Close();
                throw e;
            }
            return re;

        }

        /// <summary>
        /// 执行查询语句，返回记录集中第一行第一列的值,这个主要是针对查询结果为单值的查询
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>返回查询结果集的第一行第一列</returns>
        public  object ExecuteScalar(string sql)
        {
            object returnValue = null;
            SqlConnection sqlConn = new SqlConnection();
            sqlConn.ConnectionString = SqlConnectionString;
            sqlConn.Open();

            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Connection = sqlConn;
            sqlComm.CommandText = sql;
            try
            {
                returnValue = sqlComm.ExecuteScalar();
                sqlConn.Close();
                return returnValue;
            }
            catch (Exception e)
            {
                sqlConn.Close();
                throw e;
                //return returnValue;
            }
        }

        /// <summary>
        /// 执行多条sql语句,这些sql语句存放在字符串数组中,传进来;
        /// 多条sql语句都是增、删、改语句,这里用了事务
        /// </summary>
        /// <param name="sqls">sql语句集</param>
        public  void ExecuteSqlsSetTransaction(params string[] sqls)
        {
            SqlConnection sqlConn = new SqlConnection();
            sqlConn.ConnectionString = SqlConnectionString;
            sqlConn.Open();

            SqlTransaction sqlTrans = sqlConn.BeginTransaction();

            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Connection = sqlConn;
            sqlComm.Transaction = sqlTrans;

            try
            {
                foreach (string sql in sqls)
                {
                    sqlComm.CommandText = sql;
                    sqlComm.ExecuteNonQuery();
                }
                sqlTrans.Commit();
                sqlConn.Close();
            }
            catch (Exception e)
            {
                //如果在执行的过程中，网络断开了，进来之后，直接执行sqlTrans.Rollback()就会出错
                //因为此时该事务不再有效，那么sqlTrans.Connection==null
                if (sqlTrans.Connection != null)
                {
                    sqlTrans.Rollback();
                }
                sqlConn.Close();
                throw e;
            }
        }

        /// <summary>
        /// 执行单个语句的事务，语句都是增、删、改语句
        /// </summary>
        /// <param name="sql">sql语句字符串</param>
        /// <param name="oracleConn">Sql连接对象</param>
        /// <param name="oracleTrans">Sql事务对象</param>
        public  void ExecuteTransaction(string sql, SqlConnection sqlConn, SqlTransaction sqlTrans)
        {
            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Connection = sqlConn;
            sqlComm.Transaction = sqlTrans;
            sqlComm.CommandText = sql;
            sqlComm.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行返回数据集的查询语句
        /// </summary>
        /// <param name="sql">一个查询语句</param>
        /// <returns>返回查询到的数据集</returns>
        public  DataSet ExecuteDataSet(string sql)
        {
            SqlConnection sqlConn = new SqlConnection();
            sqlConn.ConnectionString = SqlConnectionString;
            sqlConn.Open();

            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Connection = sqlConn;
            sqlComm.CommandText = sql;

            SqlDataAdapter sqlDa = new SqlDataAdapter();
            sqlDa.SelectCommand = sqlComm;
            DataSet ds = new DataSet();

            try
            {
                sqlDa.Fill(ds);
                sqlConn.Close();
                return ds;
            }
            catch (Exception e)
            {
                sqlConn.Close();
                throw e;
                //return ds;
            }

        }

        /// <summary>
        /// 这个函数是执行存储过程的，没有返回值，存储过程的返回值可以通过参数获得
        /// </summary>
        /// <param name="procedureName">存储过程名</param>
        /// <param name="parameters">参数集合</param>
        public  void ExecuteNonQuerySP(string procedureName, params SqlParameter[] parameters)
        {
            SqlConnection sqlConn = new SqlConnection();
            sqlConn.ConnectionString = SqlConnectionString;
            sqlConn.Open();

            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Connection = sqlConn;
            sqlComm.CommandType = CommandType.StoredProcedure;
            sqlComm.CommandText = procedureName;

            foreach (SqlParameter p in parameters)
            {
                sqlComm.Parameters.Add(p);
            }

            try
            {
                sqlComm.ExecuteNonQuery();
                sqlConn.Close();
            }
            catch (Exception e)
            {
                sqlConn.Close();
                throw e;
            }

        }

        /// <summary>
        /// 这个主要是针对一条复杂的sql语句查询的，复杂的sql语句非常长，编译也比较慢，
        /// 放到存储过程里面可以减少从客户端向服务器传输的时间，存储过程是预编译
        /// 好了的，这样可以提高效率。
        /// </summary>
        /// <param name="procedureName">存储过程名</param>
        /// <param name="parameters">参数，一般都是查询的判断条件</param>
        public  DataSet ExecuteDataSetSP(string procedureName, params SqlParameter[] parameters)
        {
            SqlConnection sqlConn = new SqlConnection();
            sqlConn.ConnectionString = SqlConnectionString;
            sqlConn.Open();

            SqlCommand sqlComm = new SqlCommand();
            sqlComm.Connection = sqlConn;
            sqlComm.CommandType = CommandType.StoredProcedure;
            sqlComm.CommandText = procedureName;

            foreach (SqlParameter p in parameters)
            {
                sqlComm.Parameters.Add(p);
            }

            SqlDataAdapter sqlDa = new SqlDataAdapter();
            sqlDa.SelectCommand = sqlComm;

            DataSet ds = new DataSet();


            try
            {
                sqlDa.Fill(ds);
                sqlConn.Close();
                return ds;
            }
            catch (Exception e)
            {
                sqlConn.Close();
                throw e;
                //return ds;
            }
        }
 

        /// <summary>
        /// 执行没有返回值的命令，主要针对增、删、改三种语句。
        /// </summary>
        /// <param name="sql">增、删、改SQL语句</param>
        public  void ExecuteNonQuery1(string sql)
        {

            SqlConnection sqlConn = new SqlConnection();


            SqlCommand sqlComm = new SqlCommand();


            try
            {
                sqlConn.ConnectionString = SqlConnectionString;
                sqlConn.Open();

                sqlComm.Connection = sqlConn;
                sqlComm.CommandText = sql;

                sqlComm.ExecuteNonQuery();
                sqlConn.Close();
            }
            catch (Exception e)
            {
                sqlConn.Close();
                throw e;
            }

        }

        /// <summary>
        /// 执行查询语句，返回记录集中第一行第一列的值,这个主要是针对查询结果为单值的查询
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>返回查询结果集的第一行第一列</returns>
        public  object ExecuteScalar1(string sql)
        {
            object returnValue = null;

            SqlConnection sqlConn = new SqlConnection();

            SqlCommand sqlComm = new SqlCommand();

            try
            {
                sqlConn.ConnectionString = SqlConnectionString;
                sqlConn.Open();

                sqlComm.Connection = sqlConn;
                sqlComm.CommandText = sql;

                returnValue = sqlComm.ExecuteScalar();
                sqlConn.Close();
                return returnValue;
            }
            catch (Exception e)
            {
                sqlConn.Close();
                throw e;
                //return returnValue;
            }
        }

        /// <summary>
        /// 执行多条sql语句，这些sql语句存放在字符串数组中，传进来；
        /// 多条sql语句都是增、删、改语句，这里用了事务
        /// </summary>
        /// <param name="sqls">sql语句集</param>
        public  void ExecuteSqlsSet1(params string[] sqls)
        {
            SqlConnection sqlConn = new SqlConnection();

            SqlTransaction sqlTrans = null;

            SqlCommand sqlComm = new SqlCommand();

            try
            {
                sqlConn.ConnectionString = SqlConnectionString;
                sqlConn.Open();

                sqlTrans = sqlConn.BeginTransaction();


                sqlComm.Connection = sqlConn;
                sqlComm.Transaction = sqlTrans;

                foreach (string sql in sqls)
                {
                    sqlComm.CommandText = sql;
                    sqlComm.ExecuteNonQuery();
                }
                sqlTrans.Commit();
                sqlConn.Close();
            }
            catch (Exception e)
            {
                //如果在执行的过程中，网络断开了，进来之后，直接执行sqlTrans.Rollback()就会出错
                //因为此时该事务不再有效，那么sqlTrans.Connection==null
                if (sqlTrans.Connection != null)
                {
                    sqlTrans.Rollback();
                }
                sqlConn.Close();
                throw e;
            }
        }


        /// <summary>
        /// 执行返回数据集的查询语句
        /// </summary>
        /// <param name="sql">一个查询语句</param>
        /// <returns>返回查询到的数据集</returns>
        public  DataSet ExecuteDataSet1(string sql)
        {
            SqlConnection sqlConn = new SqlConnection();

            SqlCommand sqlComm = new SqlCommand();

            SqlDataAdapter sqlDa = new SqlDataAdapter();

            DataSet ds = new DataSet();

            try
            {
                sqlConn.ConnectionString = SqlConnectionString;
                sqlConn.Open();

                sqlComm.Connection = sqlConn;
                sqlComm.CommandText = sql;

                sqlDa.SelectCommand = sqlComm;

                sqlDa.Fill(ds);
                sqlConn.Close();
                return ds;
            }
            catch (Exception e)
            {
                sqlConn.Close();
                throw e;
                //return ds;
            }

        }

        /// <summary>
        /// 这个函数是执行存储过程的，没有返回值，存储过程的返回值可以通过参数获得
        /// </summary>
        /// <param name="procedureName">存储过程名</param>
        /// <param name="parameters">参数集合</param>
        public  void ExecuteNonQuerySP1(string procedureName, params SqlParameter[] parameters)
        {
            SqlConnection sqlConn = new SqlConnection();

            SqlCommand sqlComm = new SqlCommand();

            try
            {
                sqlConn.ConnectionString = SqlConnectionString;
                sqlConn.Open();

                sqlComm.Connection = sqlConn;
                sqlComm.CommandType = CommandType.StoredProcedure;
                sqlComm.CommandText = procedureName;

                foreach (SqlParameter p in parameters)
                {
                    sqlComm.Parameters.Add(p);
                }

                sqlComm.ExecuteNonQuery();
                sqlConn.Close();
            }
            catch (Exception e)
            {
                sqlConn.Close();
                throw e;
            }

        }


        /// <summary>
        /// 这个主要是针对一条复杂的sql语句查询的，复杂的sql语句非常长，编译也比较慢，
        /// 放到存储过程里面可以减少从客户端向服务器传输的时间，存储过程是预编译
        /// 好了的，这样可以提高效率。
        /// </summary>
        /// <param name="procedureName">存储过程名</param>
        /// <param name="parameters">参数，一般都是查询的判断条件</param>
        public  DataSet ExecuteDataSetSP1(string procedureName, params SqlParameter[] parameters)
        {
            SqlConnection sqlConn = new SqlConnection();

            SqlCommand sqlComm = new SqlCommand();

            SqlDataAdapter sqlDa = new SqlDataAdapter();

            DataSet ds = new DataSet();

            try
            {
                sqlConn.ConnectionString = SqlConnectionString;
                sqlConn.Open();

                sqlComm.Connection = sqlConn;
                sqlComm.CommandType = CommandType.StoredProcedure;
                sqlComm.CommandText = procedureName;

                foreach (SqlParameter p in parameters)
                {
                    sqlComm.Parameters.Add(p);
                }

                sqlDa.SelectCommand = sqlComm;

                sqlDa.Fill(ds);
                sqlConn.Close();
                return ds;
            }
            catch (Exception e)
            {
                sqlConn.Close();
                throw e;                
            }
        }
    }
}
