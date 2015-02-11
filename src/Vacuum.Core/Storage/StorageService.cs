#region License

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
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Vacuum.Core.Storage {
    public interface IStorageService {
        IEnumerable<string> LoadDocumentNames (string collection);
        void StoreDocument<TDocument> (string collection, string name, TDocument document);
        TDocument ReadDocument<TDocument> (string collection, string name);
        void RemoveDocument (string collection, string name);
    }

    internal class StorageService : IStorageService, IDisposable {
        private static readonly Action<object, string> Nop = (o, s) => { };
        private readonly StorageContext _context;
        private readonly Dictionary<Type, Action<object, string>> _nameSetterCache = new Dictionary<Type, Action<object, string>> ();

        public StorageService ()
            : this ("storage") {
        }

        internal StorageService (string dbName) {
            var dbFile = GetDbFile (dbName);
            if (!File.Exists (dbFile)) {
                CreateStorageDatabase (dbFile);
            }

            _context = new StorageContext (String.Format ("Data Source={0}", dbFile));
        }

        public void Dispose () {
            _context.Dispose ();
        }

        public IEnumerable<string> LoadDocumentNames (string collection) {
            return _context.Database.SqlQuery<string> ("SELECT Name FROM StorageDocuments WHERE Collection = @p0", collection).ToList ();
        }

        public TDocument ReadDocument<TDocument> (string collection, string name) {
            var storageDocument = _context.Documents.FirstOrDefault (d => d.Collection == collection && d.Name == name);
            if (storageDocument == null) {
                return default(TDocument);
            }

            var document = JsonConvert.DeserializeObject<TDocument> (storageDocument.Document);
            if (!_nameSetterCache.ContainsKey (typeof (TDocument))) {
                _nameSetterCache[typeof (TDocument)] = GetNameSetter (typeof (TDocument));
            }

            _nameSetterCache[typeof (TDocument)] (document, storageDocument.Name);
            return document;
        }

        public void StoreDocument<TDocument> (string collection, string name, TDocument document) {
            var storageDocument = _context.Documents.FirstOrDefault (d => d.Collection == collection && d.Name == name);
            if (storageDocument == null) {
                storageDocument = new StorageDocument {
                    Collection = collection,
                    Name = name
                };
                _context.Documents.Add (storageDocument);
            }

            storageDocument.Document = JsonConvert.SerializeObject (document);
            _context.SaveChanges ();
        }

        public void RemoveDocument (string collection, string name) {
            var storageDocument = _context.Documents.FirstOrDefault (d => d.Collection == collection && d.Name == name);
            if (storageDocument == null) {
                return;
            }

            _context.Documents.Remove (storageDocument);
            _context.SaveChanges ();
        }

        private static string EnsureFolder () {
            var folder = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "Vacuum");
            if (!Directory.Exists (folder)) {
                Directory.CreateDirectory (folder);
            }

            return folder;
        }

        private string GetConnectionString (string dbFile) {
            return (new SQLiteConnectionStringBuilder {
                DataSource = dbFile,
                Version = 3
            }).ToString ();
        }

        private void CreateStorageDatabase (string dbFile) {
            using (var conn = new SQLiteConnection (GetConnectionString (dbFile))) {
                conn.Open ();

                var cmd = new SQLiteCommand (@"
CREATE TABLE StorageDocuments
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 
    Collection TEXT NOT NULL COLLATE NOCASE, 
    Name TEXT NOT NULL COLLATE NOCASE, 
    Document TEXT NOT NULL 
)", conn);
                cmd.ExecuteNonQuery ();
            }
        }

        private Action<object, string> GetNameSetter (Type t) {
            var query = from p in t.GetProperties (BindingFlags.Public | BindingFlags.Instance)
                where
                    p.Name == "Name" &&
                    p.CanWrite &&
                    p.PropertyType == typeof (string)
                select
                    p;

            var prop = query.FirstOrDefault ();
            if (prop == null) {
                return Nop;
            }

            return (o, s) => { prop.SetValue (o, s); };
        }

        internal static string GetDbFile (string dbName) {
            return Path.Combine (EnsureFolder (), String.Format ("{0}.db", dbName));
        }
    }
}
