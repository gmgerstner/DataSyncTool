using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMG.DataSyncTool.Library.Data
{
    class PrimaryKeyItem
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int IsIdentity { get; set; }

        public static List<PrimaryKeyItem> GetList(DbContext context)
        {
            var items = context.Database
                .SqlQuery<PrimaryKeyItem>(Properties.Resources.PrimaryKeyColumnsScript);

            return items.ToList();
        }

    }
}
