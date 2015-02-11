﻿#region License

// Copyright (c) 2011, Matt Holmes
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the project nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT  LIMITED TO, THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
// THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT  LIMITED TO, PROCUREMENT 
// OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR 
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Reflection;

namespace Vacuum.Core.Storage {
    public class StorageDocument {
        public long Id { get; set; }
        public string Collection { get; set; }
        public string Name { get; set; }
        public string Document { get; set; }
    }

    public class SqliteConfiguration : DbConfiguration {
        public SqliteConfiguration () {
            SetDefaultConnectionFactory (new SqliteConnectionFactory ());
            SetProviderFactory ("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory ("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            var t = Type.GetType ("System.Data.SQLite.EF6.SQLiteProviderServices, System.Data.SQLite.EF6");
            var fi = t.GetField ("Instance", BindingFlags.NonPublic | BindingFlags.Static);
            SetProviderServices ("System.Data.SQLite", (DbProviderServices) fi.GetValue (null));
        }
    }

    [DbConfigurationType (typeof (SqliteConfiguration))]
    public class StorageContext : DbContext {
        public StorageContext (string connectionString)
            : base (connectionString) {
            Database.SetInitializer<StorageContext> (null);
        }

        public DbSet<StorageDocument> Documents { get; set; }
    }
}
