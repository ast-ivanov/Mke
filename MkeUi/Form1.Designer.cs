namespace MkeUi
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
            this.topEdgeGroup = new System.Windows.Forms.GroupBox();
            this.thirdTopEdge = new System.Windows.Forms.CheckBox();
            this.secondTopEdge = new System.Windows.Forms.CheckBox();
            this.firstTopEdge = new System.Windows.Forms.CheckBox();
            this.leftEdgeGroup = new System.Windows.Forms.GroupBox();
            this.thirdLeftEdge = new System.Windows.Forms.CheckBox();
            this.secondLeftEdge = new System.Windows.Forms.CheckBox();
            this.firstLeftEdge = new System.Windows.Forms.CheckBox();
            this.bottomEdgeGroup = new System.Windows.Forms.GroupBox();
            this.thirdBottomEdge = new System.Windows.Forms.CheckBox();
            this.secondBottomEdge = new System.Windows.Forms.CheckBox();
            this.firstBottomEdge = new System.Windows.Forms.CheckBox();
            this.rightEdgeGroup = new System.Windows.Forms.GroupBox();
            this.thirdRightEdge = new System.Windows.Forms.CheckBox();
            this.secondRightEdge = new System.Windows.Forms.CheckBox();
            this.firstRightEdge = new System.Windows.Forms.CheckBox();
            this.calculateBtn = new System.Windows.Forms.Button();
            this.topEdgeGroup.SuspendLayout();
            this.leftEdgeGroup.SuspendLayout();
            this.bottomEdgeGroup.SuspendLayout();
            this.rightEdgeGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // topEdgeGroup
            // 
            this.topEdgeGroup.Controls.Add(this.thirdTopEdge);
            this.topEdgeGroup.Controls.Add(this.secondTopEdge);
            this.topEdgeGroup.Controls.Add(this.firstTopEdge);
            this.topEdgeGroup.Location = new System.Drawing.Point(251, 12);
            this.topEdgeGroup.Name = "topEdgeGroup";
            this.topEdgeGroup.Size = new System.Drawing.Size(121, 100);
            this.topEdgeGroup.TabIndex = 0;
            this.topEdgeGroup.TabStop = false;
            this.topEdgeGroup.Text = "Верхняя граница";
            // 
            // thirdTopEdge
            // 
            this.thirdTopEdge.AutoSize = true;
            this.thirdTopEdge.Location = new System.Drawing.Point(7, 68);
            this.thirdTopEdge.Name = "thirdTopEdge";
            this.thirdTopEdge.Size = new System.Drawing.Size(62, 17);
            this.thirdTopEdge.TabIndex = 2;
            this.thirdTopEdge.Text = "Третьи";
            this.thirdTopEdge.UseVisualStyleBackColor = true;
            this.thirdTopEdge.CheckedChanged += new System.EventHandler(this.EdgeChanged);
            // 
            // secondTopEdge
            // 
            this.secondTopEdge.AutoSize = true;
            this.secondTopEdge.Location = new System.Drawing.Point(7, 44);
            this.secondTopEdge.Name = "secondTopEdge";
            this.secondTopEdge.Size = new System.Drawing.Size(64, 17);
            this.secondTopEdge.TabIndex = 1;
            this.secondTopEdge.Text = "Вторые";
            this.secondTopEdge.UseVisualStyleBackColor = true;
            this.secondTopEdge.CheckedChanged += new System.EventHandler(this.EdgeChanged);
            // 
            // firstTopEdge
            // 
            this.firstTopEdge.AutoSize = true;
            this.firstTopEdge.Location = new System.Drawing.Point(7, 20);
            this.firstTopEdge.Name = "firstTopEdge";
            this.firstTopEdge.Size = new System.Drawing.Size(66, 17);
            this.firstTopEdge.TabIndex = 0;
            this.firstTopEdge.Text = "Первые";
            this.firstTopEdge.UseVisualStyleBackColor = true;
            this.firstTopEdge.CheckedChanged += new System.EventHandler(this.EdgeChanged);
            // 
            // leftEdgeGroup
            // 
            this.leftEdgeGroup.Controls.Add(this.thirdLeftEdge);
            this.leftEdgeGroup.Controls.Add(this.secondLeftEdge);
            this.leftEdgeGroup.Controls.Add(this.firstLeftEdge);
            this.leftEdgeGroup.Location = new System.Drawing.Point(12, 140);
            this.leftEdgeGroup.Name = "leftEdgeGroup";
            this.leftEdgeGroup.Size = new System.Drawing.Size(121, 100);
            this.leftEdgeGroup.TabIndex = 1;
            this.leftEdgeGroup.TabStop = false;
            this.leftEdgeGroup.Text = "Левая граница";
            // 
            // thirdLeftEdge
            // 
            this.thirdLeftEdge.AutoSize = true;
            this.thirdLeftEdge.Location = new System.Drawing.Point(7, 68);
            this.thirdLeftEdge.Name = "thirdLeftEdge";
            this.thirdLeftEdge.Size = new System.Drawing.Size(62, 17);
            this.thirdLeftEdge.TabIndex = 2;
            this.thirdLeftEdge.Text = "Третьи";
            this.thirdLeftEdge.UseVisualStyleBackColor = true;
            this.thirdLeftEdge.CheckedChanged += new System.EventHandler(this.EdgeChanged);
            // 
            // secondLeftEdge
            // 
            this.secondLeftEdge.AutoSize = true;
            this.secondLeftEdge.Location = new System.Drawing.Point(7, 44);
            this.secondLeftEdge.Name = "secondLeftEdge";
            this.secondLeftEdge.Size = new System.Drawing.Size(64, 17);
            this.secondLeftEdge.TabIndex = 1;
            this.secondLeftEdge.Text = "Вторые";
            this.secondLeftEdge.UseVisualStyleBackColor = true;
            this.secondLeftEdge.CheckedChanged += new System.EventHandler(this.EdgeChanged);
            // 
            // firstLeftEdge
            // 
            this.firstLeftEdge.AutoSize = true;
            this.firstLeftEdge.Location = new System.Drawing.Point(7, 20);
            this.firstLeftEdge.Name = "firstLeftEdge";
            this.firstLeftEdge.Size = new System.Drawing.Size(66, 17);
            this.firstLeftEdge.TabIndex = 0;
            this.firstLeftEdge.Text = "Первые";
            this.firstLeftEdge.UseVisualStyleBackColor = true;
            this.firstLeftEdge.CheckedChanged += new System.EventHandler(this.EdgeChanged);
            // 
            // bottomEdgeGroup
            // 
            this.bottomEdgeGroup.Controls.Add(this.thirdBottomEdge);
            this.bottomEdgeGroup.Controls.Add(this.secondBottomEdge);
            this.bottomEdgeGroup.Controls.Add(this.firstBottomEdge);
            this.bottomEdgeGroup.Location = new System.Drawing.Point(251, 335);
            this.bottomEdgeGroup.Name = "bottomEdgeGroup";
            this.bottomEdgeGroup.Size = new System.Drawing.Size(121, 100);
            this.bottomEdgeGroup.TabIndex = 3;
            this.bottomEdgeGroup.TabStop = false;
            this.bottomEdgeGroup.Text = "Нижняя граница";
            // 
            // thirdBottomEdge
            // 
            this.thirdBottomEdge.AutoSize = true;
            this.thirdBottomEdge.Location = new System.Drawing.Point(7, 68);
            this.thirdBottomEdge.Name = "thirdBottomEdge";
            this.thirdBottomEdge.Size = new System.Drawing.Size(62, 17);
            this.thirdBottomEdge.TabIndex = 2;
            this.thirdBottomEdge.Text = "Третьи";
            this.thirdBottomEdge.UseVisualStyleBackColor = true;
            // 
            // secondBottomEdge
            // 
            this.secondBottomEdge.AutoSize = true;
            this.secondBottomEdge.Location = new System.Drawing.Point(7, 44);
            this.secondBottomEdge.Name = "secondBottomEdge";
            this.secondBottomEdge.Size = new System.Drawing.Size(64, 17);
            this.secondBottomEdge.TabIndex = 1;
            this.secondBottomEdge.Text = "Вторые";
            this.secondBottomEdge.UseVisualStyleBackColor = true;
            // 
            // firstBottomEdge
            // 
            this.firstBottomEdge.AutoSize = true;
            this.firstBottomEdge.Location = new System.Drawing.Point(7, 20);
            this.firstBottomEdge.Name = "firstBottomEdge";
            this.firstBottomEdge.Size = new System.Drawing.Size(66, 17);
            this.firstBottomEdge.TabIndex = 0;
            this.firstBottomEdge.Text = "Первые";
            this.firstBottomEdge.UseVisualStyleBackColor = true;
            // 
            // rightEdgeGroup
            // 
            this.rightEdgeGroup.Controls.Add(this.thirdRightEdge);
            this.rightEdgeGroup.Controls.Add(this.secondRightEdge);
            this.rightEdgeGroup.Controls.Add(this.firstRightEdge);
            this.rightEdgeGroup.Location = new System.Drawing.Point(489, 140);
            this.rightEdgeGroup.Name = "rightEdgeGroup";
            this.rightEdgeGroup.Size = new System.Drawing.Size(121, 100);
            this.rightEdgeGroup.TabIndex = 3;
            this.rightEdgeGroup.TabStop = false;
            this.rightEdgeGroup.Text = "Правая граница";
            // 
            // thirdRightEdge
            // 
            this.thirdRightEdge.AutoSize = true;
            this.thirdRightEdge.Location = new System.Drawing.Point(7, 68);
            this.thirdRightEdge.Name = "thirdRightEdge";
            this.thirdRightEdge.Size = new System.Drawing.Size(62, 17);
            this.thirdRightEdge.TabIndex = 2;
            this.thirdRightEdge.Text = "Третьи";
            this.thirdRightEdge.UseVisualStyleBackColor = true;
            this.thirdRightEdge.CheckedChanged += new System.EventHandler(this.EdgeChanged);
            // 
            // secondRightEdge
            // 
            this.secondRightEdge.AutoSize = true;
            this.secondRightEdge.Location = new System.Drawing.Point(7, 44);
            this.secondRightEdge.Name = "secondRightEdge";
            this.secondRightEdge.Size = new System.Drawing.Size(64, 17);
            this.secondRightEdge.TabIndex = 1;
            this.secondRightEdge.Text = "Вторые";
            this.secondRightEdge.UseVisualStyleBackColor = true;
            this.secondRightEdge.CheckedChanged += new System.EventHandler(this.EdgeChanged);
            // 
            // firstRightEdge
            // 
            this.firstRightEdge.AutoSize = true;
            this.firstRightEdge.Location = new System.Drawing.Point(7, 20);
            this.firstRightEdge.Name = "firstRightEdge";
            this.firstRightEdge.Size = new System.Drawing.Size(66, 17);
            this.firstRightEdge.TabIndex = 0;
            this.firstRightEdge.Text = "Первые";
            this.firstRightEdge.UseVisualStyleBackColor = true;
            this.firstRightEdge.CheckedChanged += new System.EventHandler(this.EdgeChanged);
            // 
            // calculateBtn
            // 
            this.calculateBtn.Location = new System.Drawing.Point(274, 169);
            this.calculateBtn.Name = "calculateBtn";
            this.calculateBtn.Size = new System.Drawing.Size(75, 45);
            this.calculateBtn.TabIndex = 4;
            this.calculateBtn.Text = "Рассчитать";
            this.calculateBtn.UseVisualStyleBackColor = true;
            this.calculateBtn.Click += new System.EventHandler(this.calculateBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 447);
            this.Controls.Add(this.calculateBtn);
            this.Controls.Add(this.rightEdgeGroup);
            this.Controls.Add(this.bottomEdgeGroup);
            this.Controls.Add(this.leftEdgeGroup);
            this.Controls.Add(this.topEdgeGroup);
            this.MaximumSize = new System.Drawing.Size(638, 486);
            this.MinimumSize = new System.Drawing.Size(638, 486);
            this.Name = "Form1";
            this.Text = "МКЭ RZ";
            this.topEdgeGroup.ResumeLayout(false);
            this.topEdgeGroup.PerformLayout();
            this.leftEdgeGroup.ResumeLayout(false);
            this.leftEdgeGroup.PerformLayout();
            this.bottomEdgeGroup.ResumeLayout(false);
            this.bottomEdgeGroup.PerformLayout();
            this.rightEdgeGroup.ResumeLayout(false);
            this.rightEdgeGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox topEdgeGroup;
        private System.Windows.Forms.CheckBox thirdTopEdge;
        private System.Windows.Forms.CheckBox secondTopEdge;
        private System.Windows.Forms.CheckBox firstTopEdge;
        private System.Windows.Forms.GroupBox leftEdgeGroup;
        private System.Windows.Forms.CheckBox thirdLeftEdge;
        private System.Windows.Forms.CheckBox secondLeftEdge;
        private System.Windows.Forms.CheckBox firstLeftEdge;
        private System.Windows.Forms.GroupBox bottomEdgeGroup;
        private System.Windows.Forms.CheckBox thirdBottomEdge;
        private System.Windows.Forms.CheckBox secondBottomEdge;
        private System.Windows.Forms.CheckBox firstBottomEdge;
        private System.Windows.Forms.GroupBox rightEdgeGroup;
        private System.Windows.Forms.CheckBox thirdRightEdge;
        private System.Windows.Forms.CheckBox secondRightEdge;
        private System.Windows.Forms.CheckBox firstRightEdge;
        private System.Windows.Forms.Button calculateBtn;
    }
}

