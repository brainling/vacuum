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
using System.Collections.ObjectModel;
using System.Linq;
using System.Speech.Recognition;
using Vacuum.Core.Commands;

namespace Vacuum.Core.Speech {
    public interface ISpeechService {
        IEnumerable<string> Messages { get; }
        int IncomingVolume { get; }
        bool IsActive { get; }
        bool IsReady { get; }
        void Message (string msg);
        void Start ();
        void Stop ();
        void ResetBindings ();
        void UpdateBinding (CommandSet set);
    }

    internal class SpeechService : PropertyStateBase, ISpeechService {
        private readonly SpeechRecognitionEngine _engine;
        private readonly Dictionary<string, Grammar> _grammars = new Dictionary<string, Grammar> ();
        private readonly ObservableCollection<string> _messages = new ObservableCollection<string> ();
        private bool _isActive;

        public SpeechService (IDispatchHandler dispatcher) {
            Message ("Initializing speech service...");

            var info = SpeechRecognitionEngine.InstalledRecognizers ().First ();
            _engine = new SpeechRecognitionEngine (info);
            _engine.SetInputToDefaultAudioDevice ();

            _engine.AudioLevelUpdated += (o, e) => OnPropertyChanged (() => IncomingVolume);

            _engine.SpeechRecognitionRejected += (o, e) => { dispatcher.Delegate (() => _messages.Add ($"Unrecognized phrase '{e.Result.Text}'")); };
            _engine.SpeechRecognized += (o, e) => { dispatcher.Delegate (() => _messages.Add ($"Recognized phrase '{e.Result.Text}' {e.Result.Confidence}")); };
            _engine.RecognizeCompleted += (o, e) => { _isActive = false; };
        }

        public bool IsReady => _engine.Grammars.Count > 0;

        public void Message (string msg) {
            _messages.Add (msg);
        }

        public void Start () {
            if (IsActive || _engine.Grammars.Count == 0) {
                return;
            }

            IsActive = true;
            _engine.RecognizeAsync (RecognizeMode.Multiple);
            Message ("Speech recognition active");
        }

        public void Stop () {
            if (!IsActive) {
                return;
            }

            _engine.RecognizeAsyncCancel ();
            Message ("Speech recognition stopped");
        }

        public IEnumerable<string> Messages => _messages;
        public int IncomingVolume => _engine.AudioLevel;

        public bool IsActive {
            get { return _isActive; }
            set { SetProperty (ref _isActive, value); }
        }

        public void ResetBindings () {
            _engine.UnloadAllGrammars ();
            _grammars.Clear ();
        }

        public void UpdateBinding (CommandSet set) {
            Grammar grammar;
            if (_grammars.ContainsKey (set.Name)) {
                grammar = _grammars[set.Name];
                _engine.UnloadGrammar (grammar);
            }

            var builder = new GrammarBuilder ();
            foreach (var phrase in set.Commands.SelectMany (cmd => cmd.Phrases)) {
                builder.Append (phrase);
            }

            grammar = new Grammar (builder);
            _engine.LoadGrammar (grammar);
            _grammars[set.Name] = grammar;
            OnPropertyChanged (() => IsReady);
        }
    }
}
