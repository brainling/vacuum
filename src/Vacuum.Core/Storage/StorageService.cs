#region License

// Copyright (c) 2015, Matt Holmes
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
using System.IO;
using System.Linq;
using Raven.Abstractions.Extensions;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Vacuum.Core.Storage {
    public class DocumentHeader {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public interface IStorageService {
        IEnumerable<DocumentHeader> LoadHeaders<TDocument> ()
            where TDocument : IDocumentObject;

        void Store<TDocument> (TDocument document)
            where TDocument : IDocumentObject;

        TDocument Read<TDocument> (string id)
            where TDocument : IDocumentObject;

        IEnumerable<TDocument> ReadByName<TDocument> (string name)
            where TDocument : IDocumentObject;

        void Remove<TDocument> (string id)
            where TDocument : IDocumentObject;

        void RemoveByName<TDocument> (string name)
            where TDocument : IDocumentObject;
    }

    internal class StorageService : IStorageService, IDisposable {
        private readonly List<Type> _indexesChecked = new List<Type> ();
        private readonly EmbeddableDocumentStore _store;

        public StorageService ()
            : this ("storage") {
        }

        internal StorageService (string dbName, bool testMode = false) {
            if (!testMode) {
                _store = new EmbeddableDocumentStore {
                    DataDirectory = EnsureFolder (dbName)
                };
            }
            else {
                _store = new EmbeddableDocumentStore {
                    DataDirectory = EnsureFolder (dbName),
                    Conventions = {
                        DefaultQueryingConsistency = ConsistencyOptions.AlwaysWaitForNonStaleResultsAsOfLastWrite
                    }
                };
            }

            _store.Initialize ();
        }

        internal IDocumentStore DocumentStore => _store;

        public void Dispose () {
            _store.Dispose ();
        }

        public IEnumerable<DocumentHeader> LoadHeaders<TDocument> ()
            where TDocument : IDocumentObject {
            CheckIndexes<TDocument> ();

            using (var session = _store.OpenSession ()) {
                return session.Query<TDocument> ($"{typeof(TDocument).Name}/Headers")                            
                              .Select (x => new DocumentHeader {
                                  Id = x.Id,
                                  Name = x.Name
                              }).ToList ();
            }
        }

        public TDocument Read<TDocument> (string id)
            where TDocument : IDocumentObject {
            using (var session = _store.OpenSession ()) {
                return session.Load<TDocument> (id);
            }
        }

        public IEnumerable<TDocument> ReadByName<TDocument> (string name)
            where TDocument : IDocumentObject {
            using (var session = _store.OpenSession ()) {
                return session.Query<TDocument> ().Where (d => d.Name == name).ToList ();
            }
        }

        public void Store<TDocument> (TDocument document)
            where TDocument : IDocumentObject {
            using (var session = _store.OpenSession ()) {
                session.Store (document);
                session.SaveChanges ();
            }
        }

        public void Remove<TDocument> (string id)
            where TDocument : IDocumentObject {
            using (var session = _store.OpenSession ()) {
                session.Delete (session.Load<TDocument> (id));
                session.SaveChanges ();
            }
        }

        public void RemoveByName<TDocument> (string name)
            where TDocument : IDocumentObject {
            using (var session = _store.OpenSession ()) {
                session.Query<TDocument> ().Where (d => d.Name == name).ForEach (d => session.Delete (d));
                session.SaveChanges ();
            }
        }

        private static string EnsureFolder (string dbName) {
            var folder = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "Vacuum");
            if (!Directory.Exists (folder)) {
                Directory.CreateDirectory (folder);
            }

            folder = Path.Combine (folder, dbName);
            if (!Directory.Exists (folder)) {
                Directory.CreateDirectory (folder);
            }

            return folder;
        }

        private void CheckIndexes<TDocument> ()
            where TDocument : IDocumentObject {
            if (_indexesChecked.Contains (typeof (TDocument))) {
                return;
            }

            var builder = new IndexDefinitionBuilder<TDocument> {
                Map = documents =>
                    documents.Select (d => new {
                        d.Id,
                        d.Name
                    })
            };

            var indexName = $"{typeof (TDocument).Name}/Headers";
            if (_store.DatabaseCommands.GetIndex (indexName) != null) {
                return;
            }

            _store.DatabaseCommands.PutIndex (indexName, builder.ToIndexDefinition (_store.Conventions));
            _indexesChecked.Add (typeof (TDocument));
        }
    }
}
