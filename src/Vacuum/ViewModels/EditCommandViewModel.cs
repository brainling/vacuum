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
using System.Linq;
using System.Windows.Input;
using Vacuum.Core;
using Vacuum.Core.Commands;
using Vacuum.Views;

namespace Vacuum.ViewModels {
    public interface IEditCommandViewModel {
    }

    public class EditCommandViewModel : PropertyStateBase, IEditCommandViewModel {
        public EditCommandViewModel (Command command) {
            IsNew = command == null;
            Command = command == null ? new Command () : command.Clone ();
            Title = IsNew ? "Add New Command" : "Edit Command";
        }

        public bool IsNew { get; }
        public Command Command { get; }
        public string Title { get; private set; }

        public ICommand Save => GetCommand ("Save", ExecuteSave, CanExecuteSave);
        public ICommand Cancel => GetCommand ("Cancel", ExecuteCancel);
        public ICommand AddPhrase => GetCommand ("AddPhrase", ExecuteAddPhrase);

        private void ExecuteSave () {
        }

        private bool CanExecuteSave () {
            return true;
        }

        private void ExecuteCancel () {
        }

        private void ExecuteAddPhrase () {
            var input = VoiceApplication.CurrentContainer.GetInstance<string, string, QuickInputView> ("Add Phrases",
                "Enter a phrase or a set of phrases seperated by a semi-colonn:");
            var result = input.ShowDialog ();
            if (result.HasValue && result.Value) {
                AddPhrases (input.Value);
            }
        }

        private void AddPhrases (string input) {
            if (String.IsNullOrWhiteSpace (input)) {
                return;
            }

            var phrases = input.Split (';');
            foreach (var phrase in phrases.Select (p => p.Trim ()).Where (p => !String.IsNullOrWhiteSpace (p))) {
                Command.Phrases.Add (phrase);
            }
        }
    }
}
