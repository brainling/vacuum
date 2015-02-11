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
using System.Windows.Input;
using Vacuum.Core;
using Vacuum.Core.Profiles;

namespace Vacuum.ViewModels {
    public interface IEditProfileView {
        void Ok ();
        void Cancel ();
    }

    public interface IEditProfileViewModel {
        IEditProfileView View { get; set; }
        bool IsNew { get; }
        Profile Profile { get; }
        string Title { get; }
        ICommand Save { get; }
        ICommand Cancel { get; }
    }

    public class EditProfileViewModel : PropertyStateBase, IEditProfileViewModel {
        private readonly IProfileService _profileService;

        public EditProfileViewModel (Profile profile, IProfileService profileService) {
            IsNew = profile == null;
            Profile = profile ?? new Profile ();
            Title = IsNew ? "New Profile" : String.Format ("Edit Profile - {0}", profile.Name);

            _profileService = profileService;
        }

        public IEditProfileView View { get; set; }

        public bool IsNew { get; private set; }
        public Profile Profile { get; private set; }
        public string Title { get; private set; }

        public ICommand Save {
            get { return GetCommand ("Save", ExecuteSave, CanExecuteSave); }
        }

        public ICommand Cancel {
            get { return GetCommand ("Cancel", ExecuteCancel); }
        }

        private void ExecuteSave () {
            _profileService.Save (Profile);
            View.Ok ();
        }

        private bool CanExecuteSave () {
            return !String.IsNullOrWhiteSpace (Profile.Name) && Profile["Name"] == null;
        }

        private void ExecuteCancel () {
            View.Cancel ();
        }
    }
}