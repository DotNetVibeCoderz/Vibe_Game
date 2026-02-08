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
            components = new System.ComponentModel.Container();
            gameTimer = new System.Windows.Forms.Timer(components);
            renderTimer = new System.Windows.Forms.Timer(components);
            pbPet = new PictureBox();
            lblHunger = new Label();
            progHunger = new ProgressBar();
            lblHygiene = new Label();
            progHygiene = new ProgressBar();
            lblHappiness = new Label();
            progHappiness = new ProgressBar();
            lblEnergy = new Label();
            progEnergy = new ProgressBar();
            lblHealth = new Label();
            progHealth = new ProgressBar();
            btnFeed = new Button();
            btnClean = new Button();
            btnPlay = new Button();
            btnHeal = new Button();
            btnSleep = new Button();
            btnReset = new Button();
            lblStatus = new Label();
            cmbPetType = new ComboBox();
            lblType = new Label();
            ((System.ComponentModel.ISupportInitialize)pbPet).BeginInit();
            SuspendLayout();
            // 
            // gameTimer
            // 
            gameTimer.Interval = 1000;
            gameTimer.Tick += gameTimer_Tick;
            // 
            // renderTimer
            // 
            renderTimer.Tick += renderTimer_Tick;
            // 
            // pbPet
            // 
            pbPet.BackColor = Color.WhiteSmoke;
            pbPet.BorderStyle = BorderStyle.Fixed3D;
            pbPet.Location = new Point(12, 82);
            pbPet.Name = "pbPet";
            pbPet.Size = new Size(460, 300);
            pbPet.TabIndex = 0;
            pbPet.TabStop = false;
            // 
            // lblHunger
            // 
            lblHunger.Location = new Point(20, 370);
            lblHunger.Name = "lblHunger";
            lblHunger.Size = new Size(100, 23);
            lblHunger.TabIndex = 3;
            lblHunger.Text = "Hunger:";
            // 
            // progHunger
            // 
            progHunger.Location = new Point(100, 370);
            progHunger.Name = "progHunger";
            progHunger.Size = new Size(350, 20);
            progHunger.TabIndex = 4;
            // 
            // lblHygiene
            // 
            lblHygiene.Location = new Point(20, 370);
            lblHygiene.Name = "lblHygiene";
            lblHygiene.Size = new Size(100, 23);
            lblHygiene.TabIndex = 5;
            lblHygiene.Text = "Hygiene:";
            // 
            // progHygiene
            // 
            progHygiene.Location = new Point(100, 370);
            progHygiene.Name = "progHygiene";
            progHygiene.Size = new Size(350, 20);
            progHygiene.TabIndex = 6;
            // 
            // lblHappiness
            // 
            lblHappiness.Location = new Point(20, 370);
            lblHappiness.Name = "lblHappiness";
            lblHappiness.Size = new Size(100, 23);
            lblHappiness.TabIndex = 7;
            lblHappiness.Text = "Happiness:";
            // 
            // progHappiness
            // 
            progHappiness.Location = new Point(100, 370);
            progHappiness.Name = "progHappiness";
            progHappiness.Size = new Size(350, 20);
            progHappiness.TabIndex = 8;
            // 
            // lblEnergy
            // 
            lblEnergy.Location = new Point(20, 370);
            lblEnergy.Name = "lblEnergy";
            lblEnergy.Size = new Size(100, 23);
            lblEnergy.TabIndex = 9;
            lblEnergy.Text = "Energy:";
            // 
            // progEnergy
            // 
            progEnergy.Location = new Point(100, 370);
            progEnergy.Name = "progEnergy";
            progEnergy.Size = new Size(350, 20);
            progEnergy.TabIndex = 10;
            // 
            // lblHealth
            // 
            lblHealth.Location = new Point(20, 370);
            lblHealth.Name = "lblHealth";
            lblHealth.Size = new Size(100, 23);
            lblHealth.TabIndex = 11;
            lblHealth.Text = "Health:";
            // 
            // progHealth
            // 
            progHealth.Location = new Point(100, 370);
            progHealth.Name = "progHealth";
            progHealth.Size = new Size(350, 20);
            progHealth.TabIndex = 12;
            // 
            // btnFeed
            // 
            btnFeed.Location = new Point(20, 370);
            btnFeed.Name = "btnFeed";
            btnFeed.Size = new Size(70, 40);
            btnFeed.TabIndex = 13;
            btnFeed.Text = "Feed";
            btnFeed.Click += btnFeed_Click;
            // 
            // btnClean
            // 
            btnClean.Location = new Point(20, 370);
            btnClean.Name = "btnClean";
            btnClean.Size = new Size(70, 40);
            btnClean.TabIndex = 14;
            btnClean.Text = "Clean";
            btnClean.Click += btnClean_Click;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(20, 370);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(70, 40);
            btnPlay.TabIndex = 15;
            btnPlay.Text = "Play";
            btnPlay.Click += btnPlay_Click;
            // 
            // btnHeal
            // 
            btnHeal.Location = new Point(20, 370);
            btnHeal.Name = "btnHeal";
            btnHeal.Size = new Size(70, 40);
            btnHeal.TabIndex = 17;
            btnHeal.Text = "Heal";
            btnHeal.Click += btnHeal_Click;
            // 
            // btnSleep
            // 
            btnSleep.Location = new Point(20, 370);
            btnSleep.Name = "btnSleep";
            btnSleep.Size = new Size(70, 40);
            btnSleep.TabIndex = 16;
            btnSleep.Text = "Sleep";
            btnSleep.Click += btnSleep_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(400, 370);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(70, 40);
            btnReset.TabIndex = 18;
            btnReset.Text = "Reset";
            btnReset.Click += btnReset_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblStatus.Location = new Point(200, 15);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(87, 21);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Status: OK";
            // 
            // cmbPetType
            // 
            cmbPetType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPetType.Location = new Point(60, 12);
            cmbPetType.Name = "cmbPetType";
            cmbPetType.Size = new Size(100, 23);
            cmbPetType.TabIndex = 1;
            cmbPetType.SelectedIndexChanged += cmbPetType_SelectedIndexChanged;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new Point(12, 15);
            lblType.Name = "lblType";
            lblType.Size = new Size(35, 15);
            lblType.TabIndex = 0;
            lblType.Text = "Type:";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 600);
            Controls.Add(lblType);
            Controls.Add(cmbPetType);
            Controls.Add(lblStatus);
            Controls.Add(pbPet);
            Controls.Add(lblHunger);
            Controls.Add(progHunger);
            Controls.Add(lblHygiene);
            Controls.Add(progHygiene);
            Controls.Add(lblHappiness);
            Controls.Add(progHappiness);
            Controls.Add(lblEnergy);
            Controls.Add(progEnergy);
            Controls.Add(lblHealth);
            Controls.Add(progHealth);
            Controls.Add(btnFeed);
            Controls.Add(btnClean);
            Controls.Add(btnPlay);
            Controls.Add(btnSleep);
            Controls.Add(btnHeal);
            Controls.Add(btnReset);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Tamagochi - By Jacky";
            ((System.ComponentModel.ISupportInitialize)pbPet).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
