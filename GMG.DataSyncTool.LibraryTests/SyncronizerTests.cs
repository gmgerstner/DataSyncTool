using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace GMG.DataSyncTool.Library.Tests
{
    [TestClass()]
    public class SyncronizerTests
    {
        [TestMethod()]
        public void GenerateScriptTest()
        {
            string sql = "";
            using (Synchronizer sync = new Synchronizer(
                 "Password=fre$hLake27;Persist Security Info=True;User ID=sa;Initial Catalog=USADB;Data Source=sql.hallmarkstevedoring.com",
                 "Password=fre$hLake27;Persist Security Info=True;User ID=sa;Initial Catalog=USADB_STAGE;Data Source=sql.hallmarkstevedoring.com"
                 ))
            {
                sql = sync.GenerateScript();
            }
            Assert.IsTrue(sql.Length > 0);
        }
    }
}
