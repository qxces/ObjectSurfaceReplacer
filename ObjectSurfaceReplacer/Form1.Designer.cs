namespace ObjectSurfaceReplacer
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.ReplaceSurfaces_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ReplaceSurfaces_btn
            // 
            this.ReplaceSurfaces_btn.Location = new System.Drawing.Point(119, 237);
            this.ReplaceSurfaces_btn.Name = "ReplaceSurfaces_btn";
            this.ReplaceSurfaces_btn.Size = new System.Drawing.Size(75, 23);
            this.ReplaceSurfaces_btn.TabIndex = 0;
            this.ReplaceSurfaces_btn.Text = "Replace surfaces";
            this.ReplaceSurfaces_btn.UseVisualStyleBackColor = true;
            this.ReplaceSurfaces_btn.Click += new System.EventHandler(this.ReplaceSurfaces_btn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ReplaceSurfaces_btn);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ReplaceSurfaces_btn;
    }
}

