namespace TestDotNetLib
{
    partial class Form1
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
            this.btnGetDevices = new System.Windows.Forms.Button();
            this.btnSetSocket = new System.Windows.Forms.Button();
            this.tBsocket = new System.Windows.Forms.NumericUpDown();
            this.LogBox = new System.Windows.Forms.RichTextBox();
            this.btnSocketOn = new System.Windows.Forms.Button();
            this.btnSocketOff = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tBsocket)).BeginInit();
            this.SuspendLayout();
            // 
            // btnGetDevices
            // 
            this.btnGetDevices.Location = new System.Drawing.Point(12, 12);
            this.btnGetDevices.Name = "btnGetDevices";
            this.btnGetDevices.Size = new System.Drawing.Size(133, 23);
            this.btnGetDevices.TabIndex = 0;
            this.btnGetDevices.Text = "getDevices";
            this.btnGetDevices.UseVisualStyleBackColor = true;
            this.btnGetDevices.Click += new System.EventHandler(this.btnGetDevices_Click);
            // 
            // btnSetSocket
            // 
            this.btnSetSocket.Location = new System.Drawing.Point(12, 107);
            this.btnSetSocket.Name = "btnSetSocket";
            this.btnSetSocket.Size = new System.Drawing.Size(133, 23);
            this.btnSetSocket.TabIndex = 1;
            this.btnSetSocket.Text = "toggle Socket";
            this.btnSetSocket.UseVisualStyleBackColor = true;
            this.btnSetSocket.Click += new System.EventHandler(this.btnSetSocket_Click);
            // 
            // tBsocket
            // 
            this.tBsocket.Location = new System.Drawing.Point(86, 81);
            this.tBsocket.Name = "tBsocket";
            this.tBsocket.Size = new System.Drawing.Size(59, 20);
            this.tBsocket.TabIndex = 2;
            // 
            // LogBox
            // 
            this.LogBox.Location = new System.Drawing.Point(200, 14);
            this.LogBox.Name = "LogBox";
            this.LogBox.Size = new System.Drawing.Size(478, 174);
            this.LogBox.TabIndex = 3;
            this.LogBox.Text = "";
            // 
            // btnSocketOn
            // 
            this.btnSocketOn.Location = new System.Drawing.Point(12, 136);
            this.btnSocketOn.Name = "btnSocketOn";
            this.btnSocketOn.Size = new System.Drawing.Size(133, 23);
            this.btnSocketOn.TabIndex = 4;
            this.btnSocketOn.Text = "Socket ON";
            this.btnSocketOn.UseVisualStyleBackColor = true;
            this.btnSocketOn.Click += new System.EventHandler(this.btnSocketOn_Click);
            // 
            // btnSocketOff
            // 
            this.btnSocketOff.Location = new System.Drawing.Point(12, 165);
            this.btnSocketOff.Name = "btnSocketOff";
            this.btnSocketOff.Size = new System.Drawing.Size(133, 23);
            this.btnSocketOff.TabIndex = 5;
            this.btnSocketOff.Text = "Socket OFF";
            this.btnSocketOff.UseVisualStyleBackColor = true;
            this.btnSocketOff.Click += new System.EventHandler(this.btnSocketOff_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Socket";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 207);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSocketOff);
            this.Controls.Add(this.btnSocketOn);
            this.Controls.Add(this.LogBox);
            this.Controls.Add(this.tBsocket);
            this.Controls.Add(this.btnSetSocket);
            this.Controls.Add(this.btnGetDevices);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.tBsocket)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGetDevices;
        private System.Windows.Forms.Button btnSetSocket;
        private System.Windows.Forms.NumericUpDown tBsocket;
        private System.Windows.Forms.RichTextBox LogBox;
        private System.Windows.Forms.Button btnSocketOn;
        private System.Windows.Forms.Button btnSocketOff;
        private System.Windows.Forms.Label label1;
    }
}