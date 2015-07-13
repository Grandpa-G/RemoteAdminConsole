namespace RemoteAdminConsole
{
    partial class ChatForm
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
            this.chatformText = new System.Windows.Forms.TextBox();
            this.chatformSend = new System.Windows.Forms.Button();
            this.chatformClear = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.chatformChatWith = new System.Windows.Forms.Label();
            this.chatformList = new System.Windows.Forms.ListBox();
            this.chatformStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chatformText
            // 
            this.chatformText.Location = new System.Drawing.Point(15, 337);
            this.chatformText.Name = "chatformText";
            this.chatformText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.chatformText.Size = new System.Drawing.Size(366, 20);
            this.chatformText.TabIndex = 1;
            this.chatformText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.chatformText_KeyPress);
            // 
            // chatformSend
            // 
            this.chatformSend.Location = new System.Drawing.Point(159, 403);
            this.chatformSend.Name = "chatformSend";
            this.chatformSend.Size = new System.Drawing.Size(75, 23);
            this.chatformSend.TabIndex = 2;
            this.chatformSend.Text = "Send";
            this.chatformSend.UseVisualStyleBackColor = true;
            this.chatformSend.Click += new System.EventHandler(this.chatformSubmit_Click);
            // 
            // chatformClear
            // 
            this.chatformClear.Location = new System.Drawing.Point(306, 42);
            this.chatformClear.Name = "chatformClear";
            this.chatformClear.Size = new System.Drawing.Size(75, 23);
            this.chatformClear.TabIndex = 3;
            this.chatformClear.Text = "Clear";
            this.chatformClear.UseVisualStyleBackColor = true;
            this.chatformClear.Click += new System.EventHandler(this.chatformClear_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Chatting with ";
            // 
            // chatformChatWith
            // 
            this.chatformChatWith.Location = new System.Drawing.Point(79, 42);
            this.chatformChatWith.Name = "chatformChatWith";
            this.chatformChatWith.Size = new System.Drawing.Size(111, 13);
            this.chatformChatWith.TabIndex = 5;
            // 
            // chatformList
            // 
            this.chatformList.FormattingEnabled = true;
            this.chatformList.HorizontalScrollbar = true;
            this.chatformList.Location = new System.Drawing.Point(15, 78);
            this.chatformList.Name = "chatformList";
            this.chatformList.Size = new System.Drawing.Size(366, 251);
            this.chatformList.TabIndex = 6;
            this.chatformList.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.chatformText_KeyPress);
            // 
            // chatformStatus
            // 
            this.chatformStatus.Location = new System.Drawing.Point(12, 62);
            this.chatformStatus.Name = "chatformStatus";
            this.chatformStatus.Size = new System.Drawing.Size(157, 13);
            this.chatformStatus.TabIndex = 7;
            this.chatformStatus.Text = "Not connected.";
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 456);
            this.Controls.Add(this.chatformStatus);
            this.Controls.Add(this.chatformList);
            this.Controls.Add(this.chatformChatWith);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chatformClear);
            this.Controls.Add(this.chatformSend);
            this.Controls.Add(this.chatformText);
            this.Name = "ChatForm";
            this.Text = "ChatForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChatForm_FormClosing);
            this.Load += new System.EventHandler(this.ChatForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox chatformText;
        private System.Windows.Forms.Button chatformSend;
        private System.Windows.Forms.Button chatformClear;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label chatformChatWith;
        private System.Windows.Forms.ListBox chatformList;
        private System.Windows.Forms.Label chatformStatus;
    }
}