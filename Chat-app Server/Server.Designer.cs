namespace Chat_app_Server
{
    partial class Server
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            btnStart = new Button();
            rtbDialog = new RichTextBox();
            btnStop = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Times New Roman", 20.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            label1.ForeColor = Color.Maroon;
            label1.Location = new Point(24, 20);
            label1.Name = "label1";
            label1.Size = new Size(246, 39);
            label1.TabIndex = 0;
            label1.Text = "Server Chat-app";
            // 
            // btnStart
            // 
            btnStart.BackColor = Color.RosyBrown;
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.Font = new Font("Times New Roman", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            btnStart.ForeColor = Color.Maroon;
            btnStart.Location = new Point(12, 81);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(306, 32);
            btnStart.TabIndex = 0;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // rtbDialog
            // 
            rtbDialog.Enabled = false;
            rtbDialog.Location = new Point(12, 141);
            rtbDialog.Name = "rtbDialog";
            rtbDialog.Size = new Size(306, 248);
            rtbDialog.TabIndex = 4;
            rtbDialog.Text = "";
            // 
            // btnStop
            // 
            btnStop.BackColor = Color.Firebrick;
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.Font = new Font("Times New Roman", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            btnStop.ForeColor = Color.White;
            btnStop.Location = new Point(12, 406);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(306, 32);
            btnStop.TabIndex = 3;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = false;
            btnStop.Click += btnStop_Click;
            // 
            // Server
            // 
            BackColor = SystemColors.Control;
            ClientSize = new Size(361, 533);
            Controls.Add(rtbDialog);
            Controls.Add(btnStop);
            Controls.Add(btnStart);
            Controls.Add(label1);
            Name = "Server";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Server";
            Load += Server_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button btnStart;
        private RichTextBox rtbDialog;
        private Button btnStop;
    }
}