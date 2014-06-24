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
            DisplayBooks();
            DisplayVis();
            DisplayMisc();
            lstLog.DataSource = _character.Log;
        }

        private void DisplayMisc()
        {
            txtAge.Text = (_character.SeasonalAge / 4).ToString();
            txtWarp.Text = _character.Warping.Value.ToString(FORMAT_STRING);
        }

        private void DisplayCharacteristics()
        {
            txtStrength.Text = _character.GetAttribute(AttributeType.Strength).Value.ToString(FORMAT_STRING);
            txtStamina.Text = _character.GetAttribute(AttributeType.Stamina).Value.ToString(FORMAT_STRING);
            txtDexterity.Text = _character.GetAttribute(AttributeType.Dexterity).Value.ToString(FORMAT_STRING);
            txtQuickness.Text = _character.GetAttribute(AttributeType.Quickness).Value.ToString(FORMAT_STRING);
            txtIntelligence.Text = _character.GetAttribute(AttributeType.Intelligence).Value.ToString(FORMAT_STRING);
            txtCommunication.Text = _character.GetAttribute(AttributeType.Communication).Value.ToString(FORMAT_STRING);
            txtPresence.Text = _character.GetAttribute(AttributeType.Presence).Value.ToString(FORMAT_STRING);
            txtPerception.Text = _character.GetAttribute(AttributeType.Perception).Value.ToString(FORMAT_STRING);
        }

        private void DisplayArts()
        {
            txtCreo.Text = _character.GetAbility(MagicArts.Creo).Value.ToString(FORMAT_STRING);
            txtIntellego.Text = _character.GetAbility(MagicArts.Intellego).Value.ToString(FORMAT_STRING);
            txtMuto.Text = _character.GetAbility(MagicArts.Muto).Value.ToString(FORMAT_STRING);
            txtPerdo.Text = _character.GetAbility(MagicArts.Perdo).Value.ToString(FORMAT_STRING);
            txtRego.Text = _character.GetAbility(MagicArts.Rego).Value.ToString(FORMAT_STRING);
            txtAnimal.Text = _character.GetAbility(MagicArts.Animal).Value.ToString(FORMAT_STRING);
            txtAquam.Text = _character.GetAbility(MagicArts.Aquam).Value.ToString(FORMAT_STRING);
            txtAuram.Text = _character.GetAbility(MagicArts.Auram).Value.ToString(FORMAT_STRING);
            txtCorpus.Text = _character.GetAbility(MagicArts.Corpus).Value.ToString(FORMAT_STRING);
            txtHerbam.Text = _character.GetAbility(MagicArts.Herbam).Value.ToString(FORMAT_STRING);
            txtIgnem.Text = _character.GetAbility(MagicArts.Ignem).Value.ToString(FORMAT_STRING);
            txtImaginem.Text = _character.GetAbility(MagicArts.Imaginem).Value.ToString(FORMAT_STRING);
            txtMentem.Text = _character.GetAbility(MagicArts.Mentem).Value.ToString(FORMAT_STRING);
            txtTerram.Text = _character.GetAbility(MagicArts.Terram).Value.ToString(FORMAT_STRING);
            txtVim.Text = _character.GetAbility(MagicArts.Vim).Value.ToString(FORMAT_STRING);
        }

        private void DisplayAbilities()
        {
            dgvAbilities.DataSource = _character.GetAbilities().Where(a => !MagicArts.IsArt(a.Ability)).OrderBy(a => a.Ability.AbilityName).ToList();
        }

        private void DisplayBooks()
        {
            dgvBooks.DataSource = _character.Books;
        }

        private void DisplayVis()
        {
            Magus mage = (Magus)_character;
            txtCreoVis.Text = mage.GetVisCount(MagicArts.Creo).ToString(FORMAT_STRING);
            txtIntellegoVis.Text = mage.GetVisCount(MagicArts.Intellego).ToString(FORMAT_STRING);
            txtMutoVis.Text = mage.GetVisCount(MagicArts.Muto).ToString(FORMAT_STRING);
            txtPerdoVis.Text = mage.GetVisCount(MagicArts.Perdo).ToString(FORMAT_STRING);
            txtRegoVis.Text = mage.GetVisCount(MagicArts.Rego).ToString(FORMAT_STRING);
            txtAnimalVis.Text = mage.GetVisCount(MagicArts.Animal).ToString(FORMAT_STRING);
            txtAquamVis.Text = mage.GetVisCount(MagicArts.Aquam).ToString(FORMAT_STRING);
            txtAuramVis.Text = mage.GetVisCount(MagicArts.Auram).ToString(FORMAT_STRING);
            txtCorpusVis.Text = mage.GetVisCount(MagicArts.Corpus).ToString(FORMAT_STRING);
            txtHerbamVis.Text = mage.GetVisCount(MagicArts.Herbam).ToString(FORMAT_STRING);
            txtIgnemVis.Text = mage.GetVisCount(MagicArts.Ignem).ToString(FORMAT_STRING);
            txtImaginemVis.Text = mage.GetVisCount(MagicArts.Imaginem).ToString(FORMAT_STRING);
            txtMentemVis.Text = mage.GetVisCount(MagicArts.Mentem).ToString(FORMAT_STRING);
            txtTerramVis.Text = mage.GetVisCount(MagicArts.Terram).ToString(FORMAT_STRING);
            txtVimVis.Text = mage.GetVisCount(MagicArts.Vim).ToString(FORMAT_STRING);
        }
    }
}
