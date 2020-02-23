using JupiterFramework.DataAccess.Dapper.Interfaces;
namespace JupiterFramework.Entities.SPROVO
{
    public class IntegrationTriggeredController : EntityBase
    {
        public int ID { get; set; }
        public string Table_Name { get; set; }
        public string Table_ID_Name { get; set; }
        public string Table_ID { get; set; }
        public string Operation { get; set; }
    }
}
