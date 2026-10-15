using System.Collections.Generic;
using Game.Puzzle;
using NUnit.Framework;

namespace Game.Parts.Tests
{
    public class PuzzleGateLogicTests
    {
        static readonly string[] RequiredFlags = { "item.mobility_device", "item.access_card" };

        [TestCase(false, false, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, true, true)]
        public void RoomExitRequiresBothItems(bool hasMobilityDevice, bool hasAccessCard, bool expected)
        {
            var flags = new HashSet<string>();
            if (hasMobilityDevice) flags.Add(RequiredFlags[0]);
            if (hasAccessCard) flags.Add(RequiredFlags[1]);

            Assert.That(PuzzleGateLogic.RequirementsMet(RequiredFlags, flags.Contains), Is.EqualTo(expected));
        }
    }
}
