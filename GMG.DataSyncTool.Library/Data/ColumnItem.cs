using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMG.DataSyncTool.Library.Data
{
    class ColumnItem
    {
        //public string CatalogName { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int Position { get; set; }
        //public string ColumnDefault { get; set; }
        //public string IsNullable { get; set; }
        public string DataType { get; set; }
        //public int? MaxCharacterLength { get; set; }
        //public int? NumericPrecision { get; set; }

        public static List<ColumnItem> GetList(DbContext context, TableItem table)
        {
            var args = new DbParameter[] 
            { 
                new SqlParameter { ParameterName = "TableName", Value = table.TableName },
                new SqlParameter { ParameterName = "SchemaName", Value = table.SchemaName }
            };

            var filter = " WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = @SchemaName";

            var items = context.Database
                .SqlQuery<ColumnItem>(Properties.Resources.TableColumnScript + filter, args);
            
            return items.ToList();
        }

        public static List<ColumnItem> GetList(DbContext context)
        {
            var items = context.Database
                .SqlQuery<ColumnItem>(Properties.Resources.TableColumnScript);
            
            return items.ToList();
        }
    }
}
