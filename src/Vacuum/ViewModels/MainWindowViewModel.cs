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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LightInject;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Vacuum.Core;
using Vacuum.Core.Profiles;
using Vacuum.Core.Speech;
using Vacuum.Core.Ui;
using Vacuum.Views;

namespace Vacuum.ViewModels {
    public interface IMainWindowView {
        void Close ();
    }

    public interface IMainWindowViewModel {
        IProfileService ProfileService { get; }
        string ActiveProfile { get; set; }
        IMainWindowView View { get; set; }
        ICommand Exit { get; }
        ICommand Options { get; }
        ICommand EditProfiles { get; }
        string Title { get; set; }
    }

    public class ProfileViewModel : BindableBase {
        private bool _isActive;
        public string Name { get; set; }

        public bool IsActive {
            get { return _isActive; }
            set { SetProperty (ref _isActive, value); }
        }
    }

    public class MainWindowViewModel : PropertyStateBase, IMainWindowViewModel {
        private string _activeProfile;
        private string _title;
        private readonly IServiceContainer _container;
        private readonly IFlyoutService _flyoutService;
        private readonly IProfileService _profileService;

        public MainWindowViewModel (IServiceContainer container, IProfileService profileService, IFlyoutService flyoutService,
            ISpeechService speechService, IEventAggregator eventAggregator) {
            _container = container;
            _profileService = profileService;
            _flyoutService = flyoutService;

            ActiveProfile = _profileService.ActiveProfile.Name;
            Title = String.Format ("Vacuum - {0}", ActiveProfile);

            eventAggregator.GetEvent<ActiveProfileChanged> ().Subscribe (p => {
                ActiveProfile = p.Name;
            });

            WhenStateUpdated (() => ActiveProfile, () => {
                _profileService.ActivateProfile (ActiveProfile);
                Title = String.Format ("Vacuum - {0}", ActiveProfile);
            });

            speechService.Start ();
        }

        public IProfileService ProfileService {
            get { return _profileService; }
        }

        public ICommand EditProfiles {
            get { return GetCommand ("EditProfiles", ExecuteEditProfiles); }
        }

        public ICommand Options {
            get { return GetCommand ("Options", ExecuteOptions); }
        }

        public string ActiveProfile {
            get { return _activeProfile; }
            set { SetProperty (ref _activeProfile, value); }
        }

        public IMainWindowView View { get; set; }

        public ICommand Exit {
            get { return GetCommand ("Exit", ExecuteExit); }
        }

        public string Title {
            get { return _title; }
            set { SetProperty (ref _title, value); }
        }

        private void ExecuteExit () {
            View.Close ();
        }

        private void ExecuteOptions () {
            var options = _container.GetInstance<IOptionsView> ();
            _flyoutService.ShowFlyout ("Settings", (FrameworkElement) options, width: 375);
        }

        private void ExecuteEditProfiles () {
            _flyoutService.ShowFlyout ("Profiles", _container.GetInstance<ProfilesView> ());
        }
    }
}
