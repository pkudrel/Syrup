using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MediatR;
using NLog;
using Syrup.Common;
using Syrup.Common.Extension;
using Syrup.Core.Global.Events;
using Syrup.Core.Local.Commands;
using Syrup.Core.Local.Dto;
using Syrup.Core.Local.ReqRes;
using Syrup.Core.Server.Events;
using Syrup.Core.Server.Models;
using Syrup.Core.Server.ReqRes;
using Syrup.Core._Infrastructure.Global;

namespace Syrup.Features.MainView.View
{
    public partial class MainForm : Form, IFormHandlersContract
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static readonly Logger _logUi = LogManager.GetLogger(Consts.LOG_UI);
        private readonly IMediator _mediator;
        private readonly Registry _registry;


        public MainForm(IMediator mediator, Registry registry)
        {
            _mediator = mediator;
            _registry = registry;
            InitializeComponent();
            FastListViewConfig.Make(fastListView);
        }

        public void Execute(LongProcessStartedEvent notification)
        {
            this.Invoke(x =>
            {
                _logUi.Info(notification.Message);
                toolStripLabel.Text = notification.Message;
                toolStripProgressBar.Style = ProgressBarStyle.Marquee;
                toolStripProgressBar.MarqueeAnimationSpeed = 30;
            });
        }

        public void Execute(LongProcessNotifiedEvent notification)
        {
            this.Invoke(x =>
            {
                _logUi.Info(notification.Message);
                toolStripLabel.Text = notification.Message;
                if (notification.Progress > 0 && notification.Progress < 100)
                {
                    toolStripProgressBar.MarqueeAnimationSpeed = notification.Progress;
                }
              
            });
        }

        public void Execute(LongProcessEndedEvent notification)
        {
            this.Invoke(x =>
            {
                _logUi.Info(notification.Message);
                toolStripLabel.Text = notification.Message;
                toolStripProgressBar.Style = ProgressBarStyle.Continuous;
                toolStripProgressBar.MarqueeAnimationSpeed = 0;
            });
        }

        public void Execute(ReleaseInfoWasFetchedEvent releaseInfoWasFetchedEvent)
        {
            this.Invoke(x =>
            {
                fastListView.SetObjects(releaseInfoWasFetchedEvent.RelaseInfos);
                fastListView.Refresh();
                fastListView.Focus();
            });
        }

        public void Execute(MakeUpdatePanelVisibleRequest makeUpdatePanelVisibleRequest)
        {
            // ReSharper disable once LocalizableElement
            buttonMakeSyrupUpdate.Text = $"Update to: {makeUpdatePanelVisibleRequest.Version}";
            buttonMakeSyrupUpdate.Visible = true;
        }

        public void Execute(GetLocalReleaseListQueryResult result)
        {
            this.Invoke(x =>
            {
                var active = result.LocalReleaseInfoDto.FirstOrDefault(z => z.IsActive);
                if (active != null)
                {
                    labelActiveApp.Text = active.Name;
                }
                fastListView.SetObjects(result.LocalReleaseInfoDto);
                fastListView.Refresh();
                fastListView.Focus();
            });
        }


        private async void MainForm_Load(object sender, EventArgs e)
        {
            Text = _registry.Version.FullName;
            _logUi.Info($"App version: {_registry.Version.FullName}");

            labelActiveApp.Text = string.Empty;
            labelSelectedApp.Text = string.Empty;
          
            await _mediator.Publish(new ViewIsReadyAsyncEvent());
        }


        private void SetObjects(List<LocalReleaseInfoDto>  list)
        {
            if (list.Count == 0 ) return;
            this.Invoke(x =>
            {
                fastListView.SetObjects(list);
                fastListView.Refresh();
                fastListView.Focus();
            });
        }
        private void fastListView_SelectionChanged(object sender, EventArgs e)
        {
            var info = (LocalReleaseInfoDto) fastListView.SelectedObject;
            if (info == null) return;
            labelSelectedApp.Text = info.Name;
            buttonMakeSelectedActive.Enabled = true;
        }

        private async void buttonMakeSyrupUpdate_Click(object sender, EventArgs e)
        {
            var aa = new GetLocalReleaseListQuery();

            var r2 = await _mediator.Send(new MakeUpdateRequest());
            Close();
        }

        private async void buttonMakeSelectedActive_Click(object sender, EventArgs e)
        {
            var info = (LocalReleaseInfoDto)fastListView.SelectedObject;
            if (info == null) return;
            await _mediator.Publish(new MakeActiveSelectedReleaseCommand(info));

        }
    }
}