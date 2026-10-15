using System;

namespace Game.Dialogue
{
    public sealed class DialogueSequence
    {
        readonly DialogueLine[] lines;
        int index;

        public DialogueSequence(DialogueLine[] lines) => this.lines = lines ?? Array.Empty<DialogueLine>();

        public bool TryGetNext(out DialogueLine line)
        {
            while (index < lines.Length)
            {
                line = lines[index++];
                if (line != null) return true;
            }

            line = null;
            return false;
        }

        public static bool CanStartIntro(bool introDone) => !introDone;
    }
}
