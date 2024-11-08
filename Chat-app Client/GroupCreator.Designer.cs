namespace Chat_app_Client
{
    partial class GroupCreator
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
            label1 = new Label();
            txtGroupName = new TextBox();
            label2 = new Label();
            label3 = new Label();
            txtMembers = new TextBox();
            btnCreate = new Button();
            btnExit = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Times New Roman", 14.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            label1.ForeColor = Color.Maroon;
            label1.Location = new Point(14, 12);
            label1.Name = "label1";
            label1.Size = new Size(147, 28);
            label1.TabIndex = 3;
            label1.Text = "Create Group";
            // 
            // txtGroupName
            // 
            txtGroupName.Font = new Font("Times New Roman", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            txtGroupName.Location = new Point(14, 75);
            txtGroupName.Margin = new Padding(3, 4, 3, 4);
            txtGroupName.Name = "txtGroupName";
            txtGroupName.Size = new Size(214, 26);
            txtGroupName.TabIndex = 0;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Times New Roman", 9.75F, FontStyle.Italic, GraphicsUnit.Point);
            label2.Location = new Point(14, 51);
            label2.Name = "label2";
            label2.Size = new Size(98, 19);
            label2.TabIndex = 4;
            label2.Text = "Group Name";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Times New Roman", 9.75F, FontStyle.Italic, GraphicsUnit.Point);
            label3.Location = new Point(14, 109);
            label3.Name = "label3";
            label3.Size = new Size(74, 19);
            label3.TabIndex = 5;
            label3.Text = "Members";
            // 
            // txtMembers
            // 
            txtMembers.Font = new Font("Times New Roman", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            txtMembers.Location = new Point(14, 133);
            txtMembers.Margin = new Padding(3, 4, 3, 4);
            txtMembers.Multiline = true;
            txtMembers.Name = "txtMembers";
            txtMembers.Size = new Size(214, 97);
            txtMembers.TabIndex = 1;
            // 
            // btnCreate
            // 
            btnCreate.BackColor = Color.RosyBrown;
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Font = new Font("Times New Roman", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnCreate.ForeColor = Color.Maroon;
            btnCreate.Location = new Point(12, 248);
            btnCreate.Margin = new Padding(3, 4, 3, 4);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(102, 36);
            btnCreate.TabIndex = 2;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = false;
            btnCreate.Click += btnCreate_Click;
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.RosyBrown;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.Font = new Font("Times New Roman", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnExit.ForeColor = Color.Maroon;
            btnExit.Location = new Point(136, 248);
            btnExit.Margin = new Padding(3, 4, 3, 4);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(102, 36);
            btnExit.TabIndex = 6;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += btnExit_Click;
            // 
            // GroupCreator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(296, 350);
            Controls.Add(btnExit);
            Controls.Add(btnCreate);
            Controls.Add(txtMembers);
            Controls.Add(label3);
            Controls.Add(txtGroupName);
            Controls.Add(label2);
            Controls.Add(label1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "GroupCreator";
            Text = "Create New Group";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtGroupName;
        private Label label2;
        private Label label3;
        private TextBox txtMembers;
        private Button btnCreate;
        private Button btnExit;
    }
}