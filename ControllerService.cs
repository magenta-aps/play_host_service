using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Reflection;

namespace PlayHostService
{
    public partial class ControllerService : ServiceBase
    {
        public ControllerService()
        {
            InitializeComponent();
        }

        private int ProcessId;

        protected override void OnStart(string[] args)
        {
            var exeDirectory = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(exeDirectory, Properties.Settings.Default.ProcessToStart),
                Arguments = Properties.Settings.Default.StartArgumentsArguments,
                WorkingDirectory = Path.Combine(exeDirectory, Properties.Settings.Default.StartIn),

                UseShellExecute = false,
                CreateNoWindow = true,

                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            };

            Process process = new Process() { StartInfo = startInfo };
            process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            ProcessId = process.Id;
        }

        Dictionary<string, string> FilePaths;

        void WriteText(string prefix, string text)
        {
            if (Properties.Settings.Default.LoggingEnabled)
            {
                if (FilePaths == null)
                {
                    string _Root = new FileInfo(typeof(ControllerService).Assembly.Location).Directory.FullName + "\\";
                    FilePaths = new Dictionary<string, string>();
                    var id = Guid.NewGuid().ToString();
                    FilePaths["out"] = _Root + "Out-" + id + ".log";
                    FilePaths["err"] = _Root + "Out-" + id + ".log";
                }
                File.AppendAllText(FilePaths[prefix], text + Environment.NewLine);
            }
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteText("err", e.Data);
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteText("out", e.Data);
        }

        protected override void OnStop()
        {
            var playProcess = Process.GetProcessesByName(Properties.Settings.Default.KillProcessName).Where(p => p.Parent().Id == ProcessId).SingleOrDefault();
            if (playProcess != null)
                playProcess.Kill();
        }
    }


}
