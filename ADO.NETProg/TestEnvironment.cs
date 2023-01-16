using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace ADO.NETProg
{
    public class TestEnvironment
    {
        /// <summary>
        /// see https://docs.oracle.com/en/database/oracle/oracle-database/12.2/adfns/odbc-driver.html#GUID-618B141E-DD46-4907-99C2-486E801CA878
        /// for more options
        /// </summary>
        public const string OracleOdbcConnectionString
            = "Driver={Oracle 12c ODBC driver};Server=10.10.0.3:1521/sqlalchemy.docker.internal;UID=sqlalchemy;PWD=sqlalchemy";
        
        public const string OracleConnectionString
            = "Data Source=10.10.0.3:1521/pdbmgyard.docker.internal;User Id=sqlalchemy;Password=sqlalchemy";
    }
}