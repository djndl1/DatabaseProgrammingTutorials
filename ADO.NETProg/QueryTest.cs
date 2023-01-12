using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Odbc;
using System.Text.Json;
using Bogus.DataSets;
using ConsoleTableExt;
using NUnit.Framework.Internal;

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
            public string Name { get; set; }

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
    }
}