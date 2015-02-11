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
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Vacuum.Core {
    public class Log {
        private const string LogLayout = "[${date:HH\\:MM\\:ss}] ${level:uppercase=true}: ${message}";

        public Log () {
            Initialize ();
        }

        private void Initialize () {
            ArchiveLog ();

            var config = new LoggingConfiguration ();

            var consoleTarget = new ColoredConsoleTarget ();
            config.AddTarget ("console", consoleTarget);

            var fileTarget = new FileTarget ();
            config.AddTarget ("file", fileTarget);

            consoleTarget.Layout = LogLayout;
            fileTarget.FileName = @"${basedir}\${logger}.log";
            fileTarget.DeleteOldFileOnStartup = true;
            fileTarget.Layout = LogLayout;

            var rule = new LoggingRule ("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add (rule);

            rule = new LoggingRule ("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add (rule);

            LogManager.Configuration = config;
        }

        private static string LogPath (string name) {
            return Path.Combine (Environment.CurrentDirectory, String.Format ("{0}.log", name));
        }

        private static string LogPath (string name, int sequence) {
            return Path.Combine (Environment.CurrentDirectory, String.Format ("{0}.{1}.log", name, sequence));
        }

        private static void StepLogs (string name, int count) {
            for (var i = count - 1; i > 0; i--) {
                var path = LogPath (name, i);
                if (File.Exists (path)) {
                    File.Copy (path, LogPath (name, i + 1), true);
                }

                if (i != 1) {
                    continue;
                }

                var baseLog = LogPath (name);
                if (File.Exists (baseLog)) {
                    File.Copy (baseLog, LogPath (name, 1), true);
                }
            }
        }

        private static void ArchiveLog () {
#if DEBUG
            const int archiveCount = 5;
#else
            const int archiveCount = 1;
#endif
            StepLogs ("vacuum", archiveCount);
        }
    }
}
