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
                 "Data Source=FTWI1-MSSQL1\\TEST;Initial Catalog=FormFoxTestLang;Integrated Security=true;Pooling=False;Persist Security Info=True;",
                 "Data Source=(local);Initial Catalog=FormFox;Integrated Security=true;Pooling=False;Persist Security Info=True;"
                 ))
            {
                sql = sync.GenerateScript();
            }
            Assert.IsTrue(sql.Length > 0);
        }
    }
}
