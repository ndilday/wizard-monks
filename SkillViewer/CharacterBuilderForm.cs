using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SkillViewer
{
    public partial class CharacterBuilderForm : Form
    {
        public CharacterBuilderForm()
        {
            InitializeComponent();
        }

        private static void LoadSkills()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                FilterIndex = 0
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamReader reader = new StreamReader(ofd.FileName);
                XmlSerializer serializer = new XmlSerializer(typeof(List<Ability>));
                //PopulateSkillList((List<Ability>)serializer.Deserialize(reader));
            }
        }

        /*private void PopulateSkillList(List<Ability> abilityList)
        {
            skillListView.Clear();
            Dictionary<AbilityType, int> abilityTypeMap = new Dictionary<AbilityType, int>();
            ListViewItem abilityItem;
            foreach (Ability ability in abilityList)
            {
                if (!abilityTypeMap.ContainsKey(ability.AbilityType))
                {
                    abilityTypeMap[ability.AbilityType] = skillListView.Groups.Add(new ListViewGroup(ability.AbilityType.ToString()));
                }

                abilityItem = new ListViewItem(ability.AbilityName);
                abilityItem.Group = skillListView.Groups[abilityTypeMap[ability.AbilityType]];
                skillListView.Items.Add(abilityItem);
            }
        }*/
    }
}
