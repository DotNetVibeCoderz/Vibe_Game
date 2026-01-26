using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tamagochi
{
    public partial class MainForm : Form
    {
        private PetModel _pet;

        public MainForm()
        {
            InitializeComponent();
            
            // Populate Types
            cmbPetType.DataSource = Enum.GetValues(typeof(PetType));
            
            // Load or New
            _pet = PetModel.Load();
            
            // Sync UI to loaded data
            cmbPetType.SelectedItem = _pet.Type;
            
            gameTimer.Start();
            renderTimer.Start();
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _pet.Save();
            base.OnFormClosing(e);
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            _pet.Tick();
            UpdateStatsUI();
        }

        private void renderTimer_Tick(object sender, EventArgs e)
        {
            // Redraw the pet
            Bitmap bmp = PixelRenderer.DrawPet(_pet, pbPet.Width, pbPet.Height);
            if (pbPet.Image != null) pbPet.Image.Dispose();
            pbPet.Image = bmp;
            
            // Update Label status
            if (_pet.Stage == LifecycleStage.Dead)
            {
                lblStatus.Text = "Status: DEAD 💀";
                lblStatus.ForeColor = Color.Red;
            }
            else
            {
                lblStatus.Text = $"Status: {_pet.CurrentEmotion} ({_pet.Stage})";
                lblStatus.ForeColor = Color.Black;
            }
        }

        private void UpdateStatsUI()
        {
            progHunger.Value = Clamp(_pet.Hunger);
            progHygiene.Value = Clamp(_pet.Hygiene);
            progHappiness.Value = Clamp(_pet.Happiness);
            progEnergy.Value = Clamp(_pet.Energy);
            progHealth.Value = Clamp(_pet.Health);
            
            // Color coding progress bars (Requires Custom Painting or just simple logic)
            // WinForms standard progress bars are usually green/system color. 
            // We'll stick to standard for simplicity in this generated code.
        }

        private int Clamp(int val)
        {
            if (val < 0) return 0;
            if (val > 100) return 100;
            return val;
        }

        // Actions
        private void btnFeed_Click(object sender, EventArgs e)
        {
            _pet.Feed();
            UpdateStatsUI();
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            _pet.Clean();
            UpdateStatsUI();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            _pet.Play();
            UpdateStatsUI();
        }

        private void btnSleep_Click(object sender, EventArgs e)
        {
            _pet.Sleep();
            UpdateStatsUI();
        }

        private void btnHeal_Click(object sender, EventArgs e)
        {
            _pet.Medicate();
            UpdateStatsUI();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset your pet? All progress will be lost!", "Reset Pet", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _pet.Reset();
                // Check combo box
                _pet.Type = (PetType)cmbPetType.SelectedItem;
                UpdateStatsUI();
            }
        }

        private void cmbPetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPetType.SelectedItem is PetType type)
            {
                // Only change if it's a new pet or egg stage to avoid swapping species mid-life without reset?
                // For fun, we allow swapping if it's still an egg, otherwise warn.
                if (_pet.Stage == LifecycleStage.Egg)
                {
                    _pet.Type = type;
                }
                else if (_pet.Type != type)
                {
                    // If user changes dropdown but pet is alive, we just ignore visuals or force reset?
                    // Let's just update the type for visual variety but keep stats.
                    _pet.Type = type; 
                }
            }
        }
    }
}
