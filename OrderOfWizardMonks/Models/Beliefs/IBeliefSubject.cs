using System;

namespace WizardMonks.Models.Beliefs
{
    public interface IBeliefSubject
    {
        Guid Id { get; } // A unique identifier for any subject.
        string Name { get; }
    }
}
