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
using Vacuum.Core.Storage;

namespace Vacuum.Core.Profiles {
    public interface IProfileService {
        Profile ActiveProfile { get; set; }
        IEnumerable<DocumentHeader> AvailableProfiles { get; }
        bool ProfileExists (string name);
        void Save (Profile profile);
        Profile Load (string name);
        void Remove (string name);
        void ActivateProfile (string name);
    }

    internal class ProfileService : PropertyStateBase, IProfileService {
        private Profile _activeProfile;
        private readonly ObservableCollection<DocumentHeader> _availableProfiles;
        private readonly IDispatchHandler _dispatcher;
        private readonly IStorageService _storageService;

        public ProfileService (IStorageService storageService, IDispatchHandler dispatcher, IEventAggregator eventAggregator) {
            _storageService = storageService;
            _dispatcher = dispatcher;
            _availableProfiles = new ObservableCollection<DocumentHeader> (storageService.LoadHeaders<Profile>());
            if (!AvailableProfiles.Any ()) {
                var profile = new Profile {
                    Name = "Default"
                };

                Save (profile);
                ActiveProfile = profile;
            }
            else {
                ActiveProfile = Load (AvailableProfiles.First ().Name);
            }

            WhenStateUpdated (() => ActiveProfile, () => {
                UpdateProfileBindings ();
                eventAggregator.GetEvent<ActiveProfileChanged> ().Publish (ActiveProfile);
            });
        }

        public Profile ActiveProfile {
            get { return _activeProfile; }
            set { SetProperty (ref _activeProfile, value); }
        }

        public bool ProfileExists (string name) {
            return _availableProfiles.Any (p => p.Name.Equals (name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Save (Profile profile) {
            var isNew = !ProfileExists (profile.Name);
            _storageService.Store (profile);
            if (isNew) {
                _availableProfiles.Add (new DocumentHeader {
                    Id = profile.Id,
                    Name = profile.Name
                });
            }
        }

        public IEnumerable<DocumentHeader> AvailableProfiles => _availableProfiles;

        public Profile Load (string name) {
            if (!ProfileExists (name)) {
                return null;
            }

            return _storageService.Read<Profile> (GetProfileId (name));
        }

        public void Remove (string name) {
            if (name == ActiveProfile.Name) {
                throw new InvalidOperationException ("Cannot remove active profile.");
            }

            _storageService.Remove<Profile> (GetProfileId (name));

        }

        public void ActivateProfile (string name) {
            ActiveProfile = Load (name);
        }

        private string GetProfileId (string name) {
            return _availableProfiles
                .FirstOrDefault (p => p.Name.Equals (name, StringComparison.InvariantCultureIgnoreCase))
                ?.Id;
        }

        private void UpdateProfileBindings () {
            foreach (var set in ActiveProfile.CommandSets) {
            }
        }
    }
}
