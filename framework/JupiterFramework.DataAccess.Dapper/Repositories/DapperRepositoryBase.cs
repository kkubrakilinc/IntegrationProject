using Dapper;
using JupiterFramework.DataAccess.Dapper.Enums;
using JupiterFramework.DataAccess.Dapper.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace JupiterFramework.DataAccess.Dapper.Repositories
{
    public class DapperRepositoryBase<T> : IDapperRepositoryBase<T>, IDisposable where T : EntityBase
    {
        public static string ConnStr;
        public static DataSources _DataSource;

        public static IDbConnection Connection
        {
            get
            {
                if (string.IsNullOrEmpty(ConnStr))
                {
                    if (_DataSource == DataSources.SPROVO)
                        ConnStr = ConfigurationManager.ConnectionStrings["BaseConnectionString"].ConnectionString;
                    else
                        ConnStr = ConfigurationManager.ConnectionStrings["BaseConnectionStringCRM"].ConnectionString;
                }
                return new SqlConnection(ConnStr);
            }
        }

        public bool Execute(string query, DataSources src = DataSources.SPROVO)
        {
            _DataSource = src;

            using (var ctx = Connection)
            {
                ctx.ConnectionString = "server = .\\kubraserver; database=KozaCRM; integrated security=true";
                return ctx.Execute(query) > 0;
            }
        }

        public bool ClearTriggerController(string query)
        {
            using (var ctx = Connection)
            {
                ctx.ConnectionString = "server = .\\kubraserver; database=sprovo_v1; integrated security=true";
                return ctx.Execute(query) > 0;
            }
        }

        public List<T> GetAll(string query, DataSources src = DataSources.SPROVO)
        {
            List<T> res;
            _DataSource = src;

            using (var ctx = Connection)
            {

                var data = ctx.Query<T>(query);
                res = data.ToList();
            }

            return res;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
