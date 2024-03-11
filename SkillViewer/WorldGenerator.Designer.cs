using WizardMonks.Character;

namespace SkillViewer
{
    partial class WorldGenerator
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
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lstMembers = new System.Windows.Forms.ListBox();
            this.characterBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnAdvance = new System.Windows.Forms.Button();
            this.lstAdvance = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.characterBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(13, 12);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(119, 23);
            this.btnGenerate.TabIndex = 0;
            this.btnGenerate.Text = "Generate New Order";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // lstMembers
            // 
            this.lstMembers.DataSource = this.characterBindingSource;
            this.lstMembers.DisplayMember = "Name";
            this.lstMembers.FormattingEnabled = true;
            this.lstMembers.Location = new System.Drawing.Point(13, 43);
            this.lstMembers.Name = "lstMembers";
            this.lstMembers.Size = new System.Drawing.Size(256, 277);
            this.lstMembers.TabIndex = 1;
            this.lstMembers.DoubleClick += new System.EventHandler(this.lstMembers_DoubleClick);
            // 
            // characterBindingSource
            // 
            this.characterBindingSource.DataSource = typeof(Character);
            // 
            // btnAdvance
            // 
            this.btnAdvance.Location = new System.Drawing.Point(412, 12);
            this.btnAdvance.Name = "btnAdvance";
            this.btnAdvance.Size = new System.Drawing.Size(119, 23);
            this.btnAdvance.TabIndex = 2;
            this.btnAdvance.Text = "Advance Season";
            this.btnAdvance.UseVisualStyleBackColor = true;
            this.btnAdvance.Click += new System.EventHandler(this.btnAdvance_Click);
            // 
            // lstAdvance
            // 
            this.lstAdvance.DataSource = this.characterBindingSource;
            this.lstAdvance.DisplayMember = "Name";
            this.lstAdvance.FormattingEnabled = true;
            this.lstAdvance.Location = new System.Drawing.Point(275, 43);
            this.lstAdvance.Name = "lstAdvance";
            this.lstAdvance.Size = new System.Drawing.Size(256, 277);
            this.lstAdvance.TabIndex = 3;
            // 
            // WorldGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(548, 337);
            this.Controls.Add(this.lstAdvance);
            this.Controls.Add(this.btnAdvance);
            this.Controls.Add(this.lstMembers);
            this.Controls.Add(this.btnGenerate);
            this.Name = "WorldGenerator";
            this.Text = "WorldGenerator";
            ((System.ComponentModel.ISupportInitialize)(this.characterBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ListBox lstMembers;
        private System.Windows.Forms.BindingSource characterBindingSource;
        private System.Windows.Forms.Button btnAdvance;
        private System.Windows.Forms.ListBox lstAdvance;
    }
}