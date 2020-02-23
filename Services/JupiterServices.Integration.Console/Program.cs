using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JupiterFramework.DataAccess.Dapper.Repositories;
using JupiterFramework.Entities.Models;
using JupiterFramework.Entities.SPROVO;
using JupiterFramework.DataAccess.Adonet.Repositories;

namespace JupiterServices.Integration.Console
{
    class Program
    {
        static void Main(string[] args)


        {
            var data = new List<IntegrationTriggeredController>();

            using (var ctx = new DapperRepositoryBase<IntegrationTriggeredController>())
            {
                data = ctx.GetAll("select baseTable.Table_Name, baseTable.Table_ID_Name, baseTable.Table_ID, baseTable.Operation from Integration_Triggered_Controller as baseTable");
            }

            using (var ctx = new DapperRepositoryBase<IntegrationTriggeredController>())
            {
                foreach (var item in data)
                {
                   var query = "";
                   var ColumnNames = new Dictionary<string, string>();
                   var ColumnValues = new Dictionary<string, string>();
                   var InsertColumnsNames = "";
                   var InsertColumnValues = "";

                    if (item.Operation == "d")
                    {
                        //table_name = hangi tabloda değişiklik yapıldığı
                        //table_id_name = değişiklik yapılan tablonun pri key'i
                        //table_id = değişiklik yapılan tablodaki satırın id'i
                        query = $@"set IDENTITY_INSERT {item.Table_Name} ON delete from {item.Table_Name} where {item.Table_ID_Name}={item.Table_ID}";
                    }
                    else
                    {
                        query = $"set IDENTITY_INSERT {item.Table_Name} ON select COLUMN_NAME Columnname, data_type datatype from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='{item.Table_Name}'";

                        var ClmPrp = new List<ColumnProps>();
                        using (var prp = new DapperRepositoryBase<ColumnProps>())
                        {
                            ClmPrp = prp.GetAll(query);
                            ClmPrp.ForEach(k =>
                            {
                                ColumnNames.Add(k.ColumnName, k.DataType);
                            });
                        }
                        // buradan sonra işlem yapmak istediğim tabloya ait tüm bilgilere sahibim.

                        foreach (var col in ColumnNames)
                        {
                            InsertColumnsNames += col.Key + ",";
                        }

                        InsertColumnsNames = InsertColumnsNames.Remove(InsertColumnsNames.Length - 1, 1);

                        query = $"set IDENTITY_INSERT {item.Table_Name} ON select {InsertColumnsNames} from {item.Table_Name} where {item.Table_ID_Name}={item.Table_ID}";

                        using (var ado = new AdonetRepositoryBase())
                        {
                            using (var cmd = new SqlCommand(query, ado.sqlcon))
                            {
                                cmd.CommandType = System.Data.CommandType.Text;

                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        foreach (var clm in ColumnNames)
                                        {
                                            if (clm.Value == "nvarchar" || clm.Value == "varchar" || clm.Value == "datetime")
                                            {
                                                if (clm.Value == "datetime")
                                                {
                                                    if (DateTime.TryParse(reader[clm.Key].ToString(), out var dt))
                                                    {
                                                        var mnt = dt.Month.ToString().Length > 1 ? dt.Month.ToString() : "0" + dt.Month.ToString();
                                                        var dy = dt.Day.ToString().Length > 1 ? dt.Day.ToString() : "0" + dt.Day.ToString();
                                                        var dv = $"{dt.Year}-{mnt}-{dy} 00:00:00";

                                                        InsertColumnValues += "'" + dv + "',";
                                                        ColumnValues.Add(clm.Key, "'" + dv + "'");
                                                    }
                                                    else
                                                    {
                                                        InsertColumnValues += "null,";
                                                        ColumnValues.Add(clm.Key, "null");
                                                    }

                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(reader[clm.Key].ToString()))
                                                    {
                                                        InsertColumnValues += "'" + reader[clm.Key] + "',";
                                                        ColumnValues.Add(clm.Key, "'" + reader[clm.Key] + "'");
                                                    }
                                                    else
                                                    {
                                                        InsertColumnValues += "null,";
                                                        ColumnValues.Add(clm.Key, "null");
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(reader[clm.Key].ToString()))
                                                {
                                                    InsertColumnValues += "null,";
                                                    ColumnValues.Add(clm.Key, "null");
                                                }
                                                else
                                                {
                                                    InsertColumnValues += reader[clm.Key].ToString().Replace(",", ".") + ",";
                                                    ColumnValues.Add(clm.Key, reader[clm.Key].ToString().Replace(",", "."));
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }

                        if (ColumnValues.Count > 0)
                        {
                            InsertColumnValues = InsertColumnValues.Remove(InsertColumnValues.Length - 1, 1);
                            if (item.Operation == "i")
                            {
                                //insert into visit (id, name, surname) values (5, 'kubra','asdf')
                                query = $@"set IDENTITY_INSERT {item.Table_Name} ON insert into {item.Table_Name} ({InsertColumnsNames}) values ({InsertColumnValues})";
                            }


                            else if (item.Operation == "u")
                            {
                                // update visit set id=5, name='kubra', surname='asdf' where id=10
                                var updateStr = "";
                                foreach (var updateData in ColumnValues)
                                {
                                    if (updateData.Key != "ID" && updateData.Key != "id")
                                    {
                                        updateStr += updateData.Key + "=" + updateData.Value + ",";
                                    }
                                }
                                updateStr = updateStr.Remove(updateStr.Length - 1, 1);
                                query = $@"set IDENTITY_INSERT {item.Table_Name} ON update {item.Table_Name} set {updateStr} where {item.Table_ID_Name}={item.Table_ID}";
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(query) && !query.Contains("select"))
                    {
                        using (var context = new DapperRepositoryBase<IntegrationTriggeredController>())
                        {
                            System.Console.WriteLine(query);
                            context.Execute(query, JupiterFramework.DataAccess.Dapper.Enums.DataSources.CRM);

                            context.ClearTriggerController("delete from Integration_Triggered_Controller");
                        }
                    }
                }
            }

            System.Console.ReadLine();
        }
    }
}
