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

using LightInject;
using Microsoft.Practices.Prism.PubSubEvents;
using Moq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Vacuum.Core;
using Vacuum.Core.Commands;
using Vacuum.Core.Configuration;
using Vacuum.ViewModels;
using Vacuum.Views;

namespace Vacuum.Spec {
    [Binding]
    public class CommandSetEditorSteps {
        private static Mock<IEditCommandView> _commandEditor;
        private static CommandSet _commandSet;
        private static CommandSetEditorViewModel _editor;
        private static Mock<IEventAggregator> _eventAggregator;
        private static Mock<IOptionsService> _optionsService;
        private static Mock<IServiceContainer> _serviceContainer;

        [BeforeFeature ("commandSetEditor")]
        public static void BeforeFeature () {
            _optionsService = new Mock<IOptionsService> ();

            _eventAggregator = new Mock<IEventAggregator> ();
            _eventAggregator.Setup (aggregator => aggregator.GetEvent<OptionsUpdate> ())
                            .Returns (new OptionsUpdate ());

            _commandEditor = new Mock<IEditCommandView> ();
            _commandEditor.Setup (editor => editor.ShowDialog ())
                          .Returns (true);

            _serviceContainer = new Mock<IServiceContainer> ();
            _serviceContainer.Setup (container => container.GetInstance<Command, IEditCommandView> (null))
                             .Returns (_commandEditor.Object);
        }

        [Given (@"user is editing a command set")]
        public void GivenUserIsEditingACommandSet () {
            _commandSet = new CommandSet {
                Name = "Test Set"
            };

            _editor = new CommandSetEditorViewModel (_commandSet, _optionsService.Object, _eventAggregator.Object, _serviceContainer.Object);
        }

        [When (@"user adds command '(.*)'")]
        public void WhenUserAddsCommand (string p0) {
            _editor.AddCommand.Execute (null);
        }

        [Then (@"command editor is shown")]
        public void ThenCommandEditorIsShown () {
            _commandEditor.Verify (editor => editor.ShowDialog (), Times.AtMostOnce);
        }

        [Given (@"user has no selected command")]
        public void GivenUserHasNoSelectedCommand () {
            _editor.SelectedCommand = null;
        }

        [Then (@"user cannot execute remove")]
        public void ThenUserCannotExecuteRemove () {
            Assert.IsFalse (_editor.RemoveCommand.CanExecute (null));
        }

        [Given (@"user has selected a command")]
        public void GivenUserHasSelectedACommand () {
            _editor.SelectedCommand = new Command ();
        }

        [Then (@"user can execute remove")]
        public void ThenUserCanExecuteRemove () {
            Assert.IsTrue (_editor.RemoveCommand.CanExecute (null));
        }
    }
}
