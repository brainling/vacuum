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
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using LightInject;
using Microsoft.Practices.Prism.PubSubEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Vacuum.Core.Configuration {
    public interface IOptionsService {
        Options Get ();
        void Update (Options options);
    }

    internal class OptionsService : PropertyStateBase, IOptionsService {
        private Options _currentOptions;
        private readonly IServiceContainer _serviceContainer;
        private readonly IEventAggregator _eventAggregator;

        public OptionsService (IServiceContainer serviceContainer, IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;
            _serviceContainer = serviceContainer;
        }

        public Options Get () {
            if (_currentOptions == null) {
                _currentOptions = Load ();
                _serviceContainer.RegisterInstance (_currentOptions);
            }

            return _currentOptions;
        }

        public void Update (Options options) {
            var path = Path.Combine (EnsureFolder (), "config.json");
            var json = JsonConvert.SerializeObject (options, Formatting.Indented, new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver ()
            });
            using (var writer = new StreamWriter (path, false)) {
                writer.WriteLine (json);
            }

            _currentOptions = options;            
            _eventAggregator.GetEvent<OptionsUpdate> ().Publish (_currentOptions);
        }

        private Options Load () {
            var path = Path.Combine (EnsureFolder (), "config.json");
            if (!File.Exists (path)) {
                var config = BuildDefault ();
                Update (config);
                return config;
            }

            var serializer = new JsonSerializer ();
            return serializer.Deserialize<Options> (new JsonTextReader (new StreamReader (path)));
        }

        private Options BuildDefault () {
            var conf = new Options ();
            var engine = SpeechRecognitionEngine.InstalledRecognizers ().First ();
            conf.SelectedEngine = engine.Description;

            var synth = new SpeechSynthesizer ();
            var voice = synth.GetInstalledVoices ().First ();
            conf.DefaultTtsVoice = voice.VoiceInfo.Name;
            synth.Dispose ();

            conf.ScriptEditor = new ScriptEditorOptions {
                ShowLineNumbers = true,
                HighlightCurrentLine = true,
                ShowEndOfLineMarkers = false,
                IndentionSize = 4,
                ConvertTabsToSpaces = true,
                ShowWhiteSpace = false,
                ShowColumnRuler = true,
                ColumnRulerPosition = 100,
                EnableHyperLinks = true,
                EnableVirtualSpace = false
            };

            return conf;
        }

        private static string EnsureFolder () {
            var folder = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "Vacuum");
            if (!Directory.Exists (folder)) {
                Directory.CreateDirectory (folder);
            }

            return folder;
        }
    }
}
