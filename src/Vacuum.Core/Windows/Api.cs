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
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Vacuum.Core.Windows {
    public struct HARDWAREINPUT {
        public int uMsg;
        public short wParamH;
        public short wParamL;
    }

    public struct KEYBDINPUT {
        public IntPtr dwExtraInfo;
        public KeyboardEventFlags dwFlags;
        public int time;
        public short wScan;
        public short wVk;
    }

    public struct MOUSEINPUT {
        public IntPtr dwExtraInfo;
        public MouseEventFlags dwFlags;
        public int dx;
        public int dy;
        public int mouseData;
        public int time;
    }

    [StructLayout (LayoutKind.Explicit)]
    public struct INPUT {
        [FieldOffset (0)] public InputType type;
        [FieldOffset (8)] public MOUSEINPUT mi;
        [FieldOffset (8)] public KEYBDINPUT ki;
        [FieldOffset (8)] public HARDWAREINPUT hi;
    }

    [Flags]
    public enum InputType {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }

    [Flags]
    public enum KeyboardEventFlags {
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        ScanKey = 0x0008
    }

    [Flags]
    public enum MouseEventFlags {
        Absolute = 0x8000,
        HWheel = 0x01000,
        Move = 0x0001,
        MoveNoCoalesce = 0x2000,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        VirtualDesk = 0x4000,
        Wheel = 0x0800,
        XDown = 0x0080,
        XUp = 0x0100
    }

    public delegate bool EnumWindowsProc (IntPtr hWnd, IntPtr lParam);

    public class Api {
        [DllImport ("user32.dll")]
        public static extern uint SendInput (uint inputCount, /* [MarshalAs(UnmanagedType.LPArray)] */ INPUT[] inputs, int size);

        [DllImport ("user32.dll")]
        [return: MarshalAs (UnmanagedType.Bool)]
        public static extern bool EnumWindows (EnumWindowsProc lpEnumFunc, IntPtr lParam);
    }
}
