using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace PayValueManualSln.Shared.DapperServices
{
    public class DapperService : IDapper
    {
        private readonly IConfiguration _config;
        private readonly string Connectionstring = "DefaultConnection";

        public DapperService(IConfiguration config)
        {
            _config = config;

            //Read Configuration from appSettings
            //var configs = new ConfigurationBuilder()
            //  .AddJsonFile("appsettings.json")
            //  .Build();

            //Initialize Logger
            //Log.Logger = new LoggerConfiguration()
            //    .ReadFrom.Configuration(configs)
            //    .CreateLogger();
        }
        public void Dispose()
        {

        }

        public int Execute(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null)
        {
            int result;
            using IDbConnection db = GetDbconnection(connecString);
            if (db.State == ConnectionState.Closed)
                db.Open();

            using var tran = db.BeginTransaction();
            try
            {
                result = db.Execute(sp, parms, commandType: commandType, transaction: tran);
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                var SerializeReponse = JsonConvert.SerializeObject(ex.Message);
                Log.Fatal(SerializeReponse);
                throw ex;
            }

            return result;
        }

        public int Execute<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null)
        {
            int result;
            using IDbConnection db = GetDbconnection(connecString);
            if (db.State == ConnectionState.Closed)
                db.Open();

            using var tran = db.BeginTransaction();
            try
            {
                result = db.Execute(sp, parms, commandType: commandType, transaction: tran);
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                var SerializeReponse = JsonConvert.SerializeObject(ex.Message);
                Log.Fatal(SerializeReponse);
                throw ex;
            }

            return result;
        }

        public T Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null)
        {
            using IDbConnection db = GetDbconnection(connecString); //new SqlConnection(_config.GetConnectionString(connecString));

            return db.Query<T>(sp, parms, commandType: commandType).FirstOrDefault();
        }

        public List<T> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.Text, string connecString = null)
        {
            using IDbConnection db = GetDbconnection(connecString); //new SqlConnection(_config.GetConnectionString(Connectionstring));
            return db.Query<T>(sp, parms, commandType: commandType).ToList();
        }

        public DbConnection GetDbconnection(string connecString)
        {
            if (string.IsNullOrEmpty(connecString))
            {
                connecString = Connectionstring;
            }
			return new SqlConnection(_config.GetConnectionString(connecString));
        }

        public T Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null)
        {
            T result;
            using IDbConnection db = GetDbconnection(connecString); //new SqlConnection(_config.GetConnectionString(Connectionstring));
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                using var tran = db.BeginTransaction();
                try
                {
                    var results = db.Execute(sp, parms, commandType: CommandType.StoredProcedure, transaction: tran);

                    result = db.Query<T>(sp, parms, commandType: commandType, transaction: tran).FirstOrDefault();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    var SerializeReponse = JsonConvert.SerializeObject(ex.Message);
                    Log.Fatal(SerializeReponse);
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }

            return result;
        }

        public T Update<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure, string connecString = null)
        {
            T result;
            using IDbConnection db = GetDbconnection(connecString); //new SqlConnection(_config.GetConnectionString(Connectionstring));
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                using var tran = db.BeginTransaction();
                try
                {
                    result = db.Query<T>(sp, parms, commandType: commandType, transaction: tran).FirstOrDefault();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }

            return result;
        }

    }
}
