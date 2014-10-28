﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using Sq1.Core.Broker;
using Sq1.Core.DataFeed;
using Sq1.Core.Accounting;

namespace Sq1.QuikAdapter {
	[ToolboxBitmap(typeof(QuikBrokerEditor), "BrokerQuik")]
	[Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
	public class QuikBrokerEditor : BrokerEditor {
		public string QuikFolder {
			get { return this.txtQuikFolder.Text; }
			set { this.txtQuikFolder.Text = value; }
		}
		public int ReconnectTimeoutMillis {
			get {
				int ret = 0;
				try {
					ret = Convert.ToInt32(this.txtReconnectTimeoutMillis.Text);
					this.txtReconnectTimeoutMillis.BackColor = Color.White;
				} catch (Exception e) {
					this.txtReconnectTimeoutMillis.BackColor = Color.LightCoral;
					this.txtReconnectTimeoutMillis.Text = "1000";	// induce one more event?...
				}
				return ret;
			}
			set { this.txtReconnectTimeoutMillis.Text = value.ToString(); }
		}
		public Account Account {
			get {
				Account ret;
				try {
					ret = new Account(this.txtQuikAccount.Text,
						Convert.ToDouble(this.txtCashAvailable.Text));
				} catch (Exception e) {
					ret = new Account();
					ret.AccountNumber = this.txtQuikAccount.Text;
				}
				return ret;
			}
			set {
				if (value == null) return;
				this.txtQuikAccount.Text = value.AccountNumber;
				this.txtCashAvailable.Text = value.CashAvailable.ToString();
			}
		}
		public Account AccountMicex {
			get {
				Account ret;
				try {
					ret = new Account(this.txtQuikAccountMicex.Text, Convert.ToDouble(this.txtCashAvailableMicex.Text));
				} catch (Exception e) {
					ret = new Account();
					ret.AccountNumber = this.txtQuikAccountMicex.Text;
				}
				return ret;
			}
			set {
				if (value == null) return;
				this.txtQuikAccountMicex.Text = value.AccountNumber;
				this.txtCashAvailableMicex.Text = value.CashAvailable.ToString();
			}
		}
		QuikBrokerProvider quikBrokerProvider {
			get { return base.brokerProvider as QuikBrokerProvider; }
		}

		public QuikBrokerEditor(QuikBrokerProvider quikBrokerProvider, IDataSourceEditor dataSourceEditor)
			: base(quikBrokerProvider, dataSourceEditor) {
			InitializeComponent();
			base.InitializeEditorFields();
		}
		public override void PushBrokerProviderSettingsToEditor() {
			this.Account = this.quikBrokerProvider.AccountAutoPropagate;
			// quik-specific
			this.AccountMicex = this.quikBrokerProvider.AccountMicexAutoPopulated;
			this.QuikFolder = this.quikBrokerProvider.QuikFolder;
			this.ReconnectTimeoutMillis = Convert.ToInt32(this.quikBrokerProvider.ReconnectTimeoutMillis);
			//QuikClientCode = SettingsEditor.QuikClientCode;
		}
		public override void PushEditedSettingsToBrokerProvider() {
			if (base.ignoreEditorFieldChangesWhileInitializingEditor) return;
			this.quikBrokerProvider.AccountAutoPropagate = this.Account;
			// quik-specific
			this.quikBrokerProvider.AccountMicexAutoPopulated = this.AccountMicex;
			this.quikBrokerProvider.QuikFolder = QuikFolder;
			this.quikBrokerProvider.ReconnectTimeoutMillis = ReconnectTimeoutMillis;
			//this.editor.QuikClientCode = QuikClientCode;
		}

        #region Component Designer generated code
		private System.ComponentModel.IContainer components = null;
		private Label lblQuikPath;
		private TextBox txtQuikFolder;
		private Label lblQuikAccount;
		private TextBox txtQuikAccount;
		private TextBox txtReconnectTimeoutMillis;
		private Label lblReconnectTimeoutMillis;
		private TextBox txtCashAvailable;
		private Label lblCashAvailable;
		private TextBox txtQuikAccountMicex;
		private TextBox txtCashAvailableMicex;
		private Label lblCashAvailableMicex;
		private Label lblQuikAccountMicex;
		private void InitializeComponent() {
			this.lblQuikPath = new System.Windows.Forms.Label();
			this.lblQuikAccount = new System.Windows.Forms.Label();
			this.txtQuikFolder = new System.Windows.Forms.TextBox();
			this.txtQuikAccount = new System.Windows.Forms.TextBox();
			this.txtReconnectTimeoutMillis = new System.Windows.Forms.TextBox();
			this.lblReconnectTimeoutMillis = new System.Windows.Forms.Label();
			this.txtCashAvailable = new System.Windows.Forms.TextBox();
			this.lblCashAvailable = new System.Windows.Forms.Label();
			this.txtQuikAccountMicex = new System.Windows.Forms.TextBox();
			this.lblQuikAccountMicex = new System.Windows.Forms.Label();
			this.txtCashAvailableMicex = new System.Windows.Forms.TextBox();
			this.lblCashAvailableMicex = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblQuikPath
			// 
			this.lblQuikPath.AutoSize = true;
			this.lblQuikPath.Location = new System.Drawing.Point(3, 6);
			this.lblQuikPath.Name = "lblQuikPath";
			this.lblQuikPath.Size = new System.Drawing.Size(54, 13);
			this.lblQuikPath.TabIndex = 0;
			this.lblQuikPath.Text = "Quik Path";
			// 
			// lblQuikAccount
			// 
			this.lblQuikAccount.AutoSize = true;
			this.lblQuikAccount.Location = new System.Drawing.Point(3, 84);
			this.lblQuikAccount.Name = "lblQuikAccount";
			this.lblQuikAccount.Size = new System.Drawing.Size(92, 13);
			this.lblQuikAccount.TabIndex = 1;
			this.lblQuikAccount.Text = "Account SPBFUT";
			// 
			// txtQuikFolder
			// 
			this.txtQuikFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtQuikFolder.Location = new System.Drawing.Point(3, 22);
			this.txtQuikFolder.Name = "txtQuikFolder";
			this.txtQuikFolder.Size = new System.Drawing.Size(238, 20);
			this.txtQuikFolder.TabIndex = 6;
			// 
			// txtQuikAccount
			// 
			this.txtQuikAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtQuikAccount.Location = new System.Drawing.Point(3, 100);
			this.txtQuikAccount.Name = "txtQuikAccount";
			this.txtQuikAccount.Size = new System.Drawing.Size(108, 20);
			this.txtQuikAccount.TabIndex = 7;
			// 
			// txtReconnectTimeoutMillis
			// 
			this.txtReconnectTimeoutMillis.Location = new System.Drawing.Point(3, 61);
			this.txtReconnectTimeoutMillis.Name = "txtReconnectTimeoutMillis";
			this.txtReconnectTimeoutMillis.Size = new System.Drawing.Size(108, 20);
			this.txtReconnectTimeoutMillis.TabIndex = 19;
			// 
			// lblReconnectTimeoutMillis
			// 
			this.lblReconnectTimeoutMillis.AutoSize = true;
			this.lblReconnectTimeoutMillis.Location = new System.Drawing.Point(3, 45);
			this.lblReconnectTimeoutMillis.Name = "lblReconnectTimeoutMillis";
			this.lblReconnectTimeoutMillis.Size = new System.Drawing.Size(82, 13);
			this.lblReconnectTimeoutMillis.TabIndex = 18;
			this.lblReconnectTimeoutMillis.Text = "ReconnectMillis";
			// 
			// txtCashAvailable
			// 
			this.txtCashAvailable.Location = new System.Drawing.Point(3, 139);
			this.txtCashAvailable.Name = "txtCashAvailable";
			this.txtCashAvailable.Size = new System.Drawing.Size(108, 20);
			this.txtCashAvailable.TabIndex = 21;
			// 
			// lblCashAvailable
			// 
			this.lblCashAvailable.AutoSize = true;
			this.lblCashAvailable.Location = new System.Drawing.Point(3, 123);
			this.lblCashAvailable.Name = "lblCashAvailable";
			this.lblCashAvailable.Size = new System.Drawing.Size(116, 13);
			this.lblCashAvailable.TabIndex = 20;
			this.lblCashAvailable.Text = "Cash Available FORTS";
			// 
			// txtQuikAccountMicex
			// 
			this.txtQuikAccountMicex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtQuikAccountMicex.Location = new System.Drawing.Point(133, 100);
			this.txtQuikAccountMicex.Name = "txtQuikAccountMicex";
			this.txtQuikAccountMicex.Size = new System.Drawing.Size(108, 20);
			this.txtQuikAccountMicex.TabIndex = 23;
			// 
			// lblQuikAccountMicex
			// 
			this.lblQuikAccountMicex.AutoSize = true;
			this.lblQuikAccountMicex.Location = new System.Drawing.Point(133, 84);
			this.lblQuikAccountMicex.Name = "lblQuikAccountMicex";
			this.lblQuikAccountMicex.Size = new System.Drawing.Size(102, 13);
			this.lblQuikAccountMicex.TabIndex = 22;
			this.lblQuikAccountMicex.Text = "Account * (EQBR...)";
			// 
			// txtCashAvailableMicex
			// 
			this.txtCashAvailableMicex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtCashAvailableMicex.Location = new System.Drawing.Point(130, 139);
			this.txtCashAvailableMicex.Name = "txtCashAvailableMicex";
			this.txtCashAvailableMicex.Size = new System.Drawing.Size(108, 20);
			this.txtCashAvailableMicex.TabIndex = 25;
			// 
			// lblCashAvailableMicex
			// 
			this.lblCashAvailableMicex.AutoSize = true;
			this.lblCashAvailableMicex.Location = new System.Drawing.Point(130, 123);
			this.lblCashAvailableMicex.Name = "lblCashAvailableMicex";
			this.lblCashAvailableMicex.Size = new System.Drawing.Size(113, 13);
			this.lblCashAvailableMicex.TabIndex = 24;
			this.lblCashAvailableMicex.Text = "Cash Available MICEX";
			// 
			// QuikBrokerProviderSettingsEditor
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.AutoScroll = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.txtCashAvailableMicex);
			this.Controls.Add(this.lblCashAvailableMicex);
			this.Controls.Add(this.txtQuikAccountMicex);
			this.Controls.Add(this.lblQuikAccountMicex);
			this.Controls.Add(this.txtCashAvailable);
			this.Controls.Add(this.lblCashAvailable);
			this.Controls.Add(this.txtReconnectTimeoutMillis);
			this.Controls.Add(this.lblReconnectTimeoutMillis);
			this.Controls.Add(this.txtQuikAccount);
			this.Controls.Add(this.txtQuikFolder);
			this.Controls.Add(this.lblQuikAccount);
			this.Controls.Add(this.lblQuikPath);
			this.Name = "QuikBrokerProviderSettingsEditor";
			this.Size = new System.Drawing.Size(244, 171);
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

    }
}