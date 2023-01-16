using System.Collections.ObjectModel;
using System.Data;
using System.Data.Odbc;
using System.Text.Json;
using Bogus.DataSets;
using ConsoleTableExt;
using NUnit.Framework.Internal;
using Oracle.ManagedDataAccess.Client;

namespace ADO.NETProg
{
    [TestFixture]
    public class QueryTest
    {
        [Test]
        public async Task ScalarQuery_Test()
        {
            using var connection = new OdbcConnection(TestEnvironment.OracleOdbcConnectionString);
            connection.Open();

            using var command = new OdbcCommand();
            command.Connection = connection;
            command.CommandText = "SELECT BANNER_FULL FROM \"GV$VERSION\"";

            string fullBanner = (string)command.ExecuteScalar();

            Assert.That(fullBanner, Contains.Substring("Oracle Database"));
        }

        internal class TKeyUser
        {
            public long Id { get; set; }
            public string? Name { get; set; }

            public decimal? MaxSize { get; set; }
        }

        [Test]
        public void DataReader_MultiRow()
        {
            using var connection = new OdbcConnection(TestEnvironment.OracleOdbcConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM \"tkeyuser\"";

            using OdbcDataReader reader = cmd.ExecuteReader();

            List<TKeyUser> buffer = new List<TKeyUser>();
            while (reader.Read())
            {
                var entity = new TKeyUser();
                entity.Id = (long)reader.GetDecimal("iduser");
                entity.Name = reader.GetString("dtName");
                entity.MaxSize = reader.IsDBNull("dtmaxSize") ? null : reader.GetDecimal("dtmaxSize");

                buffer.Add(entity);
            }

            ConsoleTableBuilder.From(buffer).ExportAndWriteLine();

            ReadOnlyCollection<DbColumn> columns = reader.GetColumnSchema();
            var columnTypes = columns.Select(col => new
            {
                ColumnName = col.ColumnName,
                DataType = col.DataType,
                DataTypeName = col.DataTypeName,
                ColumnSize = col.ColumnSize,
                NumericPrecision = col.NumericPrecision,
                NumericScale = col.NumericScale,
            })
                .ToList();

            ConsoleTableBuilder.From(columnTypes).ExportAndWriteLine();
        }

        [Test]
        public void DeleteById()
        {
            using var connection = new OdbcConnection(TestEnvironment.OracleOdbcConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM \"tkeyuser\" WHERE \"iduser\" = 3";

            int i = cmd.ExecuteNonQuery();

            Assert.That(i, Is.EqualTo(1));
        }
        
        [Test]
        public void ParameterizedQuery()
        {
            using var connection = new OdbcConnection(TestEnvironment.OracleOdbcConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            // named parameters are not supported
            cmd.CommandText = "SELECT * FROM \"tkeyuser\" WHERE \"iduser\" = ?";
            cmd.Parameters.AddWithValue(":p1", 1);
            
            using var reader = cmd.ExecuteReader();
            reader.Read();
            
            var entity = new TKeyUser();
            entity.Id = (long)reader.GetDecimal("iduser");
            entity.Name = reader.GetString("dtName");
            entity.MaxSize = reader.IsDBNull("dtmaxSize") ? null : reader.GetDecimal("dtmaxSize");
            
            Assert.That(entity.Id, Is.EqualTo(1));
            Assert.That(entity.Name, Is.EqualTo("Christa"));
            Assert.That(entity.MaxSize, Is.EqualTo(10000));
        }

        [Test]
        public void CallFunction()
        {
            using var connection = new OdbcConnection(TestEnvironment.OracleOdbcConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DECLARE i integer; BEGIN :One := GET_ONE(); END;";
            cmd.CommandType = CommandType.StoredProcedure;
            var output = new OdbcParameter
            {
                ParameterName = ":One",
                DbType = DbType.Decimal,
                Direction = ParameterDirection.Output,
            };
            cmd.Parameters.Add(output);
            cmd.ExecuteNonQuery();

            decimal one = (decimal)output.Value;
            TestContext.Progress.WriteLine(one);
            Assert.That(one, Is.EqualTo(100));
        }
        
        [Test]
        public void DataAdapter_RetrieveData()
        {
            using var connection = new OdbcConnection(TestEnvironment.OracleOdbcConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            // named parameters are not supported
            cmd.CommandText = "SELECT * FROM \"tkeyuser\" WHERE \"iduser\" = ? OR \"dtname\" = ?";
            cmd.Parameters.AddWithValue(":p1", 1);
            cmd.Parameters.AddWithValue(":p2", "NEW_USER");
            
            using var adapter = new OdbcDataAdapter(cmd);
            var table = new DataTable("keyuser");
            adapter.Fill(table);
            
            Assert.That(table.AsEnumerable().Select(r => r.Field<string>("dtname"))
                .All(s => s == "NEW_USER" || s == "Christa"),
                Is.True);
            
            ConsoleTableBuilder.From(table).ExportAndWriteLine();
        }
        
        [Test]
        public void DataAdapter_CommandBuilder()
        {
            // the ODBC provider cannot retrieve column key information
            using var connection = new OracleConnection(TestEnvironment.OracleConnectionString);
            using var adapter = new OracleDataAdapter("SELECT * FROM TKEYUSER", connection);
            using var builder = new OracleCommandBuilder(adapter);

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO TKEYUSER (DT_NAME, DT_MAX_SIZE) VALUES (:p1, :p2) RETURNING ID_USER INTO :p3"; 
            insertCmd.Parameters.Add(":p1", OracleDbType.Varchar2, 40, "DT_NAME");
            insertCmd.Parameters.Add(":p2", OracleDbType.Decimal, 38, "DT_MAX_SIZE");
            //The application can specify which type to return for an output parameter 
            // by setting the DbType property of the output parameter (.NET type) 
            // or the OracleDbType property (ODP.NET type) of the OracleParameter object. 
            // if the output parameter is set as a DbType.String type by setting the DbType property, 
            // the output data is returned as a .NET String type. 
            // if the parameter is set as an OracleDbType.Char type by setting the OracleDbType property, 
            // the output data is returned as an OracleString type. 
            insertCmd.Parameters.Add(new OracleParameter()
            {
                ParameterName = ":p3",
                SourceColumn = "ID_USER",
                Direction = ParameterDirection.Output,
                DbType = DbType.Decimal,
            });
            adapter.InsertCommand = insertCmd;

            var table = new DataTable("TKEYUSER");
            adapter.Fill(table);
            
            Assert.That(table.PrimaryKey.Length, Is.EqualTo(0));
            
            table.PrimaryKey = new DataColumn[] { table.Columns["ID_USER"] };
            
            var christa = table.Rows.Find(1);
            christa!["DT_MAX_SIZE"] = 20000;

            adapter.Update(table);
            christa!["DT_MAX_SIZE"] = 10000;
            adapter.Update(table);
            
            table.AsEnumerable().Where(r => r.Field<string?>("DT_NAME") == "NEW_USER")
                .Select(r => { r.Delete(); return 0; }).ToList();
            
            Enumerable.Range(1, 5).Select(i => 
            {
                var row = table.NewRow();
                row["ID_USER"] = -i;
                row["DT_NAME"] = "NEW_USER";
                table.Rows.Add(row);
                
                return 0;
            }).ToList();
            
            adapter.Update(table);
            
            //Assert.That(table.AsEnumerable().Where(r => r.Field<string?>("DT_NAME") == "NEW_USER")
            //        .All(r => r.Field<decimal>("ID_USER") > 0), Is.True);
            
            ConsoleTableBuilder.From(table).ExportAndWriteLine();
        }
        
        [Test]
        public void DataAdapter_UpdateData()
        {
            using var cmd = new OdbcCommand();
            cmd.CommandText = "SELECT * FROM \"tkeyuser\"";
            using var adapter = new OdbcDataAdapter(cmd);
            
            var insertCmd = new OdbcCommand();
            insertCmd.CommandText = "INSERT INTO \"tkeyuser\" (\"dtname\", \"dtmaxSize\") VALUES (?, ?) RETURNING \"iduser\" INTO ?"; 
            insertCmd.Parameters.Add(":p1", OdbcType.VarChar, 40, "dtname");
            insertCmd.Parameters.Add(":p2", OdbcType.Decimal, 38, "dtmaxSize");
            insertCmd.Parameters.Add(":p3", OdbcType.Decimal, 38, "iduser").Direction = ParameterDirection.Output;
            adapter.InsertCommand = insertCmd;
            
            var updateCmd = new OdbcCommand();
            updateCmd.CommandText = "UPDATE \"tkeyuser\" SET \"dtname\" = ?, \"dtmaxSize\" = ? WHERE \"iduser\" = ?";
            updateCmd.Parameters.Add(":p1", OdbcType.VarChar, 40, "dtname");
            updateCmd.Parameters.Add(":p2", OdbcType.Decimal, 38, "dtmaxSize");
            updateCmd.Parameters.Add(":p3", OdbcType.Decimal, 38, "iduser");
            adapter.UpdateCommand = updateCmd;

            var deleteCmd = new OdbcCommand("DELETE FROM \"tkeyuser\" WHERE \"iduser\" = ?");
            // the row of the current version has been deleted upon updating, use the original version
            deleteCmd.Parameters.Add(":p1", OdbcType.Decimal, 38, "iduser").SourceVersion = DataRowVersion.Original;
            adapter.DeleteCommand = deleteCmd;

            
            using var connection = new OdbcConnection(TestEnvironment.OracleOdbcConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            
            adapter.SelectCommand!.Connection = connection;
            adapter.SelectCommand!.Transaction = transaction;

            adapter.InsertCommand!.Connection = connection;
            adapter.InsertCommand!.Transaction = transaction;

            adapter.UpdateCommand!.Connection = connection;
            adapter.UpdateCommand!.Transaction = transaction;

            adapter.DeleteCommand!.Connection = connection;
            adapter.DeleteCommand!.Transaction = transaction;
            
            var table = new DataTable("keyuser");
            adapter.Fill(table);
            
            Assert.That(table.PrimaryKey.Length, Is.EqualTo(0));
            
            table.PrimaryKey = new DataColumn[] { table.Columns["iduser"] };
            
            var christa = table.Rows.Find(1);
            christa!["dtmaxSize"] = 20000;

            adapter.Update(table);
            christa!["dtmaxSize"] = 10000;
            adapter.Update(table);
            
            table.AsEnumerable().Where(r => r.Field<string?>("dtname") == "NEW_USER")
                .Select(r => { r.Delete(); return 0; }).ToList();
            
            Enumerable.Range(1, 20).Select(i => 
            {
                var row = table.NewRow();
                row["iduser"] = -i;
                row["dtname"] = "NEW_USER";
                table.Rows.Add(row);
                
                return 0;
            }).ToList();
            
            adapter.Update(table);
            transaction.Commit();
            
            Assert.That(table.AsEnumerable().Where(r => r.Field<string?>("dtname") == "NEW_USER")
                    .All(r => r.Field<decimal>("iduser") > 0), Is.True);
            
            ConsoleTableBuilder.From(table).ExportAndWriteLine();
        }
    }
}