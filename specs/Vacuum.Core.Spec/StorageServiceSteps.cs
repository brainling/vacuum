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
using System.Threading;
using NUnit.Framework;
using Raven.Abstractions.Extensions;
using TechTalk.SpecFlow;
using Vacuum.Core.Storage;

namespace Vacuum.Core.Spec {
    class TestDocument : IDocumentObject {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    [Binding]
    public class StorageServiceSteps {
        private static IStorageService _storageService;
        private IEnumerable<DocumentHeader> _headers;
        private TestDocument _lastDocument;

        [AfterScenario ("storage")]
        public static void AfterScenario () {
            ((StorageService) _storageService).Dispose ();
        }

        [AfterFeature ("storage")]
        public static void AfterFeature () {
            var start = DateTime.UtcNow;
            while (true) {
                try {
                    Directory.Delete (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "Vacuum/test"), true);
                    break;
                }
                catch (Exception) {
                    if ((DateTime.UtcNow - start).TotalSeconds > 30) {
                        break;
                    }

                    Thread.Sleep (500);
                }
            }
        }

        [Given ("storage service")]
        public void GivenStorageService () {
            _storageService = new StorageService ("test", true);
        }

        [When (@"document headers are loaded")]
        public void WhenDocumentHeadersAreLoaded () {
            _headers = _storageService.LoadHeaders<TestDocument> ();
        }


        [Then (@"document '(.*)' exists")]
        public void ThenDocumentExists (string p0) {
            var documents = _storageService.ReadByName<TestDocument> (p0);
            Assert.IsTrue (documents.Any ());
        }

        [Given (@"document '(.*)' has been created")]
        public void GivenDocumentAlreadyExists (string p0) {
            _lastDocument = new TestDocument {
                Name = p0
            };
            _storageService.Store (_lastDocument);
        }

        [When (@"document '(.*)' is deleted")]
        public void WhenDocumentIsDeleted (string p0) {
            _storageService.RemoveByName<TestDocument> (p0);
        }

        [Then (@"document '(.*)' does not exist")]
        public void ThenDocumentDoesNotExist (string p0) {
            var documents = _storageService.ReadByName<TestDocument> (p0);
            Assert.IsFalse (documents.Any ());
        }

        [Then (@"document with id exists")]
        public void ThenDocumentWithIdExists () {
            Assert.IsNotNull (_storageService.Read<TestDocument> (_lastDocument.Id));
        }

        [When (@"document is deleted by id")]
        public void WhenDocumentIsDeletedById () {
            _storageService.Remove<TestDocument> (_lastDocument.Id);
        }

        [Then (@"document with id does not exist")]
        public void ThenDocumentWithIdDoesNotExist () {
            Assert.IsNull (_storageService.Read<TestDocument> (_lastDocument.Id));
        }

        [Then (@"headers (.*)")]
        public void ThenDocumentsExist (string[] tests) {
            tests.ForEach (t => Assert.IsTrue (_headers.Any (h => String.Equals (h.Name, t, StringComparison.InvariantCultureIgnoreCase))));
        }

        [StepArgumentTransformation (@"contains '(.*)'")]
        public string[] CommaSeperatedList (string list) {
            return list.Split (',').Select (s => s.Trim ()).ToArray ();
        }
    }
}
