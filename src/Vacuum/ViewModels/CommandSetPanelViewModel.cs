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

using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Win32;
using Vacuum.Core;
using Vacuum.Core.Commands;
using Vacuum.Core.Profiles;
using Vacuum.Views;

namespace Vacuum.ViewModels {
    public class CommandSetSelection : BindableBase {
        private Profile _profile;
        private readonly string _commandSet;
        private readonly IProfileService _profileService;

        public CommandSetSelection (string commandSet, Profile profile, IProfileService profileService) {
            _commandSet = commandSet;
            _profile = profile;
            _profileService = profileService;

            PropertyChanged += (o, e) => {
                if (e.PropertyName != "Profile") {
                    return;
                }

                OnPropertyChanged (() => IsSelected);
            };
        }

        public Profile Profile {
            get { return _profile; }
            set { SetProperty (ref _profile, value); }
        }

        public string Name {
            get { return _commandSet; }
        }

        public bool IsSelected {
            get { return _profile.CommandSets.ContainsKey (_commandSet); }
            set {
                var contains = _profile.CommandSets.ContainsKey (_commandSet);
                if ((contains && value) || (!contains && !value)) {
                    return;
                }

                if (value) {
                    _profile.CommandSets.Add (_commandSet, true);
                }
                else {
                    _profile.CommandSets.Remove (_commandSet);
                }

                _profileService.Save (_profile);
                OnPropertyChanged (() => IsSelected);
            }
        }
    }

    public interface ICommandSetPanelViewModel {
        ICommand NewCommandSet { get; }
        ICommand EditCommandSet { get; }
        IEnumerable<CommandSetSelection> CommandSets { get; }
    }

    public class CommandSetPanelViewModel : PropertyStateBase, ICommandSetPanelViewModel {
        private DelegateCommand _editCommandSet;
        private DelegateCommand _exportCommandSet;
        private DelegateCommand _importCommandSet;
        private DelegateCommand _newCommandSet;
        private CommandSetSelection _selectedCommandSet;
        private readonly ICommandService _commandService;
        private readonly List<CommandSetSelection> _commandSets = new List<CommandSetSelection> ();
        private readonly IProfileService _profileService;

        public CommandSetPanelViewModel (IProfileService profileService, ICommandService commandService, IEventAggregator eventAggregator) {
            _profileService = profileService;
            _commandService = commandService;

            eventAggregator.GetEvent<ActiveProfileChanged> ().Subscribe (p => { _commandSets.ForEach (cs => { cs.Profile = p; }); });
            _commandService.AvailableCommandSets.ToList ().ForEach (cs => { _commandSets.Add (new CommandSetSelection (cs, _profileService.ActiveProfile, _profileService)); });

            SelectedCommandSet = CommandSets.FirstOrDefault ();

            WhenStateUpdated (() => SelectedCommandSet, () => {
                _editCommandSet.RaiseCanExecuteChanged ();
                _exportCommandSet.RaiseCanExecuteChanged ();
            });
        }

        public ICommand ImportCommandSet {
            get { return _importCommandSet ?? (_importCommandSet = new DelegateCommand (ExecuteImportCommandSet)); }
        }

        public ICommand ExportCommandSet {
            get { return _exportCommandSet ?? (_exportCommandSet = new DelegateCommand (ExecuteExportCommandSet, CanExecuteExportCommandSet)); }
        }

        public CommandSetSelection SelectedCommandSet {
            get { return _selectedCommandSet; }
            set { SetProperty (ref _selectedCommandSet, value); }
        }

        public ICommand NewCommandSet {
            get { return _newCommandSet ?? (_newCommandSet = new DelegateCommand (ExecuteNewCommandSetCommand)); }
        }

        public IEnumerable<CommandSetSelection> CommandSets {
            get { return _commandSets; }
        }

        public ICommand EditCommandSet {
            get { return _editCommandSet ?? (_editCommandSet = new DelegateCommand (ExecuteEditCommandSetCommand, CanExecuteEditCommandSet)); }
        }

        private bool CanExecuteExportCommandSet () {
            return SelectedCommandSet != null;
        }

        private bool CanExecuteEditCommandSet () {
            return SelectedCommandSet != null;
        }

        private void ExecuteNewCommandSetCommand () {
        }

        private void ExecuteEditCommandSetCommand () {
            var set = _commandService.Load (SelectedCommandSet.Name);
            var editor = ((VoiceApplication) Application.Current).Container.GetInstance<CommandSet, CommandSetEditorView> (set);
            editor.Owner = Application.Current.MainWindow;
            editor.Show ();
        }

        private void ExecuteImportCommandSet () {
            var diag = new OpenFileDialog {
                Filter = "Vacuum Command Set|*.vaccs",
                Multiselect = false
            };

            var result = diag.ShowDialog ();
            if (result.HasValue && result.Value) {
            }
        }

        private void ExecuteExportCommandSet () {
            var diag = new SaveFileDialog {
                Filter = "Vacuum Command Set|*.vaccs"
            };

            var result = diag.ShowDialog ();
            if (result.HasValue && result.Value) {
                _commandService.Export (SelectedCommandSet.Name, diag.FileName);
            }
        }
    }
}
