using Game.World.Logic;
using NUnit.Framework;

namespace Game.Parts.Tests
{
    public class RespawnLogicTests
    {
        [TestCase(-5.1f, true)]
        [TestCase(-5f, false)]
        [TestCase(-4.9f, false)]
        public void FallThresholdTriggersOnlyBelowKillY(float y, bool expected) =>
            Assert.That(RespawnLogic.ShouldRespawn(y, -5f, false), Is.EqualTo(expected));

        [Test]
        public void RespawnInProgressPreventsAnotherRespawn() =>
            Assert.That(RespawnLogic.ShouldRespawn(-10f, -5f, true), Is.False);

        [Test]
        public void SafePositionChangesOnlyWhenGroundedAndAboveKillY()
        {
            var tracker = new SafePositionTracker();
            tracker.Reset(1f, 2f, 3f);

            Assert.That(tracker.TryUpdate(4f, 5f, 6f, false, -5f), Is.False);
            Assert.That(tracker.TryUpdate(7f, -6f, 8f, true, -5f), Is.False);
            Assert.That(tracker.TryUpdate(9f, 10f, 11f, true, -5f), Is.True);
            Assert.That(tracker.TryGet(out float x, out float y, out float z), Is.True);
            Assert.That((x, y, z), Is.EqualTo((9f, 10f, 11f)));
        }

        [TestCase(float.NaN, 0f, 0f, true)]
        [TestCase(float.PositiveInfinity, 0f, 0f, true)]
        [TestCase(0f, -6f, 0f, true)]
        [TestCase(0f, 0f, 0f, false)]
        public void InvalidSavedPositionsAreRejected(float x, float y, float z, bool areaExists) =>
            Assert.That(RespawnLogic.IsSavedPositionValid(x, y, z, -5f, areaExists), Is.False);
    }
}
