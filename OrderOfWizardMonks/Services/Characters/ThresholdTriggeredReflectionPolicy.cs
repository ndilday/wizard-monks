using WizardMonks.Models.Characters;

namespace WizardMonks.Services.Characters
{
    /// <summary>
    /// Triggers reflection when accumulated unprocessed importance crosses a threshold.
    /// </summary>
    public sealed class ThresholdTriggeredReflectionPolicy : IReflectionPolicy
    {
        private readonly float _importanceThreshold;

        public ThresholdTriggeredReflectionPolicy(float importanceThreshold = 3.0f)
        {
            _importanceThreshold = importanceThreshold;
        }

        public bool ShouldReflect(Character character, CharacterMemoryStream memoryStream, int currentTick)
            => memoryStream.AccumulatedUnprocessedImportance >= _importanceThreshold;
    }
}