namespace BoggleClient
{
    partial class StartForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.GoButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.ServerLabel = new System.Windows.Forms.Label();
            this.UserLabel = new System.Windows.Forms.Label();
            this.TimeLabel = new System.Windows.Forms.Label();
            this.ServerText = new System.Windows.Forms.TextBox();
            this.UserText = new System.Windows.Forms.TextBox();
            this.TimeText = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.HelpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.DefaultButton = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // GoButton
            // 
            this.GoButton.Location = new System.Drawing.Point(25, 135);
            this.GoButton.Name = "GoButton";
            this.GoButton.Size = new System.Drawing.Size(75, 23);
            this.GoButton.TabIndex = 0;
            this.GoButton.Text = "GO";
            this.GoButton.UseVisualStyleBackColor = true;
            this.GoButton.Click += new System.EventHandler(this.GoButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Enabled = false;
            this.CancelButton.Location = new System.Drawing.Point(117, 135);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "CANCEL";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ServerLabel
            // 
            this.ServerLabel.AutoSize = true;
            this.ServerLabel.Location = new System.Drawing.Point(13, 37);
            this.ServerLabel.Name = "ServerLabel";
            this.ServerLabel.Size = new System.Drawing.Size(66, 13);
            this.ServerLabel.TabIndex = 2;
            this.ServerLabel.Text = "ServerName";
            // 
            // UserLabel
            // 
            this.UserLabel.AutoSize = true;
            this.UserLabel.Location = new System.Drawing.Point(13, 71);
            this.UserLabel.Name = "UserLabel";
            this.UserLabel.Size = new System.Drawing.Size(57, 13);
            this.UserLabel.TabIndex = 3;
            this.UserLabel.Text = "UserName";
            // 
            // TimeLabel
            // 
            this.TimeLabel.AutoSize = true;
            this.TimeLabel.Location = new System.Drawing.Point(13, 106);
            this.TimeLabel.Name = "TimeLabel";
            this.TimeLabel.Size = new System.Drawing.Size(78, 13);
            this.TimeLabel.TabIndex = 4;
            this.TimeLabel.Text = "Game Duration";
            // 
            // ServerText
            // 
            this.ServerText.Location = new System.Drawing.Point(107, 34);
            this.ServerText.Name = "ServerText";
            this.ServerText.Size = new System.Drawing.Size(100, 20);
            this.ServerText.TabIndex = 5;
            // 
            // UserText
            // 
            this.UserText.Location = new System.Drawing.Point(107, 68);
            this.UserText.Name = "UserText";
            this.UserText.Size = new System.Drawing.Size(100, 20);
            this.UserText.TabIndex = 6;
            // 
            // TimeText
            // 
            this.TimeText.Location = new System.Drawing.Point(107, 103);
            this.TimeText.Name = "TimeText";
            this.TimeText.Size = new System.Drawing.Size(100, 20);
            this.TimeText.TabIndex = 7;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HelpMenu});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(227, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // HelpMenu
            // 
            this.HelpMenu.Name = "HelpMenu";
            this.HelpMenu.Size = new System.Drawing.Size(44, 20);
            this.HelpMenu.Text = "Help";
            this.HelpMenu.Click += new System.EventHandler(this.HelpMenu_Click);
            // 
            // DefaultButton
            // 
            this.DefaultButton.Location = new System.Drawing.Point(50, 169);
            this.DefaultButton.Name = "DefaultButton";
            this.DefaultButton.Size = new System.Drawing.Size(117, 23);
            this.DefaultButton.TabIndex = 9;
            this.DefaultButton.Text = "Default Settings";
            this.DefaultButton.UseVisualStyleBackColor = true;
            this.DefaultButton.Click += new System.EventHandler(this.DefaultButton_Click);
            // 
            // StartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(227, 204);
            this.Controls.Add(this.DefaultButton);
            this.Controls.Add(this.TimeText);
            this.Controls.Add(this.UserText);
            this.Controls.Add(this.ServerText);
            this.Controls.Add(this.TimeLabel);
            this.Controls.Add(this.UserLabel);
            this.Controls.Add(this.ServerLabel);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.GoButton);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "StartForm";
            this.Text = "Boggle Setup";
            this.Load += new System.EventHandler(this.StartForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GoButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label ServerLabel;
        private System.Windows.Forms.Label UserLabel;
        private System.Windows.Forms.Label TimeLabel;
        private System.Windows.Forms.TextBox ServerText;
        private System.Windows.Forms.TextBox UserText;
        private System.Windows.Forms.TextBox TimeText;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem HelpMenu;
        private System.Windows.Forms.Button DefaultButton;
    }
}

