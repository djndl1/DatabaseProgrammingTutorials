using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Data.Odbc;

using NUnit.Framework;

namespace ADO.NET.ODBC
{
    [TestFixture]
    public class TestConnection
    {
        private const string SqlAlchemyDS = "DSN=sqlalchemy_test;Uid=sqlalchemy;Pwd=sqlalchemy";

        [Test]
        public void SimpleConnection()
        {
            using var conn = new OdbcConnection(SqlAlchemyDS);

            conn.Open();

            TestContext.Progress.WriteLine(
                $"Connected to Database {conn.Database} with Driver {conn.Driver}"
                + $" Version {conn.ServerVersion} on {conn.DataSource}");
        }
    }
}