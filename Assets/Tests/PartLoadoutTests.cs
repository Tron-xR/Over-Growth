using Game.Parts;
using Game.Core;
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

        [Test]
        public void InventoryRejectsUnownedPartsAndPersistsOwnedEquippedIds()
        {
            var inventory = new PartInventory(3);
            part.id = "wheel_base";
            var legs = CreatePart(3);
            legs.id = "legs_g7";

            Assert.That(inventory.Equip(part), Is.False);
            Assert.That(inventory.Add(part), Is.True);
            Assert.That(inventory.Add(part), Is.False);
            Assert.That(inventory.Add(legs), Is.True);
            Assert.That(inventory.Equip(part), Is.True);
            Assert.That(inventory.Equip(legs), Is.False, "Over-budget parts must remain owned but unequipped.");

            var saved = new SaveData { ownedParts = new[] { "wheel_base", "legs_g7" }, equippedParts = new[] { "wheel_base" } };
            var restoredData = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(saved));
            Assert.That(restoredData.ownedParts, Is.EqualTo(new[] { "wheel_base", "legs_g7" }));
            Assert.That(restoredData.equippedParts, Is.EqualTo(new[] { "wheel_base" }));
        }

        [Test]
        public void RestoreKeepsOnlyOwnedEquippedPartsWithinBudget()
        {
            var inventory = new PartInventory(3);
            part.id = "wheel_base";
            var legs = CreatePart(3);
            legs.id = "legs_g7";

            inventory.Restore(new[] { part, legs }, new[] { part, legs });

            Assert.That(inventory.Owned.Count, Is.EqualTo(2));
            Assert.That(inventory.Loadout.Equipped.Count, Is.EqualTo(1));
            Assert.That(inventory.Loadout.Equipped[0], Is.SameAs(part));
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
