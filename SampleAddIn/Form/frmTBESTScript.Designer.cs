using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TBESTScripting
{
	public partial class frmTBESTScript : Form
	{

		// Form overrides dispose to clean up the component list.
		[DebuggerNonUserCode()]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components is not null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		// Required by the Windows Form Designer
		private System.ComponentModel.IContainer components = null;

		// NOTE: The following procedure is required by the Windows Form Designer
		// It can be modified using the Windows Form Designer.  
		// Do not modify it using the code editor.
		[DebuggerStepThrough()]
		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this._Frame1 = new System.Windows.Forms.GroupBox();
			this.btnGo = new System.Windows.Forms.Button();
			this._cboSystems = new System.Windows.Forms.ComboBox();
			this._cboAction = new System.Windows.Forms.ComboBox();
			this._Label4 = new System.Windows.Forms.Label();
			this._cboScenarios = new System.Windows.Forms.ComboBox();
			this._Label1 = new System.Windows.Forms.Label();
			this._Label2 = new System.Windows.Forms.Label();
			this._chkSelectAll = new System.Windows.Forms.CheckBox();
			this.DataGridViewRoutes = new System.Windows.Forms.DataGridView();
			this.colSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.colRouteName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.btnClose = new System.Windows.Forms.Button();
			this.Panel2 = new System.Windows.Forms.Panel();
			this._Label5 = new System.Windows.Forms.Label();
			this.Panel3 = new System.Windows.Forms.Panel();
			this._Frame1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewRoutes)).BeginInit();
			this.Panel2.SuspendLayout();
			this.Panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// _Frame1
			// 
			this._Frame1.BackColor = System.Drawing.SystemColors.Control;
			this._Frame1.Controls.Add(this.btnGo);
			this._Frame1.Controls.Add(this._cboSystems);
			this._Frame1.Controls.Add(this._cboAction);
			this._Frame1.Controls.Add(this._Label4);
			this._Frame1.Controls.Add(this._cboScenarios);
			this._Frame1.Controls.Add(this._Label1);
			this._Frame1.Controls.Add(this._Label2);
			this._Frame1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Frame1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._Frame1.Location = new System.Drawing.Point(14, 11);
			this._Frame1.Name = "_Frame1";
			this._Frame1.Padding = new System.Windows.Forms.Padding(0);
			this._Frame1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Frame1.Size = new System.Drawing.Size(473, 142);
			this._Frame1.TabIndex = 4;
			this._Frame1.TabStop = false;
			this._Frame1.Text = "Select a Transit System and Network Scenario...";
			// 
			// btnGo
			// 
			this.btnGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnGo.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.btnGo.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.btnGo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnGo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnGo.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.btnGo.Location = new System.Drawing.Point(374, 95);
			this.btnGo.Name = "btnGo";
			this.btnGo.Size = new System.Drawing.Size(46, 23);
			this.btnGo.TabIndex = 16;
			this.btnGo.Text = "Go";
			this.btnGo.UseVisualStyleBackColor = false;
			this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
			// 
			// _cboSystems
			// 
			this._cboSystems.BackColor = System.Drawing.Color.White;
			this._cboSystems.Cursor = System.Windows.Forms.Cursors.Default;
			this._cboSystems.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._cboSystems.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._cboSystems.Location = new System.Drawing.Point(151, 31);
			this._cboSystems.Name = "_cboSystems";
			this._cboSystems.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._cboSystems.Size = new System.Drawing.Size(217, 23);
			this._cboSystems.TabIndex = 5;
			this._cboSystems.Text = "- Select One -";
			this._cboSystems.SelectedIndexChanged += new System.EventHandler(this.cboSystems_SelectedIndexChanged);
			// 
			// _cboAction
			// 
			this._cboAction.BackColor = System.Drawing.Color.White;
			this._cboAction.Cursor = System.Windows.Forms.Cursors.Default;
			this._cboAction.Enabled = false;
			this._cboAction.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._cboAction.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._cboAction.Items.AddRange(new object[] {
            "Load Routes"});
			this._cboAction.Location = new System.Drawing.Point(151, 95);
			this._cboAction.Name = "_cboAction";
			this._cboAction.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._cboAction.Size = new System.Drawing.Size(217, 23);
			this._cboAction.TabIndex = 8;
			this._cboAction.Text = "- Select One -";
			// 
			// _Label4
			// 
			this._Label4.BackColor = System.Drawing.SystemColors.Control;
			this._Label4.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._Label4.Location = new System.Drawing.Point(17, 98);
			this._Label4.Name = "_Label4";
			this._Label4.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label4.Size = new System.Drawing.Size(97, 17);
			this._Label4.TabIndex = 9;
			this._Label4.Text = "TBEST Action:";
			// 
			// _cboScenarios
			// 
			this._cboScenarios.BackColor = System.Drawing.Color.White;
			this._cboScenarios.Cursor = System.Windows.Forms.Cursors.Default;
			this._cboScenarios.Enabled = false;
			this._cboScenarios.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._cboScenarios.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._cboScenarios.Location = new System.Drawing.Point(151, 63);
			this._cboScenarios.Name = "_cboScenarios";
			this._cboScenarios.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._cboScenarios.Size = new System.Drawing.Size(217, 23);
			this._cboScenarios.TabIndex = 4;
			this._cboScenarios.Text = "- Select One -";
			this._cboScenarios.SelectedIndexChanged += new System.EventHandler(this.cboScenarios_SelectedIndexChanged);
			// 
			// _Label1
			// 
			this._Label1.BackColor = System.Drawing.SystemColors.Control;
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
			// _Label2
			// 
			this._Label2.BackColor = System.Drawing.SystemColors.Control;
			this._Label2.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._Label2.Location = new System.Drawing.Point(17, 66);
			this._Label2.Name = "_Label2";
			this._Label2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label2.Size = new System.Drawing.Size(119, 20);
			this._Label2.TabIndex = 6;
			this._Label2.Text = "Network Scenario:";
			// 
			// _chkSelectAll
			// 
			this._chkSelectAll.AutoSize = true;
			this._chkSelectAll.BackColor = System.Drawing.SystemColors.Control;
			this._chkSelectAll.Cursor = System.Windows.Forms.Cursors.Default;
			this._chkSelectAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._chkSelectAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._chkSelectAll.Location = new System.Drawing.Point(18, 401);
			this._chkSelectAll.Name = "_chkSelectAll";
			this._chkSelectAll.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._chkSelectAll.Size = new System.Drawing.Size(74, 19);
			this._chkSelectAll.TabIndex = 12;
			this._chkSelectAll.Text = "Select All";
			this._chkSelectAll.UseVisualStyleBackColor = false;
			this._chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
			// 
			// DataGridViewRoutes
			// 
			this.DataGridViewRoutes.AllowUserToAddRows = false;
			this.DataGridViewRoutes.AllowUserToDeleteRows = false;
			this.DataGridViewRoutes.AllowUserToOrderColumns = true;
			this.DataGridViewRoutes.AllowUserToResizeColumns = false;
			this.DataGridViewRoutes.AllowUserToResizeRows = false;
			this.DataGridViewRoutes.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
			this.DataGridViewRoutes.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.DataGridViewRoutes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.DataGridViewRoutes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.DataGridViewRoutes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSelect,
            this.colRouteName});
			this.DataGridViewRoutes.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
			this.DataGridViewRoutes.Location = new System.Drawing.Point(14, 180);
			this.DataGridViewRoutes.Name = "DataGridViewRoutes";
			this.DataGridViewRoutes.RowHeadersVisible = false;
			this.DataGridViewRoutes.Size = new System.Drawing.Size(473, 215);
			this.DataGridViewRoutes.TabIndex = 13;
			// 
			// colSelect
			// 
			this.colSelect.HeaderText = "";
			this.colSelect.Name = "colSelect";
			this.colSelect.Width = 30;
			// 
			// colRouteName
			// 
			this.colRouteName.HeaderText = "Route";
			this.colRouteName.Name = "colRouteName";
			this.colRouteName.Width = 400;
			// 
			// btnClose
			// 
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClose.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.btnClose.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnClose.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.btnClose.Location = new System.Drawing.Point(398, 7);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(89, 28);
			this.btnClose.TabIndex = 15;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = false;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// Panel2
			// 
			this.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.Panel2.Controls.Add(this._Label5);
			this.Panel2.Controls.Add(this._Frame1);
			this.Panel2.Controls.Add(this._chkSelectAll);
			this.Panel2.Controls.Add(this.DataGridViewRoutes);
			this.Panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Panel2.Location = new System.Drawing.Point(0, 0);
			this.Panel2.Name = "Panel2";
			this.Panel2.Size = new System.Drawing.Size(499, 465);
			this.Panel2.TabIndex = 17;
			// 
			// _Label5
			// 
			this._Label5.BackColor = System.Drawing.SystemColors.Control;
			this._Label5.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(13)))), ((int)(((byte)(30)))));
			this._Label5.Location = new System.Drawing.Point(15, 160);
			this._Label5.Name = "_Label5";
			this._Label5.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label5.Size = new System.Drawing.Size(222, 17);
			this._Label5.TabIndex = 14;
			this._Label5.Text = "Network Scenario - Route List";
			// 
			// Panel3
			// 
			this.Panel3.BackColor = System.Drawing.SystemColors.Control;
			this.Panel3.Controls.Add(this.btnClose);
			this.Panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Panel3.Location = new System.Drawing.Point(0, 423);
			this.Panel3.Name = "Panel3";
			this.Panel3.Size = new System.Drawing.Size(499, 42);
			this.Panel3.TabIndex = 18;
			// 
			// frmTBESTScript
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(499, 465);
			this.Controls.Add(this.Panel3);
			this.Controls.Add(this.Panel2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "frmTBESTScript";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TBEST Scripting - Sample Application";
			this.Load += new System.EventHandler(this.frmTBESTScript_Load);
			this._Frame1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewRoutes)).EndInit();
			this.Panel2.ResumeLayout(false);
			this.Panel2.PerformLayout();
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
		private ComboBox _cboScenarios;

		public virtual ComboBox cboScenarios
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _cboScenarios;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				if (_cboScenarios != null)
				{
					_cboScenarios.SelectedIndexChanged -= cboScenarios_SelectedIndexChanged;
				}

				_cboScenarios = value;
				if (_cboScenarios != null)
				{
					_cboScenarios.SelectedIndexChanged += cboScenarios_SelectedIndexChanged;
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
		private Label _Label2;

		public virtual Label Label2
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _Label2;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_Label2 = value;
			}
		}
		private CheckBox _chkSelectAll;

		public virtual CheckBox chkSelectAll
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _chkSelectAll;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				if (_chkSelectAll != null)
				{
					_chkSelectAll.CheckedChanged -= chkSelectAll_CheckedChanged;
				}

				_chkSelectAll = value;
				if (_chkSelectAll != null)
				{
					_chkSelectAll.CheckedChanged += chkSelectAll_CheckedChanged;
				}
			}
		}
		internal DataGridView DataGridViewRoutes;
		internal Button btnClose;
		internal DataGridViewCheckBoxColumn colSelect;
		internal DataGridViewTextBoxColumn colRouteName;
		internal Panel Panel2;
		internal Panel Panel3;
		internal Button btnGo;
		private ComboBox _cboAction;

		public virtual ComboBox cboAction
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _cboAction;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_cboAction = value;
			}
		}
		private Label _Label4;

		public virtual Label Label4
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				return _Label4;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				_Label4 = value;
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
	}
}