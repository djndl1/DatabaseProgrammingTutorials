using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Data.Common;
using System.Data;

using NUnit.Framework;
using System.Configuration;

namespace ClassicEntityFramework
{
    [SetUpFixture]
    public class TestEnvironment
    {
        public static readonly string OracleConnectionString
            = ConfigurationManager.ConnectionStrings["KeyUserContext"].ConnectionString;

        [OneTimeSetUp]
        public void SetProviders()
        {
            // initialize ADO.NET Providers, a bug perhaps
            DbProviderFactories.GetFactoryClasses();
        }
    }
    
    public class MainApp
    {
        public static async Task Main()
        {
            var test = new KeyUserModel.KeyUserTest();
            
            await test.Insertion();
        }
    }
}