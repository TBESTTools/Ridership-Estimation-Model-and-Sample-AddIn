using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace RidershipEstimationModel
{

	public partial class frmTBESTStandAloneModelRun : Form
	{

		// Form overrides dispose to clean up the component list.
		[DebuggerNonUserCode()]

		[DebuggerStepThrough()]
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTBESTStandAloneModelRun));
			this._Frame1 = new System.Windows.Forms.GroupBox();
			this._cboSystems = new System.Windows.Forms.ComboBox();
			this._Label1 = new System.Windows.Forms.Label();
			this.btnGo = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.Panel2 = new System.Windows.Forms.Panel();
			this._GroupBox1 = new System.Windows.Forms.GroupBox();
			this._lstScenarios = new System.Windows.Forms.CheckedListBox();
			this._lstTimePeriods = new System.Windows.Forms.CheckedListBox();
			this._Label5 = new System.Windows.Forms.Label();
			this._Label6 = new System.Windows.Forms.Label();
			this.pgModelOptions = new Azuria.Common.Controls.FilteredPropertyGrid();
			this.Panel3 = new System.Windows.Forms.Panel();
			this._Frame1.SuspendLayout();
			this.Panel2.SuspendLayout();
			this._GroupBox1.SuspendLayout();
			this.Panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// _Frame1
			// 
			this._Frame1.BackColor = System.Drawing.Color.WhiteSmoke;
			this._Frame1.Controls.Add(this._cboSystems);
			this._Frame1.Controls.Add(this._Label1);
			this._Frame1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Frame1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._Frame1.Location = new System.Drawing.Point(14, 11);
			this._Frame1.Name = "_Frame1";
			this._Frame1.Padding = new System.Windows.Forms.Padding(0);
			this._Frame1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Frame1.Size = new System.Drawing.Size(444, 79);
			this._Frame1.TabIndex = 4;
			this._Frame1.TabStop = false;
			this._Frame1.Text = "Select a Transit System...";
			// 
			// _cboSystems
			// 
			this._cboSystems.BackColor = System.Drawing.Color.White;
			this._cboSystems.Cursor = System.Windows.Forms.Cursors.Default;
			this._cboSystems.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._cboSystems.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._cboSystems.Location = new System.Drawing.Point(113, 31);
			this._cboSystems.Name = "_cboSystems";
			this._cboSystems.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._cboSystems.Size = new System.Drawing.Size(298, 23);
			this._cboSystems.TabIndex = 5;
			this._cboSystems.Text = "- Select One -";
			this._cboSystems.SelectedIndexChanged += new System.EventHandler(this.cboSystems_SelectedIndexChanged);
			// 
			// _Label1
			// 
			this._Label1.BackColor = System.Drawing.Color.WhiteSmoke;
			this._Label1.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._Label1.Location = new System.Drawing.Point(17, 34);
			this._Label1.Name = "_Label1";
			this._Label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label1.Size = new System.Drawing.Size(97, 17);
			this._Label1.TabIndex = 7;
			this._Label1.Text = "Transit System:";
			// 
			// btnGo
			// 
			this.btnGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnGo.BackColor = System.Drawing.Color.WhiteSmoke;
			this.btnGo.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.btnGo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnGo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnGo.ForeColor = System.Drawing.SystemColors.ControlText;
			this.btnGo.Image = ((System.Drawing.Image)(resources.GetObject("btnGo.Image")));
			this.btnGo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnGo.Location = new System.Drawing.Point(263, 8);
			this.btnGo.Name = "btnGo";
			this.btnGo.Size = new System.Drawing.Size(95, 30);
			this.btnGo.TabIndex = 16;
			this.btnGo.Text = "Run Model";
			this.btnGo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnGo.UseVisualStyleBackColor = false;
			this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
			// 
			// btnClose
			// 
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClose.BackColor = System.Drawing.Color.WhiteSmoke;
			this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnClose.ForeColor = System.Drawing.SystemColors.ControlText;
			this.btnClose.Location = new System.Drawing.Point(369, 8);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(89, 30);
			this.btnClose.TabIndex = 15;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = false;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// Panel2
			// 
			this.Panel2.BackColor = System.Drawing.Color.WhiteSmoke;
			this.Panel2.Controls.Add(this._GroupBox1);
			this.Panel2.Controls.Add(this.pgModelOptions);
			this.Panel2.Controls.Add(this._Frame1);
			this.Panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Panel2.Location = new System.Drawing.Point(0, 0);
			this.Panel2.Name = "Panel2";
			this.Panel2.Size = new System.Drawing.Size(473, 432);
			this.Panel2.TabIndex = 17;
			// 
			// _GroupBox1
			// 
			this._GroupBox1.BackColor = System.Drawing.Color.WhiteSmoke;
			this._GroupBox1.Controls.Add(this._lstScenarios);
			this._GroupBox1.Controls.Add(this._lstTimePeriods);
			this._GroupBox1.Controls.Add(this._Label5);
			this._GroupBox1.Controls.Add(this._Label6);
			this._GroupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this._GroupBox1.ForeColor = System.Drawing.SystemColors.ControlText;
			this._GroupBox1.Location = new System.Drawing.Point(14, 96);
			this._GroupBox1.Name = "_GroupBox1";
			this._GroupBox1.Padding = new System.Windows.Forms.Padding(0);
			this._GroupBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._GroupBox1.Size = new System.Drawing.Size(444, 178);
			this._GroupBox1.TabIndex = 6;
			this._GroupBox1.TabStop = false;
			this._GroupBox1.Text = "Scenarios and Time Periods for Model Run";
			// 
			// _lstScenarios
			// 
			this._lstScenarios.BackColor = System.Drawing.Color.White;
			this._lstScenarios.CheckOnClick = true;
			this._lstScenarios.Cursor = System.Windows.Forms.Cursors.Default;
			this._lstScenarios.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this._lstScenarios.ForeColor = System.Drawing.SystemColors.WindowText;
			this._lstScenarios.Location = new System.Drawing.Point(8, 40);
			this._lstScenarios.Name = "_lstScenarios";
			this._lstScenarios.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._lstScenarios.Size = new System.Drawing.Size(231, 124);
			this._lstScenarios.TabIndex = 5;
			this._lstScenarios.SelectedIndexChanged += new System.EventHandler(this.lstScenarios_SelectedIndexChanged);
			// 
			// _lstTimePeriods
			// 
			this._lstTimePeriods.BackColor = System.Drawing.SystemColors.Window;
			this._lstTimePeriods.CheckOnClick = true;
			this._lstTimePeriods.Cursor = System.Windows.Forms.Cursors.Default;
			this._lstTimePeriods.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this._lstTimePeriods.ForeColor = System.Drawing.SystemColors.WindowText;
			this._lstTimePeriods.Location = new System.Drawing.Point(249, 40);
			this._lstTimePeriods.Name = "_lstTimePeriods";
			this._lstTimePeriods.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._lstTimePeriods.Size = new System.Drawing.Size(178, 124);
			this._lstTimePeriods.TabIndex = 1;
			// 
			// _Label5
			// 
			this._Label5.BackColor = System.Drawing.Color.WhiteSmoke;
			this._Label5.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this._Label5.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Label5.Location = new System.Drawing.Point(8, 24);
			this._Label5.Name = "_Label5";
			this._Label5.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label5.Size = new System.Drawing.Size(201, 17);
			this._Label5.TabIndex = 6;
			this._Label5.Text = "Select the Scenario(s) to model:";
			// 
			// _Label6
			// 
			this._Label6.BackColor = System.Drawing.Color.WhiteSmoke;
			this._Label6.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this._Label6.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Label6.Location = new System.Drawing.Point(246, 24);
			this._Label6.Name = "_Label6";
			this._Label6.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label6.Size = new System.Drawing.Size(181, 17);
			this._Label6.TabIndex = 4;
			this._Label6.Text = "Select Time Period(s) to model:";
			// 
			// pgModelOptions
			// 
			this.pgModelOptions.BrowsableProperties = null;
			this.pgModelOptions.HelpVisible = false;
			this.pgModelOptions.HiddenAttributes = null;
			this.pgModelOptions.HiddenProperties = null;
			this.pgModelOptions.Location = new System.Drawing.Point(14, 280);
			this.pgModelOptions.Name = "pgModelOptions";
			this.pgModelOptions.Size = new System.Drawing.Size(446, 89);
			this.pgModelOptions.TabIndex = 5;
			this.pgModelOptions.ToolbarVisible = false;
			// 
			// Panel3
			// 
			this.Panel3.BackColor = System.Drawing.Color.WhiteSmoke;
			this.Panel3.Controls.Add(this.btnGo);
			this.Panel3.Controls.Add(this.btnClose);
			this.Panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Panel3.Location = new System.Drawing.Point(0, 383);
			this.Panel3.Name = "Panel3";
			this.Panel3.Size = new System.Drawing.Size(473, 49);
			this.Panel3.TabIndex = 18;
			// 
			// frmTBESTStandAloneModelRun
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.ClientSize = new System.Drawing.Size(473, 432);
			this.Controls.Add(this.Panel3);
			this.Controls.Add(this.Panel2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmTBESTStandAloneModelRun";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TBEST Ridership Estimation Model";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmTBESTStandAloneModelRun_FormClosed);
			this.Load += new System.EventHandler(this.frmTBESTScript_Load);
			this._Frame1.ResumeLayout(false);
			this.Panel2.ResumeLayout(false);
			this._GroupBox1.ResumeLayout(false);
			this.Panel3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		private GroupBox _Frame1;

		public virtual GroupBox Frame1
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _Frame1;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_Frame1 = value;
			}
		}
		private ComboBox _cboSystems;

		public virtual ComboBox cboSystems
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _cboSystems;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				if (_cboSystems != null)
				{
					_cboSystems.SelectedIndexChanged -= cboSystems_SelectedIndexChanged;
				}

				_cboSystems = value;
				if (_cboSystems != null)
				{
					_cboSystems.SelectedIndexChanged += cboSystems_SelectedIndexChanged;
				}
			}
		}
		private Label _Label1;

		public virtual Label Label1
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _Label1;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_Label1 = value;
			}
		}
		internal Button btnClose;
		internal Panel Panel2;
		internal Panel Panel3;
		internal Button btnGo;
		internal Azuria.Common.Controls.FilteredPropertyGrid pgModelOptions;
		private GroupBox _GroupBox1;

		public virtual GroupBox GroupBox1
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _GroupBox1;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_GroupBox1 = value;
			}
		}
		private CheckedListBox _lstScenarios;

		public virtual CheckedListBox lstScenarios
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _lstScenarios;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				if (_lstScenarios != null)
				{
					_lstScenarios.SelectedIndexChanged -= lstScenarios_SelectedIndexChanged;
				}

				_lstScenarios = value;
				if (_lstScenarios != null)
				{
					_lstScenarios.SelectedIndexChanged += lstScenarios_SelectedIndexChanged;
				}
			}
		}
		private CheckedListBox _lstTimePeriods;

		public virtual CheckedListBox lstTimePeriods
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _lstTimePeriods;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_lstTimePeriods = value;
			}
		}
		private Label _Label5;

		public virtual Label Label5
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _Label5;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_Label5 = value;
			}
		}
		private Label _Label6;

		public virtual Label Label6
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _Label6;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_Label6 = value;
			}
		}
	}
}