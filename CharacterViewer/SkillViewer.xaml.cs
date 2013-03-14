using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

using OrderOfHermes;

namespace CharacterViewer
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class SkillViewer : Window
	{
		public SkillViewer()
		{
			InitializeComponent();
		}

		private void LoadSkillsButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            ofd.FilterIndex = 0;
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamReader reader = new StreamReader(ofd.FileName);
                XmlSerializer serializer = new XmlSerializer(typeof(List<Ability>));
                PopulateSkillTree((List<Ability>)serializer.Deserialize(reader));
            }
		}

        private void WriteSkillsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            sfd.FilterIndex = 0;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<Ability> abilityList = new List<Ability>();
                abilityList.Add(new Ability(AbilityType.Academic, "Artes Liberales"));
                abilityList.Add(new Ability(AbilityType.Martial, "Single Weapon"));
                abilityList.Add(new Ability(AbilityType.Martial, "Great Weapon"));
                abilityList.Add(new Ability(AbilityType.Martial, "Bow"));
                abilityList.Add(new Ability(AbilityType.General, "Awareness"));
                abilityList.Add(new Ability(AbilityType.General, "Brawl"));
                abilityList.Add(new Ability(AbilityType.General, "Hunt"));

                XmlSerializer serializer = new XmlSerializer(typeof(List<Ability>));
                StreamWriter writer = System.IO.File.CreateText(sfd.FileName);
                serializer.Serialize(writer, abilityList);
            }
        }

        private void PopulateSkillTree(List<Ability> abilityList)
        {
            
            Dictionary<AbilityType, List<string>> abilityBuckets = new Dictionary<AbilityType, List<string>>();
            foreach (Ability ability in abilityList)
            {
                if (!abilityBuckets.ContainsKey(ability.AbilityType))
                {
                    abilityBuckets[ability.AbilityType] = new List<string>();
                }
                abilityBuckets[ability.AbilityType].Add(ability.AbilityName);
            }
        }
	}
}
