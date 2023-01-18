using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using Oracle.ManagedDataAccess.EntityFramework;
using Oracle.ManagedDataAccess.Client;
using NUnit.Framework;
using System.Configuration;
using System.Data;
using ConsoleTableExt;
using System.Data.Common;

namespace ClassicEntityFramework.KeyUserModel
{
    public class KeyUser
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long? MaxSize { get; set; }
    }
}