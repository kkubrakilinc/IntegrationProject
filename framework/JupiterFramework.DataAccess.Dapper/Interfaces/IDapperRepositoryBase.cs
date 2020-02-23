using System.Collections.Generic;
using JupiterFramework.DataAccess.Dapper.Enums;
namespace JupiterFramework.DataAccess.Dapper.Interfaces
{
    interface IDapperRepositoryBase<T> where T : EntityBase
    {
        List<T> GetAll(string query, DataSources src);
    }

}
