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
using System.Windows;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;

namespace Vacuum.Core.Behaviors {
    public class EditorSourceBinding : Behavior<TextEditor> {
        public string Source {
            get { return (string) GetValue (SourceProperty); }
            set { SetValue (SourceProperty, value); }
        }

        protected override void OnAttached () {
            base.OnAttached ();
            if (AssociatedObject != null) {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
            }
        }

        protected override void OnDetaching () {
            base.OnDetaching ();
            if (AssociatedObject != null) {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            }
        }

        private void AssociatedObjectOnTextChanged (object sender, EventArgs eventArgs) {
            var textEditor = sender as TextEditor;
            if (textEditor != null) {
                if (textEditor.Document != null) {
                    Source = textEditor.Document.Text;
                }
            }
        }

        private static void PropertyChangedCallback (
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            
            var behavior = dependencyObject as EditorSourceBinding;
            if (behavior == null || behavior.AssociatedObject == null) {
                return;
            }

            var editor = behavior.AssociatedObject;
            if (editor.Document == null) {
                return;
            }

            var caretOffset = editor.CaretOffset;
            editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString ();
            editor.CaretOffset = caretOffset;
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register ("Source", typeof (string), typeof (EditorSourceBinding),
                new FrameworkPropertyMetadata (default (string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));
    }
}
