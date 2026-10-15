using Game.Senses;
using NUnit.Framework;

namespace Game.Parts.Tests
{
    public class SightFadeStateTests
    {
        [TestCase(false, false, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        public void FadeStartsOnlyAfterIntroAndBeforeSight(bool introDone, bool sightRestored, bool expected) =>
            Assert.That(SightFadeState.CanBegin(introDone, sightRestored), Is.EqualTo(expected));

        [TestCase(-1f, 2f, 0f)]
        [TestCase(1f, 2f, 0.5f)]
        [TestCase(3f, 2f, 1f)]
        [TestCase(0f, 0f, 1f)]
        public void ProgressClampsToFadeRange(float elapsed, float duration, float expected) =>
            Assert.That(SightFadeState.Progress(elapsed, duration), Is.EqualTo(expected));
    }
}
