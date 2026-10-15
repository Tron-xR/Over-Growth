using Game.Puzzle.Logic;
using NUnit.Framework;
using UnityEngine;

namespace Game.Parts.Tests
{
    public class PushPullLogicTests
    {
        [TestCase(0.5f, 1f, 0f)]
        [TestCase(1f, 1f, 1f)]
        [TestCase(2f, 1f, 2f)]
        [TestCase(1f, 0f, 0f)]
        public void PushSpeedUsesForceAndMass(float force, float mass, float expected)
        {
            Assert.That(PushPullLogic.PushSpeed(force, mass), Is.EqualTo(expected));
        }

        [Test]
        public void GroundSnapPlacesColliderBottomAboveGround()
        {
            Assert.That(PushPullLogic.GroundSnapY(3f, -0.5f, 0f, 0.03f), Is.EqualTo(0.53f).Within(0.0001f));
        }

        [Test]
        public void CarryingMoreMassReducesSpeed()
        {
            Assert.That(PushPullLogic.CarrySpeedMultiplier(2f), Is.LessThan(PushPullLogic.CarrySpeedMultiplier(0.5f)));
            Assert.That(PushPullLogic.CarrySpeedMultiplier(0f), Is.EqualTo(1f));
        }

        [Test]
        public void FreeSpotChoiceReturnsNearestAvailableCandidate()
        {
            bool found = PushPullLogic.TryChooseFreeSpot(3, index => index == 2, out int chosen);
            Assert.That(found, Is.True);
            Assert.That(chosen, Is.EqualTo(2));
        }

        [Test]
        public void FreeSpotChoiceFailsWhenAllCandidatesAreBlocked()
        {
            Assert.That(PushPullLogic.TryChooseFreeSpot(1, _ => false, out _), Is.False);
        }

        [Test]
        public void InteractionStateAllowsOneHeldTypeAndReturnsToIdleOnRelease()
        {
            var state = new ObjectInteractionStateMachine();
            Assert.That(state.State, Is.EqualTo(ObjectInteractionState.Idle));
            Assert.That(state.TryStart(ObjectInteractionState.Carrying), Is.True);
            Assert.That(state.TryStart(ObjectInteractionState.Gripping), Is.False);
            Assert.That(state.Release(ObjectInteractionState.Gripping), Is.False);
            Assert.That(state.Release(ObjectInteractionState.Carrying), Is.True);
            Assert.That(state.TryStart(ObjectInteractionState.Gripping), Is.True);
            Assert.That(state.Release(ObjectInteractionState.Gripping), Is.True);
        }

        [TestCase(2f, 0f, 1f, 0f)]
        [TestCase(0f, -2f, 0f, -1f)]
        [TestCase(2f, 2f, 1f, 0f)]
        public void GripFaceSelectionChoosesNearestSideAndBreaksDiagonalTiesByX(float px, float pz, float x, float z)
        {
            var face = PushPullLogic.ChooseGripFace(px, pz);
            float faceX = face == GripFace.PositiveX ? 1f : face == GripFace.NegativeX ? -1f : 0f;
            float faceZ = face == GripFace.PositiveZ ? 1f : face == GripFace.NegativeZ ? -1f : 0f;
            Assert.That(faceX, Is.EqualTo(x));
            Assert.That(faceZ, Is.EqualTo(z));
        }

        [Test]
        public void GripInputProjectsToOneAxis()
        {
            Assert.That(PushPullLogic.ProjectGripInput(0.6f, 0.8f, 1f, 0f), Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(PushPullLogic.ProjectGripInput(0.6f, 0.8f, 0f, -1f), Is.EqualTo(-0.8f).Within(0.0001f));
        }

        [Test]
        public void GripMovementCanBeFreeOrProjectedToGripAxis()
        {
            var free = PushPullLogic.ProjectMovement(0.6f, 0.8f, 1f, 0f, false);
            var locked = PushPullLogic.ProjectMovement(0.6f, 0.8f, 1f, 0f, true);
            Assert.That(free.X, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(free.Z, Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(locked.X, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(locked.Z, Is.Zero);
        }

        [Test]
        public void GripDeltaSlidesAlongUnblockedAxisAndStopsWhenBothAxesBlocked()
        {
            var slide = PushPullLogic.ResolveDeltaPerAxis(1f, 2f, (x, z) => z > 0f);
            var blocked = PushPullLogic.ResolveDeltaPerAxis(1f, 2f, (x, z) => x > 0f);
            Assert.That(slide.X, Is.EqualTo(1f));
            Assert.That(slide.Z, Is.Zero);
            Assert.That(blocked.X, Is.Zero);
            Assert.That(blocked.Z, Is.EqualTo(2f));
        }

        [TestCase(GripFace.PositiveX, 1.55f, 0f)]
        [TestCase(GripFace.NegativeZ, 0f, -2.55f)]
        public void GripSocketOffsetUsesChosenFace(GripFace face, float expectedX, float expectedZ)
        {
            PushPullLogic.GripSocketOffset(face, 1f, 2f, 0.5f, 0.05f, out float x, out float z);
            Assert.That(x, Is.EqualTo(expectedX).Within(0.0001f));
            Assert.That(z, Is.EqualTo(expectedZ).Within(0.0001f));
        }

        [TestCase(1f, 0f, 90f)]
        [TestCase(-1f, 0f, -90f)]
        [TestCase(0f, 1f, 0f)]
        [TestCase(0f, -1f, 180f)]
        [TestCase(1f, 1f, 90f)]
        public void BlockFacingSnapsToResolvedCardinalMovement(float x, float z, float expectedYaw)
        {
            Assert.That(PushPullLogic.MovementYawDegrees(x, z), Is.EqualTo(expectedYaw));
        }

        [Test]
        public void ThrowVelocityIsDerivedClampedAndIncludesArc()
        {
            Assert.That(PushPullLogic.ThrowSpeed(2f, 1f, 5f, 2f, 4f), Is.EqualTo(4f).Within(0.0001f));
            Assert.That(PushPullLogic.ThrowSpeed(0.5f, 1f, 1f, 2f, 4f), Is.EqualTo(2f).Within(0.0001f));
            Assert.That(PushPullLogic.ThrowSpeed(0f, 1f, 1f, 2f, 4f), Is.EqualTo(0f));
        }
    }
}
