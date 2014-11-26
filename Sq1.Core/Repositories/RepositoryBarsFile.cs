using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Sq1.Core.DataTypes;

namespace Sq1.Core.Repositories {
	public class RepositoryBarsFile {
		RepositoryBarsSameScaleInterval barsRepository; // WAS_PUBLIC_EARLIER{ get; protected set; }
		public string Symbol { get; protected set; }
		public string Abspath { get; protected set; }
		public string Relpath { get { return RepositoryBarsFile.GetRelpathFromEnd(this.Abspath, 5); } }
		double barFileCurrentVersion = 3;	// yeps double :) 8 bytes!
		int symbolMaxLength = 64;			// IRRELEVANT_FOR_barFileCurrentVersion=3 32 UTF8 characters
		int symbolHRMaxLength = 128;		// IRRELEVANT_FOR_barFileCurrentVersion=3 64 UTF8 characters
		long headerSize;			// BARS_LOAD_TELEMETRY
		long oneBarSize;			// BARS_LOAD_TELEMETRY
		object fileReadWriteSequentialLock;

		Dictionary<double, long> headerSizesByVersion = new Dictionary<double, long>() { { 3, 20 } };	// got 212 in Debugger from this.headerSize while reading saved v3 file
		Dictionary<double, long> barSizesByVersion = new Dictionary<double, long>() { { 3, 48 } };	// got 212 in Debugger from this.oneBarSize while reading saved v3 file
		
		public RepositoryBarsFile(RepositoryBarsSameScaleInterval barsFolder, string symbol, bool throwIfDoesntExist = true, bool createIfDoesntExist = false) {
			fileReadWriteSequentialLock = new object();
			this.barsRepository = barsFolder;
			this.Symbol = symbol;
			this.Abspath = this.barsRepository.AbspathForSymbol(this.Symbol, throwIfDoesntExist, createIfDoesntExist);
		}

		public Bars BarsLoadThreadSafe(DateTime dateFrom, DateTime dateTill, int maxBars) {
			Bars barsAll = this.BarsLoadAllThreadSafe();
			//Assembler.PopupException("Loaded [ " + bars.Count + "] bars; symbol[" + this.Symbol + "] scaleInterval[" + this.BarsFolder.ScaleInterval + "]");
			if (dateFrom == DateTime.MinValue && dateTill == DateTime.MaxValue && maxBars == 0) return barsAll;

			string start = (dateFrom == DateTime.MinValue) ? "MIN" : dateFrom.ToString("dd-MMM-yyyy");
			string end = (dateTill == DateTime.MaxValue) ? "MAX" : dateTill.ToString("dd-MMM-yyyy");
			Bars bars = new Bars(barsAll.Symbol, barsAll.ScaleInterval, barsAll.ReasonToExist + " [" + start + "..." + end + "]max[" + maxBars + "]");
			for (int i = 0; i < barsAll.Count; i++) {
				if (maxBars > 0 && i >= maxBars) break;
				Bar barAdding = barsAll[i];
				bool skipThisBar = false;
				if (dateFrom > DateTime.MinValue && barAdding.DateTimeOpen < dateFrom) skipThisBar = true;
				if (dateTill < DateTime.MaxValue && barAdding.DateTimeOpen > dateTill) skipThisBar = true;
				if (skipThisBar) continue;
				bars.BarAppendBindStatic(barAdding.CloneDetached());
			}
			return bars;
		}
		public Bars BarsLoadAllThreadSafe(bool saveBarsIfThereWasFailedCheckOHLCV = true) {
			Bars bars = null;
			lock(this.fileReadWriteSequentialLock) {
				if (File.Exists(this.Abspath) == false) {
					string msg = "LoadBarsThreadSafe(): File doesn't exist [" + this.Abspath + "]";
					//Assembler.PopupException(msg);
					//throw new Exception(msg);
					return null;
//					return bars;
				}
				bars = this.barsLoadAll(saveBarsIfThereWasFailedCheckOHLCV);
			}
			return bars;
		}
		Bars barsLoadAll(bool resaveBarsIfThereWasFailedCheckOHLCV = true) {
			string msig = " BarsLoadAll(this.Abspath=[" + this.Abspath + "]): ";
			int barsReadTotal = 0;
			int barsFailedCheckOHLCV = 0;
			bool resaveRequiredByVersionMismatch = false;
			
			Bars bars = null;
			DateTime dateTime = DateTime.Now;
			FileStream fileStream = null;
			try {
				fileStream = File.Open(this.Abspath, FileMode.Open, FileAccess.Read, FileShare.Read);
				BinaryReader binaryReader = new BinaryReader(fileStream);

				string symbol_IGNOREDv3 = "NOT_READ_FROM_FILE";
				string symbolHumanReadable_IGNOREDv3;
				
				double version = binaryReader.ReadDouble();
				if (version != this.barFileCurrentVersion) {
					resaveRequiredByVersionMismatch = true;
					string msg = "WILL_RESAVE_IN_CURRENT_BAR_BINARY_FORMAT"
						+ " version[" + version + "] => this.barFileCurrentVersion[" + this.barFileCurrentVersion + "]"
						+ " resaveRequiredByVersionMismatch[" + resaveRequiredByVersionMismatch + "]";
					Assembler.PopupException(msg + msig, null, false);
				}
				
				if (version == 1) {
					//Assembler.PopupException("LoadBars[" + this.Relpath + "]: version[" + version + "]");
					symbol_IGNOREDv3 = binaryReader.ReadString();
					symbolHumanReadable_IGNOREDv3 = binaryReader.ReadString();
				} else if (version <= 2) {
					byte[] bytesSymbol = new byte[this.symbolMaxLength];
					binaryReader.Read(bytesSymbol, 0, this.symbolMaxLength);
					symbol_IGNOREDv3 = this.byteArrayToString(bytesSymbol);

					byte[] bytesSymbolHR = new byte[this.symbolHRMaxLength];
					binaryReader.Read(bytesSymbolHR, 0, this.symbolHRMaxLength);
					symbolHumanReadable_IGNOREDv3 = this.byteArrayToString(bytesSymbolHR);
				} else if (version <= 3) {
					// NO_SYMBOL_AND_HR_IS_PRESENT_IN_FILE
					int a = 1;
				}
				BarScale barScale = (BarScale)binaryReader.ReadInt32();
				int barInterval = binaryReader.ReadInt32();
				int barsStored = binaryReader.ReadInt32();
				this.headerSize = binaryReader.BaseStream.Position;		// BARS_LOAD_TELEMETRY

				BarScaleInterval scaleInterval = new BarScaleInterval(barScale, barInterval);
				//string shortFnameIneedMorePathParts = Path.GetFileName(this.Abspath);
				//string shortFname = this.Abspath.Substring(this.Abspath.IndexOf("" + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "") + 6);
				string shortFname = this.Relpath;
				//v1,2 AFTER_IMPLEMENTING_FIXED_SYMBOL_WIDTH_IGNORING_WHAT_I_READ_FROM_FILE  bars = new Bars(symbol, scaleInterval, shortFname);
				string v3ignoresSymbolFromFile = (this.barFileCurrentVersion <=2) ? symbol_IGNOREDv3 : this.Symbol;
				bars = new Bars(v3ignoresSymbolFromFile, scaleInterval, shortFname);
				//for (int barsRead = 0; barsRead<barsStored; barsRead++) {
				while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length) {
					DateTime dateTimeOpen = new DateTime(binaryReader.ReadInt64());
					double open = binaryReader.ReadDouble();
					double high = binaryReader.ReadDouble();
					double low = binaryReader.ReadDouble();
					double close = binaryReader.ReadDouble();
					double volume = binaryReader.ReadDouble();
					barsReadTotal++;
					if (this.oneBarSize == 0) {
						// I want to print out the size of header and bar, but I don't want to extract save-able members from Bars and Bar to use Marshal.SizeOf(<T>)
						this.oneBarSize = binaryReader.BaseStream.Position - this.headerSize;	// BARS_LOAD_TELEMETRY
					}
					try {
						Bar barAdded = bars.BarCreateAppendBindStatic(dateTimeOpen, open, high, low, close, volume, true);
					} catch (Exception ex) {
						barsFailedCheckOHLCV++;
						// already reported exception in CheckOHLCVthrow
						string msg2 = " barsFailedCheckOHLCV[" + barsFailedCheckOHLCV + "]  barsReadTotal[" + barsReadTotal + "] bars.Count[" + bars.Count + "]"
							+ " binaryReader.BaseStream.Position[" + binaryReader.BaseStream.Position + "]/[" + binaryReader.BaseStream.Length + "]";
						Assembler.PopupException(msg2 + msig, ex, false);
						continue;	//just in case if you add code below :)
					}
				}
				
				string msg3 = "BARS_LOAD_ALL_TELEMETRY SIZEOF(header)[" + this.headerSize + "] SIZEOF(Bar)[" + this.oneBarSize + "]"
					+ " version[" + version + "] bars[" + bars + "] Relpath[" + this.Relpath + "]";
				Assembler.PopupException(msg3 + msig, null, false);
				try {
					long barSize = this.barSizesByVersion[version];
					if (barSize != this.oneBarSize) {
						this.barSizesByVersion[version] = this.oneBarSize;
					}
				} catch (Exception ex) {
					string msg2 = "FAILED_TO_SYNC this.barSizesByVersion[" + version + "]";
					Assembler.PopupException(msg2 + msig, ex);
				}
	
				try {
					long headerSize = this.headerSizesByVersion[version];
					if (headerSize != this.headerSize) {
						this.headerSizesByVersion[version] = this.headerSize;
					}
				} catch (Exception ex) {
					string msg2 = "FAILED_TO_SYNC this.headerSizesByVersion[" + version + "]";
					Assembler.PopupException(msg2 + msig, ex);
				}
			} catch (Exception ex) {
				string msg = "BARS_LOAD_ALL_FAILED[" + this.Abspath + "]";
				Assembler.PopupException(msg + msig, ex);
			} finally {
				if (fileStream != null) {
					fileStream.Close();
					fileStream.Dispose();
				}
			}

			
			bool resaveRequired = resaveRequiredByVersionMismatch;
			if (barsFailedCheckOHLCV > 0) {
				string msg = "SOME_BARS_SKIPPED_WHILE_SAVING barsFailedCheckOHLCV[" + barsFailedCheckOHLCV + "] barsReadTotal[" + barsReadTotal + "] bars.Count[" + bars.Count + "]";
				Assembler.PopupException(msg, null, false);
				if (resaveBarsIfThereWasFailedCheckOHLCV) {
					resaveRequired = true;
				}
			}
			if (resaveRequired) {
				int reSaved = this.BarsSaveThreadSafe(bars);
				string msg2 = "RE-SAVED_TO_REMOVE_BARS_ALL_ZEROES reSaved[" + reSaved + "]";
				Assembler.PopupException(msg2, null, false);
			}

			return bars;
		}
		public int BarsSaveThreadSafe(Bars bars) {
			//BARS_INITIALIZED_EMPTY if (bars.Count == 0) return 0;
			int barsSaved = -1;
			lock (this.fileReadWriteSequentialLock) {
				barsSaved = this.barsSave(bars);
				//Assembler.PopupException("Saved [ " + bars.Count + "] bars; symbol[" + bars.Symbol + "] scaleInterval[" + bars.ScaleInterval + "]");
			}
			return barsSaved;
		}
		int barsSave(Bars bars) {
			string msig = " barsSave(" + bars + ")=>[" + this.Abspath + "]";
			int barsSaved = 0;
			int barsFailedCheckOHLCV = 0;

			FileStream fileStream = null;
			try {
				// ALL_THROW_IN_VS2010
				//fileStream = File.Create(this.Abspath);
				//fileStream = File.Create(this.Abspath, 1024*1024, FileOptions.SequentialScan);
				//fileStream = File.Open(this.Abspath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
				//fileStream = File.Open(this.Abspath, FileMode.Truncate, FileAccess.Write, FileShare.None);
				//fileStream = File.OpenWrite(this.Abspath);
				// SAME_THING_WORKS_IN_SHARP_DEVELOP
				fileStream = File.Open(this.Abspath, FileMode.Truncate, FileAccess.Write, FileShare.None);
			} catch (Exception ex) {
				string msg = "1/4_FILE_OPEN_THROWN";
				Assembler.PopupException(msg + msig, ex);
				return barsSaved;
			}
			try {
				// TODO create header structure and have its length the same both for Read & Write
				// HEADER BEGIN
				BinaryWriter binaryWriter = new BinaryWriter(fileStream);
				binaryWriter.Write((double)this.barFileCurrentVersion); // yes it was double :)
				if (this.barFileCurrentVersion == 1) {
					binaryWriter.Write(bars.Symbol);
					binaryWriter.Write(bars.SymbolHumanReadable);
				} else if (this.barFileCurrentVersion <= 2) {
					byte[] byteBufferSymbol = this.stringToByteArray(bars.Symbol, this.symbolMaxLength);
					#if DEBUG
					//TESTED Debugger.Break();
					#endif
					binaryWriter.Write(byteBufferSymbol);
					byte[] byteBufferSymbolHR = this.stringToByteArray(bars.SymbolHumanReadable, this.symbolHRMaxLength);
					binaryWriter.Write(byteBufferSymbolHR);
				} else if (this.barFileCurrentVersion <= 3) {
					// NO_SYMBOL_AND_HR_IS_PRESENT_IN_FILE
				}
				binaryWriter.Write((int)bars.ScaleInterval.Scale);
				binaryWriter.Write(bars.ScaleInterval.Interval);
				binaryWriter.Write(bars.Count);
				// HEADER END
				for (int i = 0; i < bars.Count; i++) {
					Bar bar = bars[i];
					try {
						bar.CheckOHLCVthrow();	//	catching the exception will display stacktrace in ExceptionsForm
					} catch (Exception ex) {
						barsFailedCheckOHLCV++;
						string msg = "NOT_SAVING_TO_FILE_THIS_BAR__TOO_LATE_TO_FIND_WHO_GENERATED_IT barAllZeroes bar[" + bar + "]";
						Assembler.PopupException(msg, ex, false);
						continue;
					}
					binaryWriter.Write(bar.DateTimeOpen.Ticks);
					binaryWriter.Write(bar.Open);
					binaryWriter.Write(bar.High);
					binaryWriter.Write(bar.Low);
					binaryWriter.Write(bar.Close);
					binaryWriter.Write(bar.Volume);
					barsSaved++;
				}
			} catch (Exception ex) {
				string msg = "Error while Saving bars[" + this + "] into [" + this.Abspath + "]";
				Assembler.PopupException(msg, ex);
			} finally {
				if (fileStream != null) {
					fileStream.Close();
					fileStream.Dispose();
				}
			}
			if (barsFailedCheckOHLCV > 0) {
				string msg = "SOME_BARS_SKIPPED_WHILE_SAVING barsFailedCheckOHLCV[" + barsFailedCheckOHLCV + "] barsSaved[" + barsSaved + "] bars.Count[" + bars.Count + "]";
				Assembler.PopupException(msg, null, false);
			}
			return barsSaved;
		}
		#region v2-related fixed-width routines for Symbol and SymbolHR (useless for v3 but still invoked)
		// http://stackoverflow.com/questions/472906/converting-a-string-to-byte-array
		byte[] stringToByteArray(string symbol, int bufferLength) {
			byte[] ret = new byte[bufferLength];
			//v1
			//string symbolTruncated = symbol;
			//if (symbolTruncated.Length > byteBuffer.Length) {
			//	symbolTruncated = symbolTruncated.Substring(0, byteBuffer.Length);
			//	string msg = "";
			//	Assembler.PopupException("TRUNCATED_SYMBOL_TO_FIT_BARFILE_HEADER v[" + this.barFileCurrentVersion + "](" + bufferLength+ ")bytes [" + symbol + "]=>[" + symbolTruncated + "]");
			//}
			//System.Buffer.BlockCopy(symbolTruncated.ToCharArray(), 0, byteBuffer, 0, symbolTruncated.Length);
			//v2
			byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(symbol);
			int min = Math.Min(utf8.Length, ret.Length);
			System.Buffer.BlockCopy(utf8, 0, ret, 0, min);
			string reconstructed = this.reconstructFromByteArray(ret);
			if (reconstructed.Length != symbol.Length) {
				string msg = "TRUNCATED_SYMBOL_TO_FIT_BARFILE_HEADER v[" + this.barFileCurrentVersion + "](" + bufferLength + ")bytes [" + symbol + "]=>[" + reconstructed + "]";
				Assembler.PopupException(msg);
			}
			return ret;
		}
		string byteArrayToString(byte[] bytes) {
			char[] chars = new char[bytes.Length / sizeof(char)];
			//v1 System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			//v2
			//for (int i = 0; i < chars.Length; i++) {
			//	if (chars[i] == 0) break;	// want to avoid "RIM3\0\0\0\0\0\0"
			//	System.Buffer.BlockCopy(bytes, i, chars, i, 1);
			//}
			//string ret = new string(chars);

			string ret = this.reconstructFromByteArray(bytes);
			return ret;
		}
		string reconstructFromByteArray(byte[] bytes) {
			string reconstructed = System.Text.Encoding.UTF8.GetString(bytes);
			char[] filtered = new char[bytes.Length];
			int validDestIndex = 0;
			foreach (char c in reconstructed.ToCharArray()) {
				if (c == 0) continue;	// avoiding "RIM3\0\0\0\0\0\0" and  "R\0I\0M\03\0\0\0\0\0\0"
				filtered[validDestIndex++] = c;
			}
			char[] final = new char[validDestIndex];
			//System.Buffer.BlockCopy(filtered, 0, final, 0, final.Length);
			for (int i = 0; i < final.Length; i++) {
				final[i] = filtered[i];
			}
			string ret = new string(final);
			return ret;
		}
		#endregion

		public int BarAppendThreadSafe(Bar barLastFormed) {
			//BARS_INITIALIZED_EMPTY if (bars.Count == 0) return 0;
			int barsAppended = -1;
			lock (this.fileReadWriteSequentialLock) {
				barsAppended = this.barAppend(barLastFormed);
				//Assembler.PopupException("Saved [ " + bars.Count + "] bars; symbol[" + bars.Symbol + "] scaleInterval[" + bars.ScaleInterval + "]");
			}
			return barsAppended;
		}
		int barAppend(Bar barLastFormed) {
			//v1
			//Bars allBars = this.BarsLoadAllThreadSafe();
			//if (allBars == null) {
			//	allBars = new Bars(barLastFormed.Symbol, barLastFormed.ScaleInterval, "DUMMY: LoadBars()=null");
			//}
			////allBars.DumpPartialInitFromStreamingBar(bar);

			//// this happens on a very first quote - this.pushBarToConsumers(StreamingBarFactory.LastBarFormed.Clone());
			//if (allBars.BarStaticLastNullUnsafe.DateTimeOpen == barLastFormed.DateTimeOpen) return 0;

			//// not really needed to clone to save it in a file, but we became strict to eliminate other bugs
			//barLastFormed = barLastFormed.CloneDetached();

			//// SetParentForBackwardUpdateAutoindex used within Bar only()
			////barLastFormed.SetParentForBackwardUpdateAutoindex(allBars);
			//if (allBars.BarStaticLastNullUnsafe.DateTimeOpen == barLastFormed.DateTimeOpen) {
			//	return 0;
			//}

			//allBars.BarAppendBindStatic(barLastFormed);
			//int barsSaved = this.BarsSaveThreadSafe(allBars);

			//v2, starting from barFileCurrentVersion=3: seek to the end, read last Bar, overwrite if same date or append if greater; 0.1ms instead of reading all - appending - writing all

			int saved = 0;
			string msig = " BarAppend(" + barLastFormed + ")=>[" + this.Abspath + "]";

			try {
				barLastFormed.CheckOHLCVthrow();	//	catching the exception will display stacktrace in ExceptionsForm
			} catch (Exception ex) {
				string msg = "NOT_APPENDING_TO_FILE_THIS_BAR__FIX_WHO_GENERATED_IT_UPSTACK barAllZeroes barLastFormed[" + barLastFormed + "]";
				Assembler.PopupException(msg + msig, ex, false);
				return saved;
			}

			FileStream fileStream = null;
			try {
				fileStream = File.Open(this.Abspath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			} catch (Exception ex) {
				string msg = "1/4_FILE_OPEN_THROWN";
				Assembler.PopupException(msg + msig, ex);
				return 0;
			}
			try {
				BinaryWriter binaryWriter = new BinaryWriter(fileStream);
				BinaryReader binaryReader = new BinaryReader(fileStream);
				//long barSize = this.barSizesByVersion[this.barFileCurrentVersion];
				try {
					// THIS_WAS_GENERATING_ZERO_BAR__YOU_WANTED_TO_PASS_NEGATIVE_VALUE_RELATIVE_TO_END_TO_SEEK_BACK_AND_ANALYZE_DATE_IF_STREAMING_SHOULD_BE_OVERWRITTEN_OR_STATIC_APPENDED fileStream.Seek(barSize, SeekOrigin.End);
					fileStream.Seek(0, SeekOrigin.End);
				} catch (Exception ex) {
					string msg = "2/4_FILESTREAM_SEEK_END_THROWN Seek(0, SeekOrigin.End)";
					Assembler.PopupException(msg + msig, ex);
					return 0;
				}
				//DateTime dateTimeOpen = new DateTime(binaryReader.ReadInt64());
				//if (dateTimeOpen >= barLastFormed.DateTimeOpen) {
				//    try {
				//        fileStream.Seek(barSize, SeekOrigin.End);	// overwrite the last bar, apparently streaming has been solidified
				//    } catch (Exception ex) {
				//        string msg = "3/4_FILESTREAM_SEEK_END_THROWN barSize[" + barSize + "]";
				//        Assembler.PopupException(msg + msig, ex);
				//        return 0;
				//    }
				//} else {
				//    try {
				//        fileStream.Seek(0, SeekOrigin.End);			// append barLastFormed to file since it's newer than last saved/read
				//    } catch (Exception ex) {
				//        string msg = "3/4_FILESTREAM_SEEK_0_THROWN";
				//        Assembler.PopupException(msg + msig, ex);
				//        return 0;
				//    }
				//}
				try {
					binaryWriter.Write(barLastFormed.DateTimeOpen.Ticks);
					binaryWriter.Write(barLastFormed.Open);
					binaryWriter.Write(barLastFormed.High);
					binaryWriter.Write(barLastFormed.Low);
					binaryWriter.Write(barLastFormed.Close);
					binaryWriter.Write(barLastFormed.Volume);
					saved++;
				} catch (Exception ex) {
					string msg = "4/4_BINARYWRITER_WRITER_THROWN";
					Assembler.PopupException(msg + msig, ex);
					return 0;
				}
			} finally {
				if (fileStream != null) {
					fileStream.Close();
					fileStream.Dispose();
				}
			}
			return saved;
		}
		
		public override string ToString() {
			return this.Relpath;
		}
		public static string GetRelpathFromEnd(string abspath, int partsFromEnd = 3) {
			string ret = "";
			string[] splitted = abspath.Split(new char[] {Path.DirectorySeparatorChar});
			for (int i=1; i<=partsFromEnd; i++) {
				int partToTake = splitted.Length - i;
				if (partToTake < 0) break;
				string thisPart = splitted[partToTake];
				if (ret != "") ret = Path.DirectorySeparatorChar + ret;
				ret = thisPart + ret;
			}
			return ret;
		}
	}
}