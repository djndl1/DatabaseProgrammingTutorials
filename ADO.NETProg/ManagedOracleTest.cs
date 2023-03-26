using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using ConsoleTableExt;
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

        int insertedCount = cmd.ExecuteNonQuery();

        Assert.That(insertedCount, Is.EqualTo(cmd.ArrayBindCount));
    }

    [Test]
    public void TimestampTest()
    {
        using var connection = (OracleConnection) OracleClientFactory.Instance.CreateConnection();
        connection.ConnectionString = TestEnvironment.OracleConnectionString;
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT TS_TZ FROM SQLALCHEMY.TS_TEST WHERE TS_TZ IS NOT NULL";

        using var reader = cmd.ExecuteReader();
        reader.Read();

        DateTimeOffset time = reader.GetDateTimeOffset(0);
        OracleTimeStampTZ timestampTZ = reader.GetOracleTimeStampTZ(0);
        object unknownType = reader[0];


        string iso = time.ToString("O");
        TestContext.Out.WriteLine(iso);
        TestContext.Out.WriteLine(timestampTZ);
        TestContext.Out.WriteLine(unknownType.GetType()); // returned as a DateTime by default
    }

    [Test]
    public void IntervalTest()
    {
        using var connection = (OracleConnection) OracleClientFactory.Instance.CreateConnection();
        connection.ConnectionString = TestEnvironment.OracleConnectionString;
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT INTERVAL_SPAN FROM SQLALCHEMY.TS_TEST WHERE TS_TZ IS NOT NULL";

        TimeSpan span = (TimeSpan)cmd.ExecuteScalar();

        TestContext.Out.WriteLine(span);
    }

    [Test]
    public void MultipleTableRead()
    {
        using var connection = (OracleConnection) OracleClientFactory.Instance.CreateConnection();
        connection.ConnectionString = TestEnvironment.OracleConnectionString;
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.BindByName = true;
        cmd.CommandText = @"
            BEGIN
                OPEN :R1 FOR SELECT * FROM SQLALCHEMY.TS_TEST;
                OPEN :R2 FOR SELECT * FROM SQLALCHEMY.ADDRESS;
            END;
        ";
        cmd.Parameters.Add("R1", OracleDbType.RefCursor, ParameterDirection.Output);
        cmd.Parameters.Add("R2", OracleDbType.RefCursor, ParameterDirection.Output);

        var reader = cmd.ExecuteReader();

        reader.Read();

        DataTable ts_schema = reader.GetSchemaTable();
        ConsoleTableBuilder.From(ts_schema).ExportAndWriteLine();

        reader.NextResult();

        DataTable addr_schema = reader.GetSchemaTable();
        ConsoleTableBuilder.From(addr_schema).ExportAndWriteLine();
    }
}