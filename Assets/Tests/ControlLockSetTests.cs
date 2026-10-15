using Game.Player.Logic;
using NUnit.Framework;

namespace Game.Parts.Tests
{
    public class ControlLockSetTests
    {
        [Test]
        public void MultipleLocksRequireEveryReasonToBeRemoved()
        {
            var locks = new ControlLockSet();
            locks.Add("intro");
            locks.Add("sight");

            locks.Remove("intro");

            Assert.That(locks.IsLocked, Is.True);
            locks.Remove("sight");
            Assert.That(locks.IsLocked, Is.False);
        }

        [Test]
        public void DuplicateAddIsIdempotent()
        {
            var locks = new ControlLockSet();
            locks.Add("inventory");
            locks.Add("inventory");

            locks.Remove("inventory");

            Assert.That(locks.IsLocked, Is.False);
        }

        [Test]
        public void RemovingUnknownReasonLeavesExistingLock()
        {
            var locks = new ControlLockSet();
            locks.Add("respawn");

            Assert.That(locks.Remove("inventory"), Is.False);
            Assert.That(locks.IsLocked, Is.True);
        }

        [Test]
        public void LocksCanBeReleasedIndependently()
        {
            var locks = new ControlLockSet();
            locks.Add("respawn");
            locks.Add("sight");

            locks.Remove("sight");

            Assert.That(locks.IsLocked, Is.True);
            locks.Remove("respawn");
            Assert.That(locks.IsLocked, Is.False);
        }
    }
}
