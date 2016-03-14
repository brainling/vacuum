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
using System.Windows;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using LightInject;
using Microsoft.Practices.Prism.PubSubEvents;
using Vacuum.Core;
using Vacuum.Core.Commands;
using Vacuum.Core.Configuration;
using Vacuum.Views;

namespace Vacuum.ViewModels {
    public interface ICommandSetEditorView {
        void SetEditorOptions (ScriptEditorOptions options);
        void Edit ();
    }

    public interface ICommandSetEditorViewModel {
        Options Options { get; }
        ICommandSetEditorView View { get; set; }
        IHighlightingDefinition LuaHighlighting { get; }
        ICommand AddCommand { get; }
        ICommand RemoveCommand { get; }
        CommandSet Set { get; }
        Command SelectedCommand { get; set; }
        string Title { get; }
    }

    public class CommandSetEditorViewModel : PropertyStateBase, ICommandSetEditorViewModel, IDisposable {
        private Options _options;
        private Command _selectedCommand;
        private CommandSet _set;
        private ICommandSetEditorView _view;
        private readonly IEventAggregator _eventAggregator;
        private readonly IServiceContainer _container;

        public CommandSetEditorViewModel (CommandSet set, IOptionsService optionsService, IEventAggregator eventAggregator, IServiceContainer container) {
            LuaHighlighting = LoadHighlighting ();

            Set = set;
            SelectedCommand = set.Commands.FirstOrDefault ();
            Options = optionsService.Get ();
            Title = $"Editing Command Set - {set.Name}";

            _eventAggregator = eventAggregator;
            eventAggregator.GetEvent<OptionsUpdate> ().Subscribe (OptionsUpdated);

            _container = container;

            WhenStateUpdated (() => View, SetTextEditorOptions);
        }

        public Command SelectedCommand {
            get { return _selectedCommand; }
            set { SetProperty (ref _selectedCommand, value); }
        }

        public CommandSet Set {
            get { return _set; }
            set { SetProperty (ref _set, value); }
        }

        public ICommand AddCommand => GetCommand ("Add", ExecuteAddCommand);
        public ICommand RemoveCommand => GetCommand ("Remove", ExecuteRemoveCommand, CanExecuteRemoveCommand);

        public Options Options {
            get { return _options; }
            set { SetProperty (ref _options, value); }
        }

        public string Title { get; }

        public ICommandSetEditorView View {
            get { return _view; }
            set { SetProperty (ref _view, value); }
        }

        public IHighlightingDefinition LuaHighlighting { get; }

        public void Dispose () {
            _eventAggregator.GetEvent<OptionsUpdate> ().Unsubscribe (OptionsUpdated);
        }

        private void ExecuteAddCommand () {
            var editor = _container.GetInstance<Command, IEditCommandView> (null);
            editor.SetOwner (View);
            editor.ShowDialog ();
        }

        private void ExecuteRemoveCommand () {
        }

        private bool CanExecuteRemoveCommand () {
            return SelectedCommand != null;
        }

        private void OptionsUpdated (Options options) {
            Options = options;
            SetTextEditorOptions ();
        }

        private void SetTextEditorOptions () {
            View?.SetEditorOptions (Options.ScriptEditor);
        }

        private IHighlightingDefinition LoadHighlighting () {
            using (var stream = typeof (CommandSetEditorView).Assembly.GetManifestResourceStream ("Vacuum.Resources.Lua.xshd")) {
                using (var reader = new XmlTextReader (stream)) {
                    return HighlightingLoader.Load (reader, HighlightingManager.Instance);
                }
            }
        }
    }
}
