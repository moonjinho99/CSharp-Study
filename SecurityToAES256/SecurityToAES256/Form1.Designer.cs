
namespace SecurityToAES256
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.beforeText = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.securityBtn = new System.Windows.Forms.Button();
            this.loadBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // beforeText
            // 
            this.beforeText.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            this.beforeText.Location = new System.Drawing.Point(81, 100);
            this.beforeText.Name = "beforeText";
            this.beforeText.Size = new System.Drawing.Size(556, 38);
            this.beforeText.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(81, 183);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(556, 456);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // securityBtn
            // 
            this.securityBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.securityBtn.Location = new System.Drawing.Point(444, 657);
            this.securityBtn.Name = "securityBtn";
            this.securityBtn.Size = new System.Drawing.Size(193, 69);
            this.securityBtn.TabIndex = 4;
            this.securityBtn.Text = "암호화하여 전송";
            this.securityBtn.UseVisualStyleBackColor = true;
            this.securityBtn.Click += new System.EventHandler(this.securityBtn_Click);
            // 
            // loadBtn
            // 
            this.loadBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.loadBtn.Location = new System.Drawing.Point(81, 657);
            this.loadBtn.Name = "loadBtn";
            this.loadBtn.Size = new System.Drawing.Size(203, 69);
            this.loadBtn.TabIndex = 5;
            this.loadBtn.Text = "불러오기";
            this.loadBtn.UseVisualStyleBackColor = true;
            this.loadBtn.Click += new System.EventHandler(this.loadBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            this.label1.Location = new System.Drawing.Point(322, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 31);
            this.label1.TabIndex = 6;
            this.label1.Text = "원본";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 857);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.loadBtn);
            this.Controls.Add(this.securityBtn);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.beforeText);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox beforeText;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button securityBtn;
        private System.Windows.Forms.Button loadBtn;
        private System.Windows.Forms.Label label1;
    }
}

