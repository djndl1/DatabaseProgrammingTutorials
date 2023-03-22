using System.Globalization;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace ADO.NETProg;


[TestFixture]
public class ManagedOracleTest
{
    [Test]
    public void MultipleParameterBinding()
    {
        using var connection = new OracleConnection(TestEnvironment.OracleConnectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();

        var names = new string[] { "OLD_USER", "OLD_USER" };
        var sizes = new int[] { 1, 2 };

        cmd.CommandText = "INSERT INTO TKEYUSER (DT_NAME, DT_MAX_SIZE) VALUES (:DtName, :DtMaxSize)";
        cmd.BindByName  = true; // uses positional arguments by default

        cmd.ArrayBindCount = names.Length;
        cmd.Parameters.Add(":DtMaxSize", OracleDbType.Int64).Value = sizes;
        cmd.Parameters.Add(":DtName", OracleDbType.Varchar2).Value = names;

        cmd.ExecuteNonQuery();
    }

    [Test]
    public void TimestampTest()
    {
        using var connection = new OracleConnection(TestEnvironment.OracleConnectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT TS_TZ FROM SQLALCHEMY.TS_TEST WHERE TS_TZ IS NOT NULL";

        using var reader = cmd.ExecuteReader();
        reader.Read();

        DateTimeOffset time = reader.GetDateTimeOffset(0);
        OracleTimeStampTZ timestampTZ = reader.GetOracleTimeStampTZ(0);


        string iso = time.ToString("O");
        TestContext.Out.WriteLine(iso);
        TestContext.Out.WriteLine(timestampTZ);
    }
}