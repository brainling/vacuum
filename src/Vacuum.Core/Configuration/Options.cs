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

using Microsoft.Practices.Prism.Mvvm;

namespace Vacuum.Core.Configuration {
    public class ScriptEditorOptions : BindableBase {
        private int _columnRulerPosition;
        private bool _convertTabsToSpaces;
        private bool _enableHyperLinks;
        private bool _enableVirtualSpace;
        private bool _highlightCurrentLine;
        private int _indentionSize;
        private bool _showColumnRuler;
        private bool _showEndOfLineMarkers;
        private bool _showLineNumbers;
        private bool _showWhiteSpace;

        public bool ShowLineNumbers {
            get { return _showLineNumbers; }
            set { SetProperty (ref _showLineNumbers, value); }
        }

        public bool HighlightCurrentLine {
            get { return _highlightCurrentLine; }
            set { SetProperty (ref _highlightCurrentLine, value); }
        }

        public bool ShowEndOfLineMarkers {
            get { return _showEndOfLineMarkers; }
            set { SetProperty (ref _showEndOfLineMarkers, value); }
        }

        public int IndentionSize {
            get { return _indentionSize; }
            set { SetProperty (ref _indentionSize, value); }
        }

        public bool ConvertTabsToSpaces {
            get { return _convertTabsToSpaces; }
            set { SetProperty (ref _convertTabsToSpaces, value); }
        }

        public bool ShowWhiteSpace {
            get { return _showWhiteSpace; }
            set { SetProperty (ref _showWhiteSpace, value); }
        }

        public bool ShowColumnRuler {
            get { return _showColumnRuler; }
            set { SetProperty (ref _showColumnRuler, value); }
        }

        public int ColumnRulerPosition {
            get { return _columnRulerPosition; }
            set { SetProperty (ref _columnRulerPosition, value); }
        }

        public bool EnableHyperLinks {
            get { return _enableHyperLinks; }
            set { SetProperty (ref _enableHyperLinks, value); }
        }

        public bool EnableVirtualSpace {
            get { return _enableVirtualSpace; }
            set { SetProperty (ref _enableVirtualSpace, value); }
        }

        public ScriptEditorOptions Clone () {
            return (ScriptEditorOptions) MemberwiseClone ();
        }
    }

    public class Options : BindableBase {
        private string _defaultTtsVoice;
        private string _selectedEngine;

        public Options () {
            ScriptEditor = new ScriptEditorOptions ();
        }

        public string SelectedEngine {
            get { return _selectedEngine; }
            set { SetProperty (ref _selectedEngine, value); }
        }

        public string DefaultTtsVoice {
            get { return _defaultTtsVoice; }
            set { SetProperty (ref _defaultTtsVoice, value); }
        }

        public ScriptEditorOptions ScriptEditor { get; set; }

        public Options Clone () {
            var options = (Options) MemberwiseClone ();
            options.ScriptEditor = ScriptEditor.Clone ();
            return options;
        }
    }
}
