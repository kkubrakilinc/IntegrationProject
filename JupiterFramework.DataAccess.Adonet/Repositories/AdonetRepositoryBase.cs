using System;
using System.Configuration;
using System.Data.SqlClient;


namespace JupiterFramework.DataAccess.Adonet.Repositories
{
    public class AdonetRepositoryBase : IDisposable
    {
        public SqlConnection sqlcon;
        public AdonetRepositoryBase()
        {
            sqlcon = new SqlConnection( ConfigurationManager.ConnectionStrings["BaseConnectionString"].ConnectionString);
            sqlcon.Open();
        }
        
        public void Dispose()
        {
            sqlcon.Close();
            GC.SuppressFinalize(this);
        }
    }
}
