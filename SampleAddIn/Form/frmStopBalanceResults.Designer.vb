<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmStopBalanceResults
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.lblNoStops = New System.Windows.Forms.Label()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.btnExcel = New System.Windows.Forms.Button()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.grdBusStopBalancing = New System.Windows.Forms.DataGridView()
        Me.Panel3.SuspendLayout()
        CType(Me.grdBusStopBalancing, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblNoStops
        '
        Me.lblNoStops.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblNoStops.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblNoStops.Location = New System.Drawing.Point(0, 0)
        Me.lblNoStops.Name = "lblNoStops"
        Me.lblNoStops.Size = New System.Drawing.Size(1421, 551)
        Me.lblNoStops.TabIndex = 19
        Me.lblNoStops.Text = "Label1"
        Me.lblNoStops.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Panel3
        '
        Me.Panel3.BackColor = System.Drawing.SystemColors.ButtonHighlight
        Me.Panel3.Controls.Add(Me.btnExcel)
        Me.Panel3.Controls.Add(Me.cmdOK)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel3.Location = New System.Drawing.Point(0, 551)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(1421, 50)
        Me.Panel3.TabIndex = 20
        '
        'btnExcel
        '
        Me.btnExcel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnExcel.AutoSize = True
        Me.btnExcel.BackColor = System.Drawing.SystemColors.ButtonHighlight
        Me.btnExcel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.btnExcel.Cursor = System.Windows.Forms.Cursors.Default
        Me.btnExcel.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnExcel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.btnExcel.Image = Global.TBESTBusStopBalancing.Resources.excel_2013
        Me.btnExcel.Location = New System.Drawing.Point(1210, 11)
        Me.btnExcel.Name = "btnExcel"
        Me.btnExcel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.btnExcel.Size = New System.Drawing.Size(120, 27)
        Me.btnExcel.TabIndex = 24
        Me.btnExcel.Text = "Export to Excel"
        Me.btnExcel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnExcel.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage
        Me.btnExcel.UseVisualStyleBackColor = False
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.AutoSize = True
        Me.cmdOK.BackColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdOK.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdOK.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!)
        Me.cmdOK.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdOK.Location = New System.Drawing.Point(1336, 11)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdOK.Size = New System.Drawing.Size(73, 27)
        Me.cmdOK.TabIndex = 0
        Me.cmdOK.Text = "Close"
        Me.cmdOK.UseVisualStyleBackColor = False
        '
        'grdBusStopBalancing
        '
        Me.grdBusStopBalancing.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Verdana", 8.25!)
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.grdBusStopBalancing.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        Me.grdBusStopBalancing.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(242, Byte), Integer), CType(CType(242, Byte), Integer))
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Verdana", 8.25!)
        DataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        DataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.grdBusStopBalancing.DefaultCellStyle = DataGridViewCellStyle2
        Me.grdBusStopBalancing.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grdBusStopBalancing.Location = New System.Drawing.Point(0, 0)
        Me.grdBusStopBalancing.Name = "grdBusStopBalancing"
        Me.grdBusStopBalancing.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.grdBusStopBalancing.RowHeadersVisible = False
        Me.grdBusStopBalancing.RowHeadersWidth = 23
        Me.grdBusStopBalancing.ShowEditingIcon = False
        Me.grdBusStopBalancing.Size = New System.Drawing.Size(1421, 551)
        Me.grdBusStopBalancing.TabIndex = 21
        '
        'frmStopBalanceResults
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1421, 601)
        Me.Controls.Add(Me.grdBusStopBalancing)
        Me.Controls.Add(Me.lblNoStops)
        Me.Controls.Add(Me.Panel3)
        Me.Name = "frmStopBalanceResults"
        Me.Text = "TBEST Bus Stop Balancing Results"
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        CType(Me.grdBusStopBalancing, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lblNoStops As System.Windows.Forms.Label
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Public WithEvents btnExcel As System.Windows.Forms.Button
    Public WithEvents cmdOK As System.Windows.Forms.Button
    Friend WithEvents grdBusStopBalancing As System.Windows.Forms.DataGridView
End Class
