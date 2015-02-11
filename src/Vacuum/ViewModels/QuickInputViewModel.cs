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

using System.Windows.Input;
using Vacuum.Core;

namespace Vacuum.ViewModels {
    public interface IQuickInputView {
        void Ok ();
        void Cancel ();
    }

    public interface IQuickInputViewModel {
        IQuickInputView View { get; set; }
        string Title { get; }
        string Prompt { get; }
        string Value { get; set; }
    }

    public class QuickInputViewModel : PropertyStateBase, IQuickInputViewModel {
        private string _value;

        public QuickInputViewModel (string title, string prompt) {
            Title = title;
            Prompt = prompt;
        }

        public IQuickInputView View { get; set; }

        public ICommand Save {
            get { return GetCommand ("Save", ExecuteSave); }
        }

        public ICommand Cancel {
            get { return GetCommand ("Cancel", ExecuteCancel); }
        }

        public string Title { get; private set; }
        public string Prompt { get; private set; }

        public string Value {
            get { return _value; }
            set { SetProperty (ref _value, value); }
        }

        private void ExecuteSave () {
            View.Ok ();
        }

        private void ExecuteCancel () {
            View.Cancel ();
        }
    }
}
