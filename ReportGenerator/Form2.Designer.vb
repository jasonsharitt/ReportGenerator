<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form2
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form2))
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblcurrentfilenumber = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.lbltotalfilecount = New System.Windows.Forms.Label()
        Me.lblcurrentfilename = New System.Windows.Forms.Label()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblcurrenttask = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(105, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Currently processing:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(13, 87)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(23, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "File"
        '
        'lblcurrentfilenumber
        '
        Me.lblcurrentfilenumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblcurrentfilenumber.Location = New System.Drawing.Point(42, 86)
        Me.lblcurrentfilenumber.Name = "lblcurrentfilenumber"
        Me.lblcurrentfilenumber.Size = New System.Drawing.Size(76, 24)
        Me.lblcurrentfilenumber.TabIndex = 2
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(124, 87)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(18, 13)
        Me.Label4.TabIndex = 3
        Me.Label4.Text = "Of"
        '
        'lbltotalfilecount
        '
        Me.lbltotalfilecount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lbltotalfilecount.Location = New System.Drawing.Point(148, 87)
        Me.lbltotalfilecount.Name = "lbltotalfilecount"
        Me.lbltotalfilecount.Size = New System.Drawing.Size(76, 24)
        Me.lbltotalfilecount.TabIndex = 4
        '
        'lblcurrentfilename
        '
        Me.lblcurrentfilename.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblcurrentfilename.Location = New System.Drawing.Point(123, 8)
        Me.lblcurrentfilename.Name = "lblcurrentfilename"
        Me.lblcurrentfilename.Size = New System.Drawing.Size(292, 23)
        Me.lblcurrentfilename.TabIndex = 5
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(16, 133)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(400, 23)
        Me.ProgressBar1.TabIndex = 6
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.IndianRed
        Me.Button1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button1.Location = New System.Drawing.Point(230, 86)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(186, 41)
        Me.Button1.TabIndex = 7
        Me.Button1.Text = "Cancel"
        Me.Button1.UseVisualStyleBackColor = False
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(46, 53)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(71, 13)
        Me.Label3.TabIndex = 8
        Me.Label3.Text = "Current Task:"
        '
        'lblcurrenttask
        '
        Me.lblcurrenttask.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblcurrenttask.Location = New System.Drawing.Point(123, 52)
        Me.lblcurrenttask.Name = "lblcurrenttask"
        Me.lblcurrenttask.Size = New System.Drawing.Size(292, 23)
        Me.lblcurrenttask.TabIndex = 9
        '
        'Form2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(427, 165)
        Me.Controls.Add(Me.lblcurrenttask)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.lblcurrentfilename)
        Me.Controls.Add(Me.lbltotalfilecount)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.lblcurrentfilenumber)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Form2"
        Me.Text = "Processing Reports"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents lblcurrentfilenumber As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents lbltotalfilecount As System.Windows.Forms.Label
    Friend WithEvents lblcurrentfilename As System.Windows.Forms.Label
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lblcurrenttask As System.Windows.Forms.Label
End Class
