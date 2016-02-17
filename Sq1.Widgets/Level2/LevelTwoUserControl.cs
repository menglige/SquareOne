﻿using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;

using Sq1.Core.Support;
using Sq1.Core.Streaming;
using Sq1.Core.DataTypes;

namespace Sq1.Widgets.Level2 {
	public partial class LevelTwoUserControl : UserControlResizeable {
				StreamingAdapter		streamingAdapter;
				SymbolInfo				symbolInfo;
				string					windowTitle;
		public	DockContentImproved		DdeMonitorForm		{ get; private set; }

		Stopwatch			stopwatchRarifyingUIupdates;

		public LevelTwoUserControl() {
			InitializeComponent();
			this.olvDomCustomize();
			this.stopwatchRarifyingUIupdates = new Stopwatch();
			this.stopwatchRarifyingUIupdates.Start();
		}
		void layoutUserControlResizeable() {
			base.UserControlInner.Controls.Add(this.olvcLevelTwo);
			this.olvcLevelTwo.Dock = DockStyle.Fill;
		}

		public void Initialize(StreamingAdapter quikStreamingPassed, SymbolInfo symbolInfoPassed, string windowTitlePassed, DockContentImproved ddeMonitorFormPassed) {
			this.streamingAdapter	= quikStreamingPassed;
			this.symbolInfo			= symbolInfoPassed;
			this.windowTitle		= windowTitlePassed;
			this.DdeMonitorForm		= ddeMonitorFormPassed;

			if (this.symbolInfo.Level2ShowCumulativesInsteadOfLots) {
				this.olvAskCumulative	.IsVisible = true;
				this.olvAsk				.IsVisible = false;
				this.olvBid				.IsVisible = false;
				this.olvBidCumulative	.IsVisible = true;
			} else {
				this.olvAskCumulative	.IsVisible = false;
				this.olvAsk				.IsVisible = true;
				this.olvBid				.IsVisible = true;
				this.olvBidCumulative	.IsVisible = false;
			}
			this.olvcLevelTwo.RebuildColumns();

			this.layoutUserControlResizeable();
			this.PopulateLevel2ToTitle(this.windowTitle, true);
		}
		public void PopulateLevel2ToTitle(string windowTitlePassed, bool ignoreTimer = false) {
			if (this.olvcLevelTwo.IsDisposed) return;
			if (base.IsDisposed) return;

			this.windowTitle = windowTitlePassed;
			if (ignoreTimer == false) {
				// WHAT_IF_BEFORE_SWITCHING_TO_GUI_THREAD?
				if (this.stopwatchRarifyingUIupdates.ElapsedMilliseconds < this.streamingAdapter.Level2RefreshRateMs) return;
			}

			if (base.InvokeRequired) {
				base.BeginInvoke((MethodInvoker)delegate { this.PopulateLevel2ToTitle(windowTitlePassed, ignoreTimer); });
				return;
			}
			// I paid the price of switching to GuiThread, but I don' have to worry if I already stopwatch.Restart()ed
			//if (ignoreTimer == false) {
			//	if (this.stopwatchRarifyingUIupdates.ElapsedMilliseconds < this.quikStreaming.DdeMonitorRefreshRate) return;
			//RESTART_ONLY_IN_ONE_PLACE_OTHERWIZE_THE_OTHER_NEVER_DOES_ANYTHING this.stopwatchRarifyingUIupdates.Restart();
			//}

			this.lblDomTitle.Text = this.windowTitle;
		}

		public void PopulateLevel2ToDomControl(LevelTwoOlv levelTwoOLV_gotFromDde_pushTo_domResizeableUserControl) {
			if (this.olvcLevelTwo.IsDisposed) return;
			if (base.IsDisposed) return;

			// WHAT_IF_BEFORE_SWITCHING_TO_GUI_THREAD?
			if (this.stopwatchRarifyingUIupdates.ElapsedMilliseconds < this.streamingAdapter.Level2RefreshRateMs) return;
			//this.stopwatchRarifyingUIupdates.Restart();

			if (base.InvokeRequired) {
				base.BeginInvoke((MethodInvoker)delegate { this.PopulateLevel2ToDomControl(levelTwoOLV_gotFromDde_pushTo_domResizeableUserControl); });
				return;
			}
			// I paid the price of switching to GuiThread, but I don' have to worry if I already stopwatch.Restart()ed
			//if (this.stopwatchRarifyingUIupdates.ElapsedMilliseconds < this.quikStreaming.DdeMonitorRefreshRate) return;
			this.stopwatchRarifyingUIupdates.Restart();

			List<LevelTwoOlvEachLine> level2rows = levelTwoOLV_gotFromDde_pushTo_domResizeableUserControl.FrozenSortedFlattened_priceLevelsInserted;
			this.olvcLevelTwo.SetObjects(level2rows, true);
		}
	}
}
