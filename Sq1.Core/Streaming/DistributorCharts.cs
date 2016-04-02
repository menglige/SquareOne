﻿using System;

using Sq1.Core.DataTypes;

namespace Sq1.Core.Streaming {
	public class DistributorCharts : Distributor {
		public const string REASON_TO_EXIST = "I_ALLOW_MULTIPLE_CONSUMERS_FOR_SAME_SYMBOL:SCALEINTERVAL_PAIR__SUITABLE_FOR_MULTIPLE_CHARTS";
		public DistributorCharts(StreamingAdapter streamingAdapter, string reasonToExist)
						  : base(streamingAdapter, Distributor.LIVE_CHARTS_FOR + " " + reasonToExist) {}

		public override bool ConsumerBarSubscribe(StreamingConsumer chartConsumer, bool quotePumpSeparatePushingThreadEnabled) {
			bool ret = false;
			string msig = " //ChartConsumerBarSubscribe(" + chartConsumer + ")";

			if (chartConsumer.ScaleInterval.Scale == BarScale.Unknown) {
				string msg = "Failed to BarRegister(): scaleInterval.Scale=Unknown; returning";
				Assembler.PopupException(msg);
				//throw new Exception(msg);
				return ret;
			}
			bool alreadyRegistered = base.ConsumerBarIsSubscribed(chartConsumer);
			if (alreadyRegistered) {
				string msg = "BAR_CONSUMER_ALREADY_REGISTERED";
				Assembler.PopupException(msg, null, false);
				return ret;
			}

			ret = base.ConsumerBarSubscribe(chartConsumer, quotePumpSeparatePushingThreadEnabled);
			string msg2 = "SUBSCRIBED_CHART_BARS[" + ret + "]";

			if (Assembler.InstanceInitialized.MainForm_dockFormsFullyDeserialized_layoutComplete) {
				Assembler.PopupException(msg2 + msig, null, false);
			}

			return ret;
		}
		public override bool ConsumerBarUnsubscribe(StreamingConsumer chartConsumer) {
			string msig = " //ConsumerBarUnsubscribe [" + chartConsumer + "]";
			bool ret = base.ConsumerBarUnsubscribe(chartConsumer);
			string msg2 = "UNSUBSCRIBED_CHART_BARS[" + ret + "]";

			if (Assembler.InstanceInitialized.MainForm_dockFormsFullyDeserialized_layoutComplete) {
				Assembler.PopupException(msg2 + msig, null, false);
			}
			return ret;
		}
		public override bool ConsumerQuoteSubscribe(StreamingConsumer chartConsumer, bool quotePumpSeparatePushingThreadEnabled) {
			
			bool ret = false;
			string msig = " //ChartConsumerQuoteSubscribe(" + chartConsumer + ")";

			bool alreadyRegistered = base.ConsumerQuoteIsSubscribed(chartConsumer);
			if (alreadyRegistered) {
				string msg = "QUOTE_CONSUMER_ALREADY_REGISTERED";
				Assembler.PopupException(msg + msig, null, false);
				return ret;
			}

			ret = base.ConsumerQuoteSubscribe(chartConsumer, quotePumpSeparatePushingThreadEnabled);
			string msg2 = "SUBSCRIBED_CHART_QUOTES upstreamUnsubscribed[" + ret + "]";

			if (Assembler.InstanceInitialized.MainForm_dockFormsFullyDeserialized_layoutComplete) {
				Assembler.PopupException(msg2 + msig, null, false);
			}

			base.StreamingAdapter.StreamingDataSnapshot.Initialize_levelTwo_lastPrevQuotes_forSymbol(chartConsumer.Symbol);
			return ret;
		}
		public override bool ConsumerQuoteUnsubscribe(StreamingConsumer chartConsumer) {
						string msig = " //ConsumerQuoteUnsubscribe(" + chartConsumer + ")";
			bool ret = base.ConsumerQuoteUnsubscribe(chartConsumer);
			string msg2 = "UNSUBSCRIBED_CHART_QUOTES upstreamUnsubscribed[" + ret + "]";

			if (Assembler.InstanceInitialized.MainForm_dockFormsFullyDeserialized_layoutComplete) {
				Assembler.PopupException(msg2 + msig, null, false);
			}

			base.StreamingAdapter.StreamingDataSnapshot.Initialize_levelTwo_lastPrevQuotes_forSymbol(chartConsumer.Symbol);

			return ret;
		}
	}
}
