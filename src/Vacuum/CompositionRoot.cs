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

using LightInject;
using Microsoft.Practices.Prism.PubSubEvents;
using Vacuum.Core.Commands;
using Vacuum.Core.Configuration;
using Vacuum.Core.Profiles;
using Vacuum.ViewModels;
using Vacuum.Views;

namespace Vacuum {
    public class CompositionRoot : ICompositionRoot {
        public void Compose (IServiceRegistry serviceRegistry) {
            #region Singleton Views

            serviceRegistry.Register<IMainWindowViewModel, MainWindowViewModel> (new PerContainerLifetime ());
            serviceRegistry.Register<IMainWindowView, MainWindowView> (new PerContainerLifetime ());

            serviceRegistry.Register<IOptionsViewModel, OptionsViewModel> (new PerContainerLifetime ());
            serviceRegistry.Register<IOptionsView, OptionsView> (new PerContainerLifetime ());

            serviceRegistry.Register<IProfilesViewModel, ProfilesViewModel> (new PerContainerLifetime ());
            serviceRegistry.Register<ProfilesView> (new PerContainerLifetime ());

            serviceRegistry.Register<ICommandSetPanelViewModel, CommandSetPanelViewModel> (new PerContainerLifetime ());
            serviceRegistry.Register<CommandSetPanelView> (new PerContainerLifetime ());

            serviceRegistry.Register<IStatusLogViewModel, StatusLogViewModel> (new PerContainerLifetime ());
            serviceRegistry.Register<StatusLogView> (new PerContainerLifetime ());

            #endregion

            #region Transient Views

            serviceRegistry.Register<CommandSet, ICommandSetEditorViewModel> ((f, c) =>
                new CommandSetEditorViewModel (c, f.GetInstance<IOptionsService> (), f.GetInstance<IEventAggregator> (), f.GetInstance<IServiceContainer> ()));
            serviceRegistry.Register<CommandSet, CommandSetEditorView> ((f, c) =>
                new CommandSetEditorView (f.GetInstance<CommandSet, ICommandSetEditorViewModel> (c)));

            serviceRegistry.Register<Command, IEditCommandViewModel> ((f, c) => new EditCommandViewModel (c));
            serviceRegistry.Register<Command, EditCommandView> ((f, c) => new EditCommandView (f.GetInstance<Command, IEditCommandViewModel> (c)));

            serviceRegistry.Register<Profile, IEditProfileViewModel> ((f, p) => new EditProfileViewModel (p, f.GetInstance<IProfileService> ()));
            serviceRegistry.Register<Profile, EditProfileView> ((f, p) => new EditProfileView (f.GetInstance<Profile, IEditProfileViewModel> (p)));

            serviceRegistry.Register<string, string, IQuickInputViewModel> ((f, t, p) => new QuickInputViewModel (t, p));
            serviceRegistry.Register<string, string, QuickInputView> ((f, t, p) =>
                new QuickInputView (f.GetInstance<string, string, IQuickInputViewModel> (t, p)));

            #endregion
        }
    }
}
