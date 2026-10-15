using Game.Dialogue;
using NUnit.Framework;
using UnityEngine;

namespace Game.Parts.Tests
{
    public class DialogueSequenceTests
    {
        [Test]
        public void ReturnsLinesInOrderThenCompletes()
        {
            var first = ScriptableObject.CreateInstance<DialogueLine>();
            var second = ScriptableObject.CreateInstance<DialogueLine>();
            try
            {
                var sequence = new DialogueSequence(new[] { first, second });
                Assert.That(sequence.TryGetNext(out var next), Is.True);
                Assert.That(next, Is.SameAs(first));
                Assert.That(sequence.TryGetNext(out next), Is.True);
                Assert.That(next, Is.SameAs(second));
                Assert.That(sequence.TryGetNext(out next), Is.False);
                Assert.That(next, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void IntroOnlyStartsWhenFlagIsUnset(bool introDone, bool expected) =>
            Assert.That(DialogueSequence.CanStartIntro(introDone), Is.EqualTo(expected));
    }
}
