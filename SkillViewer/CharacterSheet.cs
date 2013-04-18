using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WizardMonks;

namespace SkillViewer
{
    public partial class CharacterSheet : Form
    {
        private Character _character;
        public CharacterSheet(Character character)
        {
            _character = character;
            InitializeComponent();

            txtStrength.Text = _character.Strength.Value.ToString();
            txtStamina.Text = _character.Stamina.Value.ToString();
            txtDexterity.Text = _character.Dexterity.Value.ToString();
            txtQuickness.Text = _character.Quickness.Value.ToString();
            txtIntelligence.Text = _character.Intelligence.Value.ToString();
            txtCommunication.Text = _character.Communication.Value.ToString();
            txtPresence.Text = _character.Presence.Value.ToString();
            txtPerception.Text = _character.Perception.Value.ToString();
        }
    }
}
