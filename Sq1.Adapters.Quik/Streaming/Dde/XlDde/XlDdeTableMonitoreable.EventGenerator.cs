﻿using System;
using System.Collections.Generic;

using Sq1.Core;

namespace Sq1.Adapters.Quik.Streaming.Dde.XlDde {
	public abstract partial class XlDdeTableMonitoreable<T> {
		public event EventHandler<XlDdeTableMonitoringEventArg<T>>			DataStructureParsed_One;
		public event EventHandler<XlDdeTableMonitoringEventArg<List<T>>>	DataStructuresParsed_Table;

		void raiseDataStructureParsed_One(T oneParsed) {
			if (this.DataStructureParsed_One == null) return;
			try {
				this.DataStructureParsed_One(this, new XlDdeTableMonitoringEventArg<T>(oneParsed));
			} catch (Exception ex) {
				string msg = "QuikStreamingMonitorControl_TREW_IN_raiseDataStructureParsed_One(oneParsed[" + oneParsed + "])";
				Assembler.PopupException(msg, ex);
			}
		}

		void raiseDataStructuresParsed_Table(List<T> tableParsed) {
			if (this.DataStructuresParsed_Table == null) return;
			try {
				this.DataStructuresParsed_Table(this, new XlDdeTableMonitoringEventArg<List<T>>(tableParsed));
			} catch (Exception ex) {
				string msg = "QuikStreamingMonitorControl_TREW_IN_raiseDataStructuresParsed_Table(tableParsed[" + tableParsed + "])";
				Assembler.PopupException(msg, ex);
			}
		}

	}
}
