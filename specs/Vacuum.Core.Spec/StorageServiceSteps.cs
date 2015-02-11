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
using System.IO;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Vacuum.Core.Storage;

namespace Vacuum.Core.Spec {
    [Binding]
    public class StorageServiceSteps {
        private static IStorageService _storageService;
        private string _documentCollection;
        private string _documentName;

        [AfterFeature ("storage")]
        public static void AfterFeature () {
            ((StorageService) _storageService).Dispose ();

            // Ugly hack to clean up the test db
            var start = DateTimeOffset.UtcNow;
            while (true) {
                try {
                    File.Delete (StorageService.GetDbFile ("test"));
                    break;
                }
                catch {
                    if ((DateTimeOffset.UtcNow - start).Seconds > 30) {
                        throw;
                    }
                }
            }
        }

        [Given ("storage service")]
        public void GivenStorageService () {
            _storageService = new StorageService ("test");
        }

        [Given (@"document '(.*)' in collection '(.*)'")]
        public void GivenDocumentInCollection (string p0, string p1) {
            _documentName = p0;
            _documentCollection = p1;
        }

        [When ("document is stored")]
        public void WhenDocumentIsStored () {
            _storageService.StoreDocument (_documentCollection, _documentName, new Object ());
        }

        [Then (@"document '(.*)' in collection '(.*)' exists")]
        public void ThenDocumentInCollectionExists (string p0, string p1) {
            Assert.NotNull (_storageService.ReadDocument<object> (p1, p0));
        }
    }
}
