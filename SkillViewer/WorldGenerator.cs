using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

using WizardMonks;
using WizardMonks.Instances;
using WorldSimulation;

namespace SkillViewer
{
    public partial class WorldGenerator : Form
    {
        private Magus[] _magusArray = new Magus[1000];
        private uint _magusCount = 0;
        private Ability _latin;
        private Ability _magicTheory;
        private Ability _artLib;
        private Die _die = new Die();

        public WorldGenerator()
        {
            ImmutableMultiton<int, Ability>.Initialize(MagicArts.GetEnumerator());
            // read out values in multiton
            foreach(int i in ImmutableMultiton<int, Ability>.GetKeys())
            {
                Ability ability = ImmutableMultiton<int, Ability>.GetInstance(i);
                if(ability.AbilityName == "Latin")
                {
                    _latin = ability;
                }
                else if(ability.AbilityName == "Magic Theory")
                {
                    _magicTheory = ability;
                }
                else if (ability.AbilityName == "Artes Liberales")
                {
                    _artLib = ability;
                }
            }

            InitializeComponent();
        }

        public void InitializeFromFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            ofd.FilterIndex = 0;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImmutableMultiton<int, Ability>.Initialize(ofd.FileName);
            }
        }

        private void lstMembers_DoubleClick(object sender, EventArgs e)
        {
            // clicking an entry should open up that character in another window
            CharacterSheet sheet = new CharacterSheet(_magusArray[lstMembers.SelectedIndex]);
            sheet.ShowDialog(this);
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _magusArray[_magusCount] = CharacterFactory.GenerateNewMagus(_magicTheory, _latin, _artLib);
        }
    }
}
