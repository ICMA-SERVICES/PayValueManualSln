using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PayValueManualSln.Shared.DapperServices
{
    public interface IDapper : IDisposable
    {
        //DbConnection GetDbconnection();
        //DbConnection GetDbconnection(string connecString);
        T Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null);
        List<T> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null);
        int Execute(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null);
        int Execute<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null);
        T Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null);
        T Update<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null);
    }
}
