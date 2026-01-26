namespace Tamagochi
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gameTimer = new System.Windows.Forms.Timer(this.components);
            this.renderTimer = new System.Windows.Forms.Timer(this.components);
            
            this.pbPet = new System.Windows.Forms.PictureBox();
            
            this.lblHunger = new System.Windows.Forms.Label();
            this.progHunger = new System.Windows.Forms.ProgressBar();
            
            this.lblHygiene = new System.Windows.Forms.Label();
            this.progHygiene = new System.Windows.Forms.ProgressBar();
            
            this.lblHappiness = new System.Windows.Forms.Label();
            this.progHappiness = new System.Windows.Forms.ProgressBar();
            
            this.lblEnergy = new System.Windows.Forms.Label();
            this.progEnergy = new System.Windows.Forms.ProgressBar();
            
            this.lblHealth = new System.Windows.Forms.Label();
            this.progHealth = new System.Windows.Forms.ProgressBar();
            
            this.btnFeed = new System.Windows.Forms.Button();
            this.btnClean = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnHeal = new System.Windows.Forms.Button();
            this.btnSleep = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            
            this.lblStatus = new System.Windows.Forms.Label();
            this.cmbPetType = new System.Windows.Forms.ComboBox();
            this.lblType = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.pbPet)).BeginInit();
            this.SuspendLayout();
            
            // 
            // gameTimer
            // 
            this.gameTimer.Interval = 1000;
            this.gameTimer.Tick += new System.EventHandler(this.gameTimer_Tick);
            
            // 
            // renderTimer
            // 
            this.renderTimer.Interval = 100; // 10 FPS animation
            this.renderTimer.Tick += new System.EventHandler(this.renderTimer_Tick);

            // 
            // pbPet
            // 
            this.pbPet.Location = new System.Drawing.Point(12, 50);
            this.pbPet.Name = "pbPet";
            this.pbPet.Size = new System.Drawing.Size(460, 300);
            this.pbPet.TabIndex = 0;
            this.pbPet.TabStop = false;
            this.pbPet.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pbPet.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

            // Stats Layout Variables
            int startY = 370;
            int gapY = 30;
            int labelX = 20;
            int progX = 100;
            int progW = 350;
            int progH = 20;

            // Hunger
            this.lblHunger.Text = "Hunger:";
            this.lblHunger.Location = new System.Drawing.Point(labelX, startY);
            this.progHunger.Location = new System.Drawing.Point(progX, startY);
            this.progHunger.Size = new System.Drawing.Size(progW, progH);

            // Hygiene
            this.lblHygiene.Text = "Hygiene:";
            this.lblHygiene.Location = new System.Drawing.Point(labelX, startY + gapY);
            this.progHygiene.Location = new System.Drawing.Point(progX, startY + gapY);
            this.progHygiene.Size = new System.Drawing.Size(progW, progH);

            // Happiness
            this.lblHappiness.Text = "Happiness:";
            this.lblHappiness.Location = new System.Drawing.Point(labelX, startY + gapY * 2);
            this.progHappiness.Location = new System.Drawing.Point(progX, startY + gapY * 2);
            this.progHappiness.Size = new System.Drawing.Size(progW, progH);

            // Energy
            this.lblEnergy.Text = "Energy:";
            this.lblEnergy.Location = new System.Drawing.Point(labelX, startY + gapY * 3);
            this.progEnergy.Location = new System.Drawing.Point(progX, startY + gapY * 3);
            this.progEnergy.Size = new System.Drawing.Size(progW, progH);
            
            // Health
            this.lblHealth.Text = "Health:";
            this.lblHealth.Location = new System.Drawing.Point(labelX, startY + gapY * 4);
            this.progHealth.Location = new System.Drawing.Point(progX, startY + gapY * 4);
            this.progHealth.Size = new System.Drawing.Size(progW, progH);

            // Buttons
            int btnY = startY + gapY * 5 + 10;
            int btnW = 70;
            int btnH = 40;
            int btnGap = 5;

            this.btnFeed.Text = "Feed";
            this.btnFeed.Location = new System.Drawing.Point(20, btnY);
            this.btnFeed.Size = new System.Drawing.Size(btnW, btnH);
            this.btnFeed.Click += new System.EventHandler(this.btnFeed_Click);

            this.btnClean.Text = "Clean";
            this.btnClean.Location = new System.Drawing.Point(20 + (btnW + btnGap), btnY);
            this.btnClean.Size = new System.Drawing.Size(btnW, btnH);
            this.btnClean.Click += new System.EventHandler(this.btnClean_Click);

            this.btnPlay.Text = "Play";
            this.btnPlay.Location = new System.Drawing.Point(20 + (btnW + btnGap)*2, btnY);
            this.btnPlay.Size = new System.Drawing.Size(btnW, btnH);
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            
            this.btnSleep.Text = "Sleep";
            this.btnSleep.Location = new System.Drawing.Point(20 + (btnW + btnGap)*3, btnY);
            this.btnSleep.Size = new System.Drawing.Size(btnW, btnH);
            this.btnSleep.Click += new System.EventHandler(this.btnSleep_Click);

            this.btnHeal.Text = "Heal";
            this.btnHeal.Location = new System.Drawing.Point(20 + (btnW + btnGap)*4, btnY);
            this.btnHeal.Size = new System.Drawing.Size(btnW, btnH);
            this.btnHeal.Click += new System.EventHandler(this.btnHeal_Click);
            
            this.btnReset.Text = "Reset";
            this.btnReset.Location = new System.Drawing.Point(400, btnY); // Far right
            this.btnReset.Size = new System.Drawing.Size(btnW, btnH);
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);

            // Top Panel
            this.lblType.Text = "Type:";
            this.lblType.Location = new System.Drawing.Point(12, 15);
            this.lblType.AutoSize = true;

            this.cmbPetType.Location = new System.Drawing.Point(60, 12);
            this.cmbPetType.Size = new System.Drawing.Size(100, 25);
            this.cmbPetType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPetType.SelectedIndexChanged += new System.EventHandler(this.cmbPetType_SelectedIndexChanged);

            this.lblStatus.Text = "Status: OK";
            this.lblStatus.Location = new System.Drawing.Point(200, 15);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 600);
            this.Text = "Tamagochi - By Jacky";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.Controls.Add(this.lblType);
            this.Controls.Add(this.cmbPetType);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pbPet);
            this.Controls.Add(this.lblHunger);
            this.Controls.Add(this.progHunger);
            this.Controls.Add(this.lblHygiene);
            this.Controls.Add(this.progHygiene);
            this.Controls.Add(this.lblHappiness);
            this.Controls.Add(this.progHappiness);
            this.Controls.Add(this.lblEnergy);
            this.Controls.Add(this.progEnergy);
            this.Controls.Add(this.lblHealth);
            this.Controls.Add(this.progHealth);
            this.Controls.Add(this.btnFeed);
            this.Controls.Add(this.btnClean);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.btnSleep);
            this.Controls.Add(this.btnHeal);
            this.Controls.Add(this.btnReset);

            ((System.ComponentModel.ISupportInitialize)(this.pbPet)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.Timer renderTimer;
        private System.Windows.Forms.PictureBox pbPet;
        private System.Windows.Forms.Label lblHunger;
        private System.Windows.Forms.ProgressBar progHunger;
        private System.Windows.Forms.Label lblHygiene;
        private System.Windows.Forms.ProgressBar progHygiene;
        private System.Windows.Forms.Label lblHappiness;
        private System.Windows.Forms.ProgressBar progHappiness;
        private System.Windows.Forms.Label lblEnergy;
        private System.Windows.Forms.ProgressBar progEnergy;
        private System.Windows.Forms.Label lblHealth;
        private System.Windows.Forms.ProgressBar progHealth;
        
        private System.Windows.Forms.Button btnFeed;
        private System.Windows.Forms.Button btnClean;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnHeal;
        private System.Windows.Forms.Button btnSleep;
        private System.Windows.Forms.Button btnReset;
        
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ComboBox cmbPetType;
        private System.Windows.Forms.Label lblType;
    }
}
