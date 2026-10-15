namespace TRScriptSync
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.titleLabel = new System.Windows.Forms.Label();
            this.subtitleLabel = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.folderCaptionLabel = new System.Windows.Forms.Label();
            this.folderBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.mysqlCaptionLabel = new System.Windows.Forms.Label();
            this.hostBox = new System.Windows.Forms.TextBox();
            this.portBox = new System.Windows.Forms.TextBox();
            this.userBox = new System.Windows.Forms.TextBox();
            this.passBox = new System.Windows.Forms.TextBox();
            this.dbBox = new System.Windows.Forms.TextBox();
            this.scanButton = new System.Windows.Forms.Button();
            this.importButton = new System.Windows.Forms.Button();
            this.keepExtraCheck = new System.Windows.Forms.CheckBox();
            this.matchFileCheck = new System.Windows.Forms.CheckBox();
            this.rebuildItemsCheck = new System.Windows.Forms.CheckBox();
            this.progressPanel = new System.Windows.Forms.Panel();
            this.fileLogBox = new System.Windows.Forms.TextBox();
            this.statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.titleLabel.Location = new System.Drawing.Point(16, 12);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(360, 22);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "TRScriptSync";
            // 
            // subtitleLabel
            // 
            this.subtitleLabel.BackColor = System.Drawing.Color.Transparent;
            this.subtitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(158)))));
            this.subtitleLabel.Location = new System.Drawing.Point(16, 34);
            this.subtitleLabel.Name = "subtitleLabel";
            this.subtitleLabel.Size = new System.Drawing.Size(400, 18);
            this.subtitleLabel.TabIndex = 1;
            this.subtitleLabel.Text = "TalesRunner Script Sync";
            // 
            // closeButton
            // 
            this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.closeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.closeButton.FlatAppearance.BorderSize = 0;
            this.closeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.closeButton.Location = new System.Drawing.Point(480, 8);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(28, 28);
            this.closeButton.TabIndex = 2;
            this.closeButton.TabStop = false;
            this.closeButton.Text = "×";
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // folderCaptionLabel
            // 
            this.folderCaptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.folderCaptionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(158)))));
            this.folderCaptionLabel.Location = new System.Drawing.Point(16, 62);
            this.folderCaptionLabel.Name = "folderCaptionLabel";
            this.folderCaptionLabel.Size = new System.Drawing.Size(200, 16);
            this.folderCaptionLabel.TabIndex = 3;
            this.folderCaptionLabel.Text = "Script folder";
            // 
            // folderBox
            // 
            this.folderBox.AllowDrop = true;
            this.folderBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.folderBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.folderBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.folderBox.Location = new System.Drawing.Point(16, 80);
            this.folderBox.Name = "folderBox";
            this.folderBox.Size = new System.Drawing.Size(390, 26);
            this.folderBox.TabIndex = 4;
            this.folderBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.folderBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            // 
            // browseButton
            // 
            this.browseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.browseButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.browseButton.FlatAppearance.BorderSize = 0;
            this.browseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.browseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.browseButton.Location = new System.Drawing.Point(414, 79);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(90, 28);
            this.browseButton.TabIndex = 5;
            this.browseButton.TabStop = false;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = false;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // mysqlCaptionLabel
            // 
            this.mysqlCaptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.mysqlCaptionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(158)))));
            this.mysqlCaptionLabel.Location = new System.Drawing.Point(16, 114);
            this.mysqlCaptionLabel.Name = "mysqlCaptionLabel";
            this.mysqlCaptionLabel.Size = new System.Drawing.Size(200, 16);
            this.mysqlCaptionLabel.TabIndex = 6;
            this.mysqlCaptionLabel.Text = "MySQL  host / port / user / pass / database";
            // 
            // hostBox
            // 
            this.hostBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.hostBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hostBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.hostBox.Location = new System.Drawing.Point(16, 132);
            this.hostBox.Name = "hostBox";
            this.hostBox.Size = new System.Drawing.Size(108, 26);
            this.hostBox.TabIndex = 7;
            this.hostBox.Text = "127.0.0.1";
            // 
            // portBox
            // 
            this.portBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.portBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.portBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.portBox.Location = new System.Drawing.Point(128, 132);
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(52, 26);
            this.portBox.TabIndex = 8;
            this.portBox.Text = "3306";
            // 
            // userBox
            // 
            this.userBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.userBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.userBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.userBox.Location = new System.Drawing.Point(184, 132);
            this.userBox.Name = "userBox";
            this.userBox.Size = new System.Drawing.Size(72, 26);
            this.userBox.TabIndex = 9;
            this.userBox.Text = "root";
            // 
            // passBox
            // 
            this.passBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.passBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.passBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.passBox.Location = new System.Drawing.Point(260, 132);
            this.passBox.Name = "passBox";
            this.passBox.Size = new System.Drawing.Size(88, 26);
            this.passBox.TabIndex = 10;
            this.passBox.UseSystemPasswordChar = true;
            // 
            // dbBox
            // 
            this.dbBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.dbBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dbBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.dbBox.Location = new System.Drawing.Point(352, 132);
            this.dbBox.Name = "dbBox";
            this.dbBox.Size = new System.Drawing.Size(152, 26);
            this.dbBox.TabIndex = 11;
            this.dbBox.Text = "tr_game_db";
            // 
            // scanButton
            // 
            this.scanButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(166)))), ((int)(((byte)(255)))));
            this.scanButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.scanButton.FlatAppearance.BorderSize = 0;
            this.scanButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(180)))), ((int)(((byte)(255)))));
            this.scanButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scanButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.scanButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(18)))));
            this.scanButton.Location = new System.Drawing.Point(16, 168);
            this.scanButton.Name = "scanButton";
            this.scanButton.Size = new System.Drawing.Size(240, 28);
            this.scanButton.TabIndex = 12;
            this.scanButton.Text = "Scan";
            this.scanButton.UseVisualStyleBackColor = false;
            this.scanButton.Click += new System.EventHandler(this.scanButton_Click);
            // 
            // importButton
            // 
            this.importButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(166)))), ((int)(((byte)(255)))));
            this.importButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.importButton.FlatAppearance.BorderSize = 0;
            this.importButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(180)))), ((int)(((byte)(255)))));
            this.importButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.importButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.importButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(18)))));
            this.importButton.Location = new System.Drawing.Point(264, 168);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(240, 28);
            this.importButton.TabIndex = 13;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = false;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // keepExtraCheck
            // 
            this.keepExtraCheck.AutoSize = false;
            this.keepExtraCheck.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.keepExtraCheck.Cursor = System.Windows.Forms.Cursors.Hand;
            this.keepExtraCheck.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.keepExtraCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(188)))));
            this.keepExtraCheck.Location = new System.Drawing.Point(16, 204);
            this.keepExtraCheck.Name = "keepExtraCheck";
            this.keepExtraCheck.Size = new System.Drawing.Size(240, 20);
            this.keepExtraCheck.TabIndex = 14;
            this.keepExtraCheck.Text = "Keep extra DB columns";
            this.keepExtraCheck.UseVisualStyleBackColor = false;
            // 
            // matchFileCheck
            // 
            this.matchFileCheck.AutoSize = false;
            this.matchFileCheck.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.matchFileCheck.Cursor = System.Windows.Forms.Cursors.Hand;
            this.matchFileCheck.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.matchFileCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(188)))));
            this.matchFileCheck.Location = new System.Drawing.Point(264, 204);
            this.matchFileCheck.Name = "matchFileCheck";
            this.matchFileCheck.Size = new System.Drawing.Size(240, 20);
            this.matchFileCheck.TabIndex = 15;
            this.matchFileCheck.Text = "Make table match the file";
            this.matchFileCheck.UseVisualStyleBackColor = false;
            // 
            // rebuildItemsCheck
            // 
            this.rebuildItemsCheck.AutoSize = false;
            this.rebuildItemsCheck.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.rebuildItemsCheck.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rebuildItemsCheck.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.rebuildItemsCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(188)))));
            this.rebuildItemsCheck.Location = new System.Drawing.Point(16, 226);
            this.rebuildItemsCheck.Name = "rebuildItemsCheck";
            this.rebuildItemsCheck.Size = new System.Drawing.Size(488, 20);
            this.rebuildItemsCheck.TabIndex = 16;
            this.rebuildItemsCheck.Text = "Rebuild item lookups after import (usp_makeRealItemListTable)";
            this.rebuildItemsCheck.UseVisualStyleBackColor = false;
            // 
            // progressPanel
            // 
            this.progressPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.progressPanel.Location = new System.Drawing.Point(16, 254);
            this.progressPanel.Name = "progressPanel";
            this.progressPanel.Size = new System.Drawing.Size(488, 6);
            this.progressPanel.TabIndex = 17;
            this.progressPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.progressPanel_Paint);
            // 
            // fileLogBox
            // 
            this.fileLogBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.fileLogBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fileLogBox.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.fileLogBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.fileLogBox.Location = new System.Drawing.Point(16, 268);
            this.fileLogBox.Multiline = true;
            this.fileLogBox.Name = "fileLogBox";
            this.fileLogBox.ReadOnly = true;
            this.fileLogBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.fileLogBox.Size = new System.Drawing.Size(488, 118);
            this.fileLogBox.TabIndex = 18;
            this.fileLogBox.TabStop = false;
            // 
            // statusLabel
            // 
            this.statusLabel.BackColor = System.Drawing.Color.Transparent;
            this.statusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(158)))));
            this.statusLabel.Location = new System.Drawing.Point(16, 392);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(488, 16);
            this.statusLabel.TabIndex = 19;
            this.statusLabel.Text = "";
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.ClientSize = new System.Drawing.Size(520, 420);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.fileLogBox);
            this.Controls.Add(this.progressPanel);
            this.Controls.Add(this.rebuildItemsCheck);
            this.Controls.Add(this.matchFileCheck);
            this.Controls.Add(this.keepExtraCheck);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.scanButton);
            this.Controls.Add(this.dbBox);
            this.Controls.Add(this.passBox);
            this.Controls.Add(this.userBox);
            this.Controls.Add(this.portBox);
            this.Controls.Add(this.hostBox);
            this.Controls.Add(this.mysqlCaptionLabel);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.folderBox);
            this.Controls.Add(this.folderCaptionLabel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.subtitleLabel);
            this.Controls.Add(this.titleLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TRScriptSync";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label subtitleLabel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label folderCaptionLabel;
        private System.Windows.Forms.TextBox folderBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Label mysqlCaptionLabel;
        private System.Windows.Forms.TextBox hostBox;
        private System.Windows.Forms.TextBox portBox;
        private System.Windows.Forms.TextBox userBox;
        private System.Windows.Forms.TextBox passBox;
        private System.Windows.Forms.TextBox dbBox;
        private System.Windows.Forms.Button scanButton;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.CheckBox keepExtraCheck;
        private System.Windows.Forms.CheckBox matchFileCheck;
        private System.Windows.Forms.CheckBox rebuildItemsCheck;
        private System.Windows.Forms.Panel progressPanel;
        private System.Windows.Forms.TextBox fileLogBox;
        private System.Windows.Forms.Label statusLabel;
    }
}
