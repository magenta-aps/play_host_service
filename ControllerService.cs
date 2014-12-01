using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace CPreaderController
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
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Properties.Settings.Default.ProcessToStart,
                Arguments = Properties.Settings.Default.StartArgumentsArguments,
                WorkingDirectory = Properties.Settings.Default.StartIn,
            };

            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process process = new Process() { StartInfo = startInfo };
            process.Start();
            ProcessId = process.Id;
        }

        protected override void OnStop()
        {
            var playProcess = Process.GetProcessesByName(Properties.Settings.Default.KillProcessName).Where(p => p.Parent().Id == ProcessId).SingleOrDefault();
            if (playProcess != null)
                playProcess.Kill();
        }
    }


}
