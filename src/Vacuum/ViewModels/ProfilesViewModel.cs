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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LightInject;
using Microsoft.Practices.Prism.PubSubEvents;
using Vacuum.Core;
using Vacuum.Core.Profiles;
using Vacuum.Views;

namespace Vacuum.ViewModels {
    public interface IProfilesViewModel {
        IProfileService ProfileService { get; }
        ICommand EditProfile { get; }
        ICommand AddProfile { get; }
        ICommand RemoveProfile { get; }
        string ActiveProfile { get; set; }
    }

    internal class ProfilesViewModel : PropertyStateBase, IProfilesViewModel {
        private string _selectedProfile;
        private readonly IServiceContainer _container;
        private readonly IProfileService _profileService;

        public ProfilesViewModel (IServiceContainer container, IProfileService profileService, IEventAggregator eventAggregator) {
            _container = container;
            _profileService = profileService;
        }

        public string SelectedProfile {
            get { return _selectedProfile; }
            set { SetProperty (ref _selectedProfile, value); }
        }

        public IProfileService ProfileService {
            get { return _profileService; }
        }

        public ICommand EditProfile {
            get { return GetCommand ("EditProfile", ExecuteEditProfile, CanExecuteEditProfile); }
        }

        public ICommand AddProfile {
            get { return GetCommand ("AddProfile", ExecuteAddProfile); }
        }

        public ICommand RemoveProfile {
            get { return GetCommand ("RemoveProfile", ExecuteRemoveProfile, CanExecuteRemoveProfile); }
        }

        public string ActiveProfile {
            get { return _profileService.ActiveProfile.Name; }
            set {
                _profileService.ActivateProfile (value);
                OnPropertyChanged (() => ActiveProfile);
            }
        }

        private void ExecuteEditProfile () {
            ShowEditDialog (SelectedProfile == ActiveProfile ? _profileService.ActiveProfile : _profileService.Load (SelectedProfile));
        }

        private bool CanExecuteEditProfile () {
            return !String.IsNullOrWhiteSpace (SelectedProfile);
        }

        private void ExecuteAddProfile () {
            ShowEditDialog (null);
        }

        private void ExecuteRemoveProfile () {
            var result = MessageBox.Show (String.Format ("Are you sure you want to remove the profile '{0}'", SelectedProfile), "Remove Profile",
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result != MessageBoxResult.Yes) {
                return;
            }

            _profileService.Remove (SelectedProfile);
            SelectedProfile = _profileService.AvailableProfiles.FirstOrDefault ();
        }

        private bool CanExecuteRemoveProfile () {
            return !String.IsNullOrWhiteSpace (SelectedProfile) && SelectedProfile != ActiveProfile;
        }

        private void ShowEditDialog (Profile profile) {
            var editor = _container.GetInstance<Profile, EditProfileView> (profile);
            editor.Owner = Application.Current.MainWindow;
            editor.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            editor.ShowDialog ();
        }
    }
}
