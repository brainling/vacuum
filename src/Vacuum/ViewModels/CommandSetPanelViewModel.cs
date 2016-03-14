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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Win32;
using Vacuum.Core;
using Vacuum.Core.Commands;
using Vacuum.Core.Profiles;
using Vacuum.Core.Storage;

namespace Vacuum.ViewModels {
    public class CommandSetSelection : BindableBase {
        private readonly IProfileService _profileService;
        private Profile _profile;

        public CommandSetSelection (DocumentHeader commandSet, Profile profile, IProfileService profileService) {
            Id = commandSet.Id;
            Name = commandSet.Name;

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

        public string Id { get; }
        public string Name { get; }

        public bool IsSelected {
            get { return _profile.CommandSets.ContainsKey (Name); }
            set {
                var contains = _profile.CommandSets.ContainsKey (Name);
                if ((contains && value) || (!contains && !value)) {
                    return;
                }

                if (value) {
                    _profile.CommandSets.Add (Name, true);
                }
                else {
                    _profile.CommandSets.Remove (Name);
                }

                _profileService.Save (_profile);
                OnPropertyChanged (() => IsSelected);
            }
        }
    }

    public interface ICommandSetPanelView {
    }

    public interface ICommandSetPanelViewModel {
        ICommand NewCommandSet { get; }
        ICommand EditCommandSet { get; }
        IEnumerable<CommandSetSelection> CommandSets { get; }
    }

    public class CommandSetPanelViewModel : PropertyStateBase, ICommandSetPanelViewModel {
        private readonly ICommandService _commandService;
        private readonly List<CommandSetSelection> _commandSets = new List<CommandSetSelection> ();
        private readonly IProfileService _profileService;
        private CommandSetSelection _selectedCommandSet;

        public CommandSetPanelViewModel (IProfileService profileService, ICommandService commandService, IEventAggregator eventAggregator) {
            _profileService = profileService;
            _commandService = commandService;

            eventAggregator.GetEvent<ActiveProfileChanged> ().Subscribe (p => { _commandSets.ForEach (cs => { cs.Profile = p; }); });
            _commandService.AvailableCommandSets.ToList ().ForEach (cs => { _commandSets.Add (new CommandSetSelection (cs, _profileService.ActiveProfile, _profileService)); });

            SelectedCommandSet = CommandSets.FirstOrDefault ();
        }

        public ICommand ImportCommandSet => GetCommand ("Import", ExecuteImportCommandSet);
        public ICommand ExportCommandSet => GetCommand ("Export", ExecuteExportCommandSet, CanExecuteExportCommandSet);

        public CommandSetSelection SelectedCommandSet {
            get { return _selectedCommandSet; }
            set { SetProperty (ref _selectedCommandSet, value); }
        }

        public ICommand NewCommandSet => GetCommand ("New", ExecuteNewCommandSet, CanExecuteExportCommandSet);
        public ICommand EditCommandSet => GetCommand ("Edit", ExecuteEditCommandSet, CanExecuteEditCommandSet);

        public IEnumerable<CommandSetSelection> CommandSets => _commandSets;

        private bool CanExecuteExportCommandSet () {
            return SelectedCommandSet != null;
        }

        private bool CanExecuteEditCommandSet () {
            return SelectedCommandSet != null;
        }

        private void ExecuteNewCommandSet () {
        }

        private void ExecuteEditCommandSet () {
            var set = _commandService.Load (SelectedCommandSet.Id);
            var editor = ((VoiceApplication) Application.Current).Container.GetInstance<CommandSet, ICommandSetEditorView> (set);
            editor.Edit ();
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
