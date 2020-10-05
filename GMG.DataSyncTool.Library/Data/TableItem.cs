using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace GMG.DataSyncTool.Library.Data
{
    class TableItem
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public int Level { get; set; }

        public List<PrimaryKeyItem> PrimaryKeys { get; set; }
        public List<ColumnItem> Columns { get; set; }

        public static List<TableItem> GetList(DbContext context)
        {
            var items = context.Database.SqlQuery<TableItem>(Properties.Resources.DependencyOrderScript).ToList();

            var columns = ColumnItem.GetList(context).ToList();
            var pks = PrimaryKeyItem.GetList(context).ToList();

            foreach (var item in items)
            {
                item.Columns = columns.Where(c => c.TableName == item.TableName && c.SchemaName == item.SchemaName).ToList();
                item.PrimaryKeys = pks.Where(c => c.TableName == item.TableName && c.SchemaName == item.SchemaName).ToList();
            }

            return items.ToList();
        }
    }
}
