using System;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;

namespace WizardMonks.Models.Projects
{
    [Serializable]
    public abstract class SummaProject : AProject
    {
        public Summa Summa { get; protected set; }
        public double Progress { get => Summa.PointsComplete; }
        public double PointsNeeded { get => Summa.GetWritingPointsNeeded(); }
        public override bool IsComplete => Progress >= PointsNeeded;

        protected SummaProject(Character owner, Summa summa) : base(owner)
        {
            Summa = summa;
        }

        public void AddProgress(double amount)
        {
            Summa.PointsComplete += amount;
        }
    }

    [Serializable]
    public class SummaWritingProject : SummaProject
    {
        public override string Description => $"Writing the summa '{Summa.Title}' (L{Summa.Level}/Q{Summa.Quality})";
        public SummaWritingProject(Character owner, Summa summa) : base(owner, summa) { }
    }

    [Serializable]
    public class SummaCopyingProject : SummaProject
    {
        public override string Description => $"Copying the summa '{Summa.Title}' (L{Summa.Level}/Q{Summa.Quality})";
        public SummaCopyingProject(Character owner, Summa summa) : base(owner, summa) { }
    }
}