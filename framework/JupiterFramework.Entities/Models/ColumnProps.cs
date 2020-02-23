using JupiterFramework.DataAccess.Dapper.Interfaces;

namespace JupiterFramework.Entities.Models
{
    public class ColumnProps : EntityBase
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }

    }
}
