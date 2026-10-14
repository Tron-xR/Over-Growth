using Game.Parts;
using NUnit.Framework;
using UnityEngine;

namespace Game.Parts.Tests
{
    public class PartLoadoutTests
    {
        PartData part;
        readonly System.Collections.Generic.List<PartData> createdParts = new System.Collections.Generic.List<PartData>();

        [SetUp]
        public void SetUp()
        {
            createdParts.Clear();
            part = CreatePart(1);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in createdParts) Object.DestroyImmediate(created);
            createdParts.Clear();
        }

        [Test]
        public void EquipUnequipAndSlotBudgetAreEnforced()
        {
            part.slotCost = 2;
            var loadout = new PartLoadout(3);

            Assert.That(loadout.Equip(part), Is.True);
            Assert.That(loadout.Equip(part), Is.False, "Duplicate part must be rejected.");
            Assert.That(loadout.Equip(null), Is.False, "Null part must be rejected.");
            Assert.That(loadout.UsedSlots, Is.EqualTo(2));
            Assert.That(loadout.Equip(CreatePart(2)), Is.False, "Part exceeding remaining slots must be rejected.");
            Assert.That(loadout.Unequip(part), Is.True);
            Assert.That(loadout.Unequip(part), Is.False);
        }

        [Test]
        public void StatsMultiplyAndEmptyLoadoutDefaultsToOne()
        {
            var loadout = new PartLoadout(4);
            Assert.That(loadout.SpeedMultiplier, Is.EqualTo(1f));
            Assert.That(loadout.PowerDrainMultiplier, Is.EqualTo(1f));
            Assert.That(loadout.PushPullForce, Is.EqualTo(1f));

            var second = CreatePart(1);
            part.speedMultiplier = 1.2f;
            part.powerDrainMultiplier = 1.5f;
            part.pushPullForce = 1.1f;
            second.speedMultiplier = 1.5f;
            second.powerDrainMultiplier = 0.8f;
            second.pushPullForce = 2f;
            Assert.That(loadout.Equip(part), Is.True);
            Assert.That(loadout.Equip(second), Is.True);
            Assert.That(loadout.SpeedMultiplier, Is.EqualTo(1.8f).Within(0.001f));
            Assert.That(loadout.PowerDrainMultiplier, Is.EqualTo(1.2f).Within(0.001f));
            Assert.That(loadout.PushPullForce, Is.EqualTo(2.2f).Within(0.001f));
        }

        PartData CreatePart(int slots)
        {
            var result = ScriptableObject.CreateInstance<PartData>();
            result.slotCost = slots;
            createdParts.Add(result);
            return result;
        }
    }
}
