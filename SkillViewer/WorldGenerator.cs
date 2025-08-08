using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using WizardMonks;
using WizardMonks.Core;
using WizardMonks.Economy;
using WizardMonks.Instances;

namespace SkillViewer
{
    delegate void AdvanceCharacterDelegate(Character character);

    [SupportedOSPlatform("windows7.0")]
    public partial class WorldGenerator : Form
    {
        private Magus[] _magusArray = new Magus[40];
        private int _magusCount = 0;
        private Ability _latin;
        private Ability _magicTheory;
        private Ability _artLib;
        private Ability _areaLore;
        //private Die _die = new Die();
        private List<string> _log;
        private Random _rand = new Random();

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

            var fullMagi = _magusArray.Where(m => m != null && m.House != HousesEnum.Apprentice);
            _log.Add("Advancing Season");
            Parallel.ForEach(_magusArray.Where(m => m != null && !m.WantsToFollow), character =>
            {
                Task reportProgressTask = Task.Factory.StartNew(() =>
                    {
                        lstAdvance.DataSource = null;
                        lstAdvance.DataSource = _log;
                    },
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    uiScheduler);
                character.Advance();
                reportProgressTask = Task.Factory.StartNew(() =>
                {
                    lstAdvance.DataSource = null;
                    lstAdvance.DataSource = _log;
                },
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    uiScheduler);
            } );

            // TODO: handle collaboration
            _log.Add("Handling collaboration");
            Parallel.ForEach(_magusArray.Where(m => m != null && m.WantsToFollow), character =>
            {
                Task reportProgressTask = Task.Factory.StartNew(() =>
                {
                    lstAdvance.DataSource = null;
                    lstAdvance.DataSource = _log;
                },
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    uiScheduler);
                character.Advance();
                reportProgressTask = Task.Factory.StartNew(() =>
                {
                    lstAdvance.DataSource = null;
                    lstAdvance.DataSource = _log;
                },
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    uiScheduler);
            });

            // add any new apprentices found during the past season
            var newApprentices = _magusArray.Where(m => m != null && m.Apprentice != null)
                                            .Select(m => m.Apprentice)
                                            .Where(a => !_magusArray.Contains(a));
            foreach (Magus apprentice in newApprentices)
            {
                _magusArray[_magusCount] = apprentice;
                _magusCount++;
            }
            _log.Add("Done Advancing Season");

            var magiDesires = fullMagi.Select(m => m.GenerateTradingDesires()).ToList();
            GlobalEconomy.DesiredBooksByTopic = magiDesires
                .SelectMany(md => md.BookDesires.Values)
                .GroupBy(d => d.Ability)
                .ToDictionary(g => g.Key, g => g.ToList());
            GlobalEconomy.LabTextDesiresBySpellBase = magiDesires.SelectMany(md => md.LabTextDesires.Values)
                .GroupBy(d => d.SpellBase)
                .ToDictionary(g => g.Key, g => g.ToList());

            _log.Add("Considering vis and book trades");
            foreach (Magus mage in fullMagi.OrderBy(m => _rand.NextDouble()))
            {
                if (mage == null) continue;
                mage.EvaluateTradingDesires(magiDesires);
                magiDesires = magiDesires.Where(d => d.Mage != mage).ToList();
            }
            _log.Add("Done considering vis and book trades");
            btnAdvance.Enabled = true;
        }
    }
}
