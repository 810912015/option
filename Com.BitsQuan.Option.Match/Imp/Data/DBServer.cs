﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Match.Imp
{

    /// <summary>
    /// 数据库操作类 Sqlserver
    /// </summary>
    public class DBServer:IDisposable
    {
        public string _ConnString { get; private set; }
        SqlConnection _Conn { get; set; }
        public DBServer() : this(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString()) { }
        public DBServer(string connstr)
        {
            this._ConnString = connstr;
            _Conn = new SqlConnection();
        }
        
        public  bool Open()
        {
            if (_Conn.State == ConnectionState.Open) return true;
            _Conn.ConnectionString = _ConnString;
            try
            {
                _Conn.Open();
                return true;
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
               
                return false;
            }
        }

        

        public  DataSet ExecuteDataset(string sql)
        {
            try
            {
                if (_Conn.State != ConnectionState.Open)
                    if (!Open()) return null;
                SqlCommand cmd = new SqlCommand(sql, _Conn);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                   
                    DataSet ds = new DataSet();
                    // Fill the DataSet using default values for DataTable names, etc
                    da.Fill(ds);
                    // Return the dataset
                    return ds;
                }
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
                return null;
            }
        }

        //不返回作用行数
        public  void ExecuteNonQuery(string sql)
        {
            try
            {
                if (_Conn.State != ConnectionState.Open)
                    if (!Open()) return;
                SqlCommand pCommand = new SqlCommand(sql, _Conn);
                pCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);      
            }
        }
        //返回作用行数
        public  int ExecNonQuery(string sql)
        {
            try
            {
                if (_Conn.State != ConnectionState.Open)
                    if (!Open()) return 0;
                SqlCommand pCommand = new SqlCommand(sql, _Conn);
                int AffectRowNum = pCommand.ExecuteNonQuery();
                return AffectRowNum;
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex,sql);
                
            }
            return 0;
        }
        
        
        public  void Close()
        {
            _Conn.Close();
        }
        public  SqlDataReader ExecuteReader(string sql)
        {
            try
            {
                if (_Conn.State != ConnectionState.Open)
                    if (!Open()) return null;

                SqlCommand pCommand = new SqlCommand(sql, _Conn);
                SqlDataReader pReader = pCommand.ExecuteReader();
                if (pReader.Read() == false)
                {
                    pReader.Close();
                    return null;
                }
                
                return pReader;
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
                return null;
            }
        }
        public  object ExcuteScale(string sql)
        {
            try
            {
                if (_Conn.State != ConnectionState.Open)
                    if (!Open()) return null;
                SqlCommand pCommand = new SqlCommand(sql, _Conn);
                var r= pCommand.ExecuteScalar();
                return r;
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
                return 0;
            }

        }

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch(Exception e) {
                Singleton<TextLog>.Instance.Error(e, "dbserver dispose");
            }
        }
    }
}
