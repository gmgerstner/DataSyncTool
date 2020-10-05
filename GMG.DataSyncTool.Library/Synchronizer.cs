using GMG.DataSyncTool.Library.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace GMG.DataSyncTool.Library
{
    public class Synchronizer : IDisposable
    {
        private DbContext SourceContext;
        private DbContext DestinationContext;

        public Synchronizer(string SourceConnectionString, string DestinationConnectionString)
        {
            SourceContext = new DbContext(SourceConnectionString);
            DestinationContext = new DbContext(DestinationConnectionString);
        }

        public void Dispose()
        {
            SourceContext.Dispose();
            DestinationContext.Dispose();
        }

        public string GenerateScript()
        {
            //Connect to source
            //List<TableItem> sourceTables = TableItem.GetList(SourceContext);
            //Connect to destination
            List<TableItem> destinationTables = TableItem.GetList(DestinationContext);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("BEGIN TRAN");

            //Delete extra rows
            foreach (var table in destinationTables.OrderByDescending(t => t.Level))
            {
                if (table.PrimaryKeys.Count == 0) continue;

                sb.AppendFormat("DELETE FROM [{0}].[{1}] \r\n", table.SchemaName, table.TableName);
                sb.AppendFormat("FROM   [{0}].[{1}] \r\n", table.SchemaName, table.TableName);
                sb.AppendFormat("       LEFT OUTER JOIN [{2}].[{0}].[{1}] AS Source \r\n", table.SchemaName, table.TableName, SourceContext.Database.Connection.Database);
                sb.AppendFormat("                    ON [{0}].[{1}].[{2}] = Source.[{2}] \r\n", table.SchemaName, table.TableName, table.PrimaryKeys[0].ColumnName);
                for (int i = 1; i < table.PrimaryKeys.Count(); i++)
                {
                    sb.AppendFormat("                    AND [{0}].[{1}].[{2}] = Source.[{2}] \r\n", table.SchemaName, table.TableName, table.PrimaryKeys[i].ColumnName);
                }
                sb.AppendFormat("WHERE  (Source.[{0}] IS NULL)  \r\n", table.PrimaryKeys[0].ColumnName);
            }

            //Update differences in rows
            foreach (var table in destinationTables.OrderByDescending(t => t.Level))
            {
                if (table.PrimaryKeys.Count == 0) continue;

                var pkCols = from col in table.Columns
                             join pk in table.PrimaryKeys on col.ColumnName equals pk.ColumnName
                             select col;
                var nonPks = table.Columns.Except(pkCols);

                if (nonPks.Count() == 0)
                {
                    continue;
                }

                sb.AppendFormat("UPDATE [{0}].[{1}]  \r\n", table.SchemaName, table.TableName);
                sb.AppendFormat("SET  \r\n");
                for (int i = 0; i < nonPks.Count(); i++)
                {
                    sb.AppendFormat("        [{0}] = Source.[{0}]{1} \r\n", nonPks.ElementAt(i).ColumnName, (i == nonPks.Count() - 1 ? "" : ","));
                }
                sb.AppendFormat("    FROM  \r\n");
                sb.AppendFormat("        [{0}].[{1}] AS Target  \r\n", table.SchemaName, table.TableName);
                sb.AppendFormat("    INNER JOIN [{2}].[{0}].[{1}] AS Source  \r\n", table.SchemaName, table.TableName, SourceContext.Database.Connection.Database);
                sb.AppendFormat("        ON Target.[{0}] = Source.[{0}] \r\n", table.PrimaryKeys[0].ColumnName);
                for (int i = 1; i < table.PrimaryKeys.Count(); i++)
                {
                    sb.AppendFormat("        AND Target.[{0}] = Source.[{0}] \r\n", table.PrimaryKeys[0].ColumnName);
                }
                sb.AppendFormat("WHERE  \r\n");
                var firstColUsed = false;
                for (int i = 0; i < table.Columns.Count(); i++)
                {
                    switch (table.Columns[i].DataType)
                    {
                        case "bit":
                        case "decimal":
                        case "int":
                        case "datetime":
                            sb.AppendFormat("    {1}(Target.[{0}] <> Source.[{0}])  \r\n", table.Columns[0].ColumnName, firstColUsed ? "" : "OR ");
                            firstColUsed = true;
                            break;

                        case "char":
                        case "varchar":
                        case "nvarchar":
                        case "ntext":
                            sb.AppendFormat("    {1}(Isnull(CONVERT(VARCHAR(max), Target.[{0}]), 'NULL') <> Isnull(CONVERT(VARCHAR(max), Source.[{0}]), 'NULL'))  \r\n", table.Columns[0].ColumnName, firstColUsed ? "" : "OR ");
                            firstColUsed = true;
                            break;

                        case "image":
                            sb.AppendFormat("    {1}(Target.[{0}] <> Source.[{0}])  \r\n", table.Columns[0].ColumnName, firstColUsed ? "" : "OR ");
                            firstColUsed = true;
                            break;

                        case "uniqueidentifier":
                            sb.AppendFormat("    {1}(Target.[{0}] <> Source.[{0}])  \r\n", table.Columns[0].ColumnName, firstColUsed ? "" : "OR ");
                            firstColUsed = true;
                            break;

                        default:
                            sb.AppendFormat("    {1}(Target.[{0}] <> Source.[{0}])  \r\n", table.Columns[0].ColumnName, firstColUsed ? "" : "OR ");
                            firstColUsed = true;
                            break;
                    }
                }
            }

            //Insert missing rows
            foreach (var table in destinationTables.OrderBy(t => t.Level))
            {
                if (table.PrimaryKeys.Count == 0) continue;

                //var identities = from col in table.Columns
                //                 join pk in table.PrimaryKeys on col.ColumnName equals pk.ColumnName
                //                 where pk.IsIdentity == 1
                //                 select col;
                //var insertableColumns = table.Columns.Except(identities);
                var insertableColumns = table.Columns;

                if (table.PrimaryKeys.Where(pk => pk.IsIdentity == 1).Any())
                    sb.AppendFormat("SET IDENTITY_INSERT [{0}].[{1}] ON \r\n", table.SchemaName, table.TableName);

                sb.AppendFormat("INSERT INTO [{0}].[{1}]  \r\n", table.SchemaName, table.TableName);
                sb.AppendFormat("(  \r\n");
                foreach (var col in insertableColumns)
                {
                    sb.AppendFormat("  [{0}]{1}  \r\n", col.ColumnName, (col != insertableColumns.Last() ? "," : ""));
                }
                sb.AppendFormat(")  \r\n");
                sb.AppendFormat("SELECT  \r\n");
                foreach (var col in insertableColumns)
                {
                    sb.AppendFormat("  Source.[{0}]{1}  \r\n", col.ColumnName, (col != insertableColumns.Last() ? "," : ""));
                }
                sb.AppendFormat("FROM  \r\n");
                sb.AppendFormat("  [{0}].[{1}] AS Target  \r\n", table.SchemaName, table.TableName);
                sb.AppendFormat("  RIGHT OUTER JOIN [{2}].[{0}].[{1}] AS Source  \r\n", table.SchemaName, table.TableName, SourceContext.Database.Connection.Database);
                sb.AppendFormat("                ON Target.[{0}] = Source.[{0}] \r\n", table.PrimaryKeys[0].ColumnName);
                for (int i = 1; i < table.PrimaryKeys.Count(); i++)
                {
                    sb.AppendFormat("               AND Target.[{0}] = Source.[{0}] \r\n", table.PrimaryKeys[0].ColumnName);
                }
                sb.AppendFormat("WHERE  \r\n");
                sb.AppendFormat("  (Target.[{0}] IS NULL)  \r\n", table.PrimaryKeys[0].ColumnName);

                if (table.PrimaryKeys.Where(pk => pk.IsIdentity == 1).Any())
                    sb.AppendFormat("SET IDENTITY_INSERT [{0}].[{1}] OFF \r\n", table.SchemaName, table.TableName);
            }

            sb.AppendLine("ROLLBACK");

            return sb.ToString();
        }
    }
}
