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
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Prism.PubSubEvents;
using NLua;
using Vacuum.Core.Storage;

namespace Vacuum.Core.Commands {
    public interface ICommandService {
        IEnumerable<DocumentHeader> AvailableCommandSets { get; }
        bool CommandSetExists (string name);
        void Save (CommandSet set);
        void Export (string name, string exportPath);
        CommandSet Load (string id);
    }

    internal class CommandService : PropertyStateBase, ICommandService {
        private readonly ObservableCollection<DocumentHeader> _availableCommandSets;
        private readonly IDispatchHandler _dispatcher;
        private readonly IEventAggregator _eventAggregator;
        private readonly IStorageService _storageService;
        private Lua _lua;

        public CommandService (IStorageService storageService, IDispatchHandler dispatcher, IEventAggregator eventAggregator) {
            _storageService = storageService;
            _dispatcher = dispatcher;
            _eventAggregator = eventAggregator;
            _lua = new Lua ();

            _availableCommandSets = new ObservableCollection<DocumentHeader> (storageService.LoadHeaders<CommandSet> ());
            if (!AvailableCommandSets.Any ()) {
                var set = new CommandSet {
                    Name = "My Commands"
                };

                Save (set);
            }
        }

        public bool CommandSetExists (string name) {
            return _availableCommandSets.Any (c => c.Name.Equals (name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Save (CommandSet set) {
            var isNew = !CommandSetExists (set.Name);
            _storageService.Store (set);
            if (isNew) {
                _availableCommandSets.Add (new DocumentHeader {
                    Id = set.Id,
                    Name = set.Name
                });
            }
        }

        public void Export (string name, string exportPath) {
            var set = Load (GetCommandSetId (name));
            if (set == null) {
                return;
            }

            //SaveTo (set, Path.ChangeExtension (exportPath, ".vaccs"));
        }

        public CommandSet Load (string id) {
            return _storageService.Read<CommandSet> (id);
        }

        public IEnumerable<DocumentHeader> AvailableCommandSets => _availableCommandSets;

        private string GetCommandSetId (string name) {
            return _availableCommandSets
                .FirstOrDefault (p => p.Name.Equals (name, StringComparison.InvariantCultureIgnoreCase))
                ?.Id;
        }
    }
}
