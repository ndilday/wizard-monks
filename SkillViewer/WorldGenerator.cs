using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using WizardMonks;
using WizardMonks.Characters;
using WizardMonks.Core;
using WizardMonks.Instances;
using WorldSimulation;

namespace SkillViewer
{
    delegate void AdvanceCharacterDelegate(Character character);

    public partial class WorldGenerator : Form
    {
        private readonly Magus[] _magusArray = new Magus[40];
        private readonly Ability _latin;
        private readonly Ability _magicTheory;
        private readonly Ability _artLib;
        private readonly Ability _areaLore;
        private readonly List<string> _log;
        private readonly Random _rand = new();

        private int _magusCount = 0;

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
            _log = [];
            lstAdvance.DataSource = _log;
        }

        public void InitializeFromFile()
        {
            OpenFileDialog ofd = new()
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                FilterIndex = 0
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImmutableMultiton<int, Ability>.Initialize(ofd.FileName);
            }
        }

        private void lstMembers_DoubleClick(object sender, EventArgs e)
        {
            // clicking an entry should open up that character in another window
            CharacterSheet sheet = new(_magusArray[lstMembers.SelectedIndex]);
            sheet.ShowDialog(this);
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            _magusArray[_magusCount] = CharacterFactory.GenerateNewMagus(_magicTheory, _latin, _artLib, _areaLore);
            _magusArray[_magusCount].Name = GenerateName(_magusCount);
            _magusCount++;
            lstMembers.DataSource = _magusArray.Take(_magusCount).ToList();
        }

        private static string GenerateName(int nameNumber)
        {
            return nameNumber switch
            {
                0 => "Zed",
                1 => "Primus",
                2 => "Secundus",
                3 => "Tricundus",
                4 => "Quatrus",
                5 => "Quintus",
                6 => "Sextus",
                7 => "Septus",
                8 => "Octus",
                9 => "Novus",
                _ => "Magus " + nameNumber.ToString(),
            };
        }

        private void btnAdvance_Click(object sender, EventArgs e)
        {
            btnAdvance.Enabled = false;
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            var fullMagi = _magusArray.Where(m => m != null && m.House != Houses.Apprentice);
            var magiDesires = fullMagi.Select(m => m.GenerateTradingDesires()).ToList();
            GlobalEconomy.DesiredBooksList = magiDesires.SelectMany(md => md.BookDesires.Values).Distinct().ToList();
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
