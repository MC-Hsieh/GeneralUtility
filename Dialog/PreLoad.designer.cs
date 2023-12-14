namespace GeneralUtility.Dialog
{
    partial class XPreLoadViewer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XPreLoadViewer));
            this.labStatus = new System.Windows.Forms.Label();
            this.labTittle = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlBackground = new System.Windows.Forms.Panel();
            this.picNowStep = new System.Windows.Forms.PictureBox();
            this.labVersion = new System.Windows.Forms.Label();
            this.tmrAnimation = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnlBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picNowStep)).BeginInit();
            this.SuspendLayout();
            // 
            // labStatus
            // 
            this.labStatus.BackColor = System.Drawing.Color.Transparent;
            this.labStatus.Font = new System.Drawing.Font("Microsoft JhengHei", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.labStatus.Location = new System.Drawing.Point(12, 207);
            this.labStatus.Name = "labStatus";
            this.labStatus.Size = new System.Drawing.Size(400, 28);
            this.labStatus.TabIndex = 3;
            this.labStatus.Text = "初始化中 ...";
            this.labStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labTittle
            // 
            this.labTittle.BackColor = System.Drawing.Color.Transparent;
            this.labTittle.Font = new System.Drawing.Font("Microsoft JhengHei", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labTittle.ForeColor = System.Drawing.Color.Black;
            this.labTittle.Location = new System.Drawing.Point(12, 140);
            this.labTittle.Name = "labTittle";
            this.labTittle.Size = new System.Drawing.Size(401, 35);
            this.labTittle.TabIndex = 4;
            this.labTittle.Text = "Progaram Name";
            this.labTittle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(401, 114);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // pnlBackground
            // 
            this.pnlBackground.BackColor = System.Drawing.Color.White;
            this.pnlBackground.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlBackground.BackgroundImage")));
            this.pnlBackground.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlBackground.Controls.Add(this.picNowStep);
            this.pnlBackground.Controls.Add(this.labVersion);
            this.pnlBackground.Controls.Add(this.pictureBox1);
            this.pnlBackground.Controls.Add(this.labTittle);
            this.pnlBackground.Controls.Add(this.labStatus);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(425, 250);
            this.pnlBackground.TabIndex = 6;
            // 
            // picNowStep
            // 
            this.picNowStep.BackColor = System.Drawing.Color.Gainsboro;
            this.picNowStep.Location = new System.Drawing.Point(0, 252);
            this.picNowStep.Margin = new System.Windows.Forms.Padding(0);
            this.picNowStep.Name = "picNowStep";
            this.picNowStep.Size = new System.Drawing.Size(423, 13);
            this.picNowStep.TabIndex = 7;
            this.picNowStep.TabStop = false;
            this.picNowStep.Visible = false;
            // 
            // labVersion
            // 
            this.labVersion.BackColor = System.Drawing.Color.Transparent;
            this.labVersion.Font = new System.Drawing.Font("Microsoft JhengHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.labVersion.Location = new System.Drawing.Point(12, 177);
            this.labVersion.Name = "labVersion";
            this.labVersion.Size = new System.Drawing.Size(401, 28);
            this.labVersion.TabIndex = 6;
            this.labVersion.Text = "Ver. 1.0.0.1";
            this.labVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tmrAnimation
            // 
            this.tmrAnimation.Tick += new System.EventHandler(this.tmrAnimation_Tick);
            // 
            // XPreLoadViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(425, 250);
            this.Controls.Add(this.pnlBackground);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(425, 250);
            this.MinimumSize = new System.Drawing.Size(425, 250);
            this.Name = "XPreLoadViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormPreLoad";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.XPreLoadViewer_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnlBackground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picNowStep)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

		public System.Windows.Forms.Label labStatus;
		public System.Windows.Forms.Label labTittle;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlBackground;
        public System.Windows.Forms.Label labVersion;
		private System.Windows.Forms.Timer tmrAnimation;
        private System.Windows.Forms.PictureBox picNowStep;
    }
}