using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syrup.Self.Parts.FileDownloader;
using Syrup.Self.Parts.Globals;
using Syrup.Self.Parts.Helpers;
using Syrup.Self.Parts.Log;
using Syrup.Self.Parts.SelfUpdater;

namespace Syrup.Self
{
    public partial class Form1 : Form, INotifier
    {
        private static readonly Logger _log = Logger.Instance;
        private readonly SelfUpdaterService _selfUpdaterService;

        public Form1()
        {
            InitializeComponent();
            var globals = Global.Instance;
            var syrupDowloader = new SyrupDowloader(this);
            _selfUpdaterService = new SelfUpdaterService(globals, syrupDowloader, this);
        }


        public void SetProgress(int max)
        {
            if (InvokeRequired)
                Invoke(new Action(() =>
                {
                    progressBar.Maximum = max;
                    progressBar.Value = 0;
                }));
            else
            {
                progressBar.Maximum = max;
                progressBar.Value = 0;
            }
        }

        public void UpdateProgress(int value)
        {
            if (InvokeRequired)
                Invoke(new Action(() => { progressBar.Value = value; }));
            else
                progressBar.Value = value;
            ;
        }

        public void FinishProgress()
        {
            if (InvokeRequired)
                Invoke(new Action(() => { progressBar.Value = progressBar.Maximum; }));
            else
                progressBar.Value = progressBar.Maximum;
        }

        public void AddToLog(string text)
        {
            var t = text + Environment.NewLine;
            if (InvokeRequired)
                Invoke(new Action(() => { textBoxLog.AppendText(t); }));
            else
                textBoxLog.AppendText(t);
        }

        public void CloseMe()
        {
            Close();
        }


        private async Task Make()
        {
            textBoxLog.AppendText("Start");
            await _selfUpdaterService.MakeUpdate();
        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            _log.Debug("Begin process");
            await Make();
        }
    }
}