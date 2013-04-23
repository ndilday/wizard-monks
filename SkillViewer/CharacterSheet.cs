using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WizardMonks;
using WizardMonks.Instances;

namespace SkillViewer
{
    public partial class CharacterSheet : Form
    {
        private Character _character;
        private const string FORMAT_STRING = "0.00";
        public CharacterSheet(Character character)
        {
            _character = character;
            InitializeComponent();

            DisplayCharacteristics();
            DisplayArts();
            DisplayAbilities();
            txtLog.Text = _character.Log;
        }

        private void DisplayCharacteristics()
        {
            txtStrength.Text = _character.Strength.Value.ToString(FORMAT_STRING);
            txtStamina.Text = _character.Stamina.Value.ToString(FORMAT_STRING);
            txtDexterity.Text = _character.Dexterity.Value.ToString(FORMAT_STRING);
            txtQuickness.Text = _character.Quickness.Value.ToString(FORMAT_STRING);
            txtIntelligence.Text = _character.Intelligence.Value.ToString(FORMAT_STRING);
            txtCommunication.Text = _character.Communication.Value.ToString(FORMAT_STRING);
            txtPresence.Text = _character.Presence.Value.ToString(FORMAT_STRING);
            txtPerception.Text = _character.Perception.Value.ToString(FORMAT_STRING);
        }

        private void DisplayArts()
        {
            txtCreo.Text = _character.GetAbility(MagicArts.Creo).GetValue().ToString(FORMAT_STRING);
            txtIntellego.Text = _character.GetAbility(MagicArts.Intellego).GetValue().ToString(FORMAT_STRING);
            txtMuto.Text = _character.GetAbility(MagicArts.Muto).GetValue().ToString(FORMAT_STRING);
            txtPerdo.Text = _character.GetAbility(MagicArts.Perdo).GetValue().ToString(FORMAT_STRING);
            txtRego.Text = _character.GetAbility(MagicArts.Rego).GetValue().ToString(FORMAT_STRING);
            txtAnimal.Text = _character.GetAbility(MagicArts.Animal).GetValue().ToString(FORMAT_STRING);
            txtAquam.Text = _character.GetAbility(MagicArts.Aquam).GetValue().ToString(FORMAT_STRING);
            txtAuram.Text = _character.GetAbility(MagicArts.Auram).GetValue().ToString(FORMAT_STRING);
            txtCorpus.Text = _character.GetAbility(MagicArts.Corpus).GetValue().ToString(FORMAT_STRING);
            txtHerbam.Text = _character.GetAbility(MagicArts.Herbam).GetValue().ToString(FORMAT_STRING);
            txtIgnem.Text = _character.GetAbility(MagicArts.Ignem).GetValue().ToString(FORMAT_STRING);
            txtImaginem.Text = _character.GetAbility(MagicArts.Imaginem).GetValue().ToString(FORMAT_STRING);
            txtMentem.Text = _character.GetAbility(MagicArts.Mentem).GetValue().ToString(FORMAT_STRING);
            txtTerram.Text = _character.GetAbility(MagicArts.Terram).GetValue().ToString(FORMAT_STRING);
            txtVim.Text = _character.GetAbility(MagicArts.Vim).GetValue().ToString(FORMAT_STRING);
        }

        private void DisplayAbilities()
        {
            dgvAbilities.DataSource = _character.GetAbilities();
        }

        private void btnAdvance_Click(object sender, EventArgs e)
        {
            _character.Advance();

            DisplayArts();
            DisplayAbilities();

            txtLog.Text = _character.Log;
        }
    }
}
