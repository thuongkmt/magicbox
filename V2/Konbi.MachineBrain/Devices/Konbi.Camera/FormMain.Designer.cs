namespace Konbi.Camera
{
    partial class FormMain
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
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.imageButton = new System.Windows.Forms.Button();
            this.cbCamera = new System.Windows.Forms.ComboBox();
            this.cbCompress = new System.Windows.Forms.ComboBox();
            this.webCameraControl1 = new Konbi.Camera.WebCameraControl();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.startButton.Enabled = false;
            this.startButton.Location = new System.Drawing.Point(138, 279);
            this.startButton.Margin = new System.Windows.Forms.Padding(2);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 1;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.stopButton.Enabled = false;
            this.stopButton.Location = new System.Drawing.Point(222, 279);
            this.stopButton.Margin = new System.Windows.Forms.Padding(2);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(75, 23);
            this.stopButton.TabIndex = 2;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // imageButton
            // 
            this.imageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.imageButton.Enabled = false;
            this.imageButton.Location = new System.Drawing.Point(388, 279);
            this.imageButton.Margin = new System.Windows.Forms.Padding(2);
            this.imageButton.Name = "imageButton";
            this.imageButton.Size = new System.Drawing.Size(75, 23);
            this.imageButton.TabIndex = 3;
            this.imageButton.Text = "Image...";
            this.imageButton.UseVisualStyleBackColor = true;
            this.imageButton.Click += new System.EventHandler(this.imageButton_Click);
            // 
            // comboBox1
            // 
            this.cbCamera.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbCamera.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCamera.FormattingEnabled = true;
            this.cbCamera.Location = new System.Drawing.Point(8, 280);
            this.cbCamera.Name = "comboBox1";
            this.cbCamera.Size = new System.Drawing.Size(121, 21);
            this.cbCamera.TabIndex = 0;
            this.cbCamera.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // cbCompress
            // 
            this.cbCompress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbCompress.DisplayMember = "Name";
            this.cbCompress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCompress.FormattingEnabled = true;
            this.cbCompress.Location = new System.Drawing.Point(307, 280);
            this.cbCompress.Name = "cbCompress";
            this.cbCompress.Size = new System.Drawing.Size(68, 21);
            this.cbCompress.TabIndex = 5;
            this.cbCompress.ValueMember = "Value";
            // 
            // webCameraControl1
            // 
            this.webCameraControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webCameraControl1.Location = new System.Drawing.Point(8, 8);
            this.webCameraControl1.Margin = new System.Windows.Forms.Padding(2);
            this.webCameraControl1.Name = "webCameraControl1";
            this.webCameraControl1.Size = new System.Drawing.Size(460, 267);
            this.webCameraControl1.TabIndex = 4;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 304);
            this.Controls.Add(this.cbCompress);
            this.Controls.Add(this.cbCamera);
            this.Controls.Add(this.webCameraControl1);
            this.Controls.Add(this.imageButton);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(408, 285);
            this.Name = "FormMain";
            this.Text = "Camera";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button imageButton;
        private WebCameraControl webCameraControl1;
        private System.Windows.Forms.ComboBox cbCamera;
        private System.Windows.Forms.ComboBox cbCompress;
    }
}

