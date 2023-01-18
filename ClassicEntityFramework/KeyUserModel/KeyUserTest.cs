using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using Oracle.ManagedDataAccess.EntityFramework;
using Oracle.ManagedDataAccess.Client;
using NUnit.Framework;
using System.Reflection;
using System.Configuration;
using System.Data;
using ConsoleTableExt;
using System.Data.Common;

namespace ClassicEntityFramework.KeyUserModel
{
    
    [TestFixture]
    public class KeyUserTest
    {
        [Test]
        public async Task Insertion()
        {
            var user = new KeyUser
            {
                Id = -1,
                Name = "EF_USER",
            };
            
            using (var ctx = new KeyUserContext())
            {
                ctx.KeyUsers.Add(user);

                await ctx.SaveChangesAsync();
            }
            Console.WriteLine(user.Id);
            Assert.That(user.Id, Is.GreaterThan(0));
        }
    }
}