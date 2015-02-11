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
using System.ComponentModel;
using Newtonsoft.Json;

namespace Vacuum.Core.Profiles {
    public class Profile : PropertyStateBase, IDataErrorInfo {
        private string _name;
        private string _originalName;

        public Profile () {
            CommandSets = new Dictionary<string, bool> ();
        }

        [JsonIgnore]
        public string Name {
            get { return _name; }
            set {
                SetProperty (ref _name, value);
                if (_originalName == null) {
                    _originalName = _name;
                }
            }
        }

        [JsonProperty ("commandSets")]
        public Dictionary<string, bool> CommandSets { get; private set; }

        public override string ToString () {
            return Name;
        }

        public string this [string columnName] {
            get {
                if (columnName != "Name") {
                    return null;
                }

                if (String.IsNullOrWhiteSpace (Name)) {
                    return "Please enter a valid profile name.";
                }
                if (Name != _originalName && VoiceApplication.CurrentContainer.GetInstance<IProfileService> ().ProfileExists (Name)) {
                    return "Profile with this name already exists.";
                }

                return null;
            }
        }

        [JsonIgnore]
        public string Error {
            get { return null; }
        }
    }
}
