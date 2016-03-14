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

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Vacuum.Core.Behaviors {
    public class LastColumnFill : Behavior<ListView> {
        private bool _attached;
        private GridView _gridView;

        protected override void OnAttached () {
            base.OnAttached ();

            _gridView = AssociatedObject.View as GridView;
            if (_gridView == null) {
                return;
            }

            _attached = true;
            AssociatedObject.SizeChanged += ListViewSizeChanged;
        }

        protected override void OnDetaching () {
            base.OnDetaching ();

            if (!_attached) {
                return;
            }

            _attached = false;
            AssociatedObject.SizeChanged -= ListViewSizeChanged;
        }

        private void ListViewSizeChanged (object stender, SizeChangedEventArgs e) {
            if (_gridView.Columns.Count == 0) {
                return;
            }

            var width = e.NewSize.Width
                        - (AssociatedObject.Margin.Left + AssociatedObject.Margin.Right)
                        - (AssociatedObject.BorderThickness.Left + AssociatedObject.BorderThickness.Right)
                        - (AssociatedObject.Padding.Left + AssociatedObject.Padding.Right);
            if (_gridView.Columns.Count == 1) {
                _gridView.Columns[0].Width = width;
                return;
            }

            for (var i = 0; i < _gridView.Columns.Count - 1; i++) {
                width -= _gridView.Columns[i].ActualWidth;
            }

            if (width <= 0) {
                return;
            }

            _gridView.Columns.Last ().Width = width - 10;
        }
    }
}
