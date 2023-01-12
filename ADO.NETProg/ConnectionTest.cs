using System.Data.Common;
using System.Data.Odbc;

namespace ADO.NETProg;

public class ConnectionTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Connection_Odbc_OpenAsync()
    {
        using (var connection = new OdbcConnection(TestEnvironment.OracleOdbcConnectionString))
        {
            await connection.OpenAsync();

            Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));

            TestContext.Progress.WriteLine(connection.ServerVersion);
        }
    }
}