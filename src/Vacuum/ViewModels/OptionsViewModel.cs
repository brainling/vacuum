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
using System.Speech.Recognition;
using System.Speech.Synthesis;
using Vacuum.Core;
using Vacuum.Core.Configuration;
using Vacuum.Core.Ui;

namespace Vacuum.ViewModels {
    public interface IOptionsView {
        void CloseDialog (bool? result);
    }

    public interface IOptionsViewModel {
        IEnumerable<RecognizerInfo> Engines { get; }
        IEnumerable<InstalledVoice> Voices { get; }
        IOptionsView View { get; set; }
        Options Options { get; set; }
    }

    public class OptionsViewModel : PropertyStateBase, IOptionsViewModel, IFlyoutViewModel {
        private readonly IOptionsService _optionsService;

        public OptionsViewModel (IOptionsService optionsService) {
            _optionsService = optionsService;
            Options = optionsService.Get ().Clone ();
            Engines = SpeechRecognitionEngine.InstalledRecognizers ();

            using (var synth = new SpeechSynthesizer ()) {
                Voices = synth.GetInstalledVoices ();
            }

            Options.PropertyChanged += (o, e) => OptionsChanged ();
            Options.ScriptEditor.PropertyChanged += (o, e) => OptionsChanged ();
        }

        private bool IsDirty { get; set; }

        void IFlyoutViewModel.Closing () {
            _optionsService.Update (Options);
        }

        void IFlyoutViewModel.Closed () {
        }

        public Options Options { get; set; }
        public IEnumerable<RecognizerInfo> Engines { get; private set; }
        public IEnumerable<InstalledVoice> Voices { get; private set; }
        public IOptionsView View { get; set; }

        private void OptionsChanged () {
            _optionsService.Update (Options);
        }
    }
}
