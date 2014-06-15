using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using WizardMonks;
using WizardMonks.Instances;
using WorldSimulation;

namespace SkillViewer
{
    delegate void AdvanceCharacterDelegate(Character character);

    public partial class WorldGenerator : Form
    {
        private Magus[] _magusArray = new Magus[100];
        private int _magusCount = 0;
        private Ability _latin;
        private Ability _magicTheory;
        private Ability _artLib;
        private Ability _areaLore;
        private Die _die = new Die();
        private List<string> _log;

        public WorldGenerator()
        {
            ImmutableMultiton<int, Ability>.Initialize(MagicArts.GetEnumerator());
            ImmutableMultiton<int, Ability>.Initialize(Abilities.GetEnumerator());
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
                else if (ability.AbilityName == "Area Lore")
                {
                    _areaLore = ability;
                }
            }

            InitializeComponent();

            foreach (Magus founder in Founders.GetEnumerator())
            {
                _magusArray[_magusCount] = founder;
                _magusCount++;
            }
            lstMembers.DataSource = _magusArray.Take(_magusCount).ToList();
             _log = new List<string>();
             lstAdvance.DataSource = _log;
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
            _magusArray[_magusCount] = CharacterFactory.GenerateNewMagus(_magicTheory, _latin, _artLib, _areaLore);
            _magusArray[_magusCount].Name = GenerateName(_magusCount);
            _magusCount++;
            lstMembers.DataSource = _magusArray.Take(_magusCount).ToList();
        }

        private string GenerateName(int nameNumber)
        {
            switch (nameNumber)
            {
                case 0:
                    return "Zed";
                case 1:
                    return "Primus";
                case 2:
                    return "Secundus";
                case 3:
                    return "Tricundus";
                case 4:
                    return "Quatrus";
                case 5:
                    return "Quintus";
                case 6:
                    return "Sextus";
                case 7:
                    return "Septus";
                case 8:
                    return "Octus";
                case 9:
                    return "Novus";
                default:
                    return "Magus " + nameNumber.ToString();
            }
        }

        private void btnAdvance_Click(object sender, EventArgs e)
        {
            btnAdvance.Enabled = false;
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //foreach (Magus mage in _magusArray)
            //{
            //    mage.Advance();
            //}
            Parallel.ForEach(_magusArray.Where(m => m != null), character =>
                {
                    _log.Add("Advancing " + character.Name);
                    Task reportProgressTask = Task.Factory.StartNew(() =>
                        {
                            lstAdvance.DataSource = null;
                            lstAdvance.DataSource = _log;
                        },
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        uiScheduler);
                    character.Advance();
                    _log.Add("Done advancing " + character.Name);
                    reportProgressTask = Task.Factory.StartNew(() =>
                    {
                        lstAdvance.DataSource = null;
                        lstAdvance.DataSource = _log;
                    },
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        uiScheduler);
                } );

            btnAdvance.Enabled = true;
        }
    }
}
