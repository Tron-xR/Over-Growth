using System.Collections;
using Game.Core;
using Game.Dialogue;
using Game.Player;
using UnityEngine;

namespace Game.UI
{
    public class IntroSequence : MonoBehaviour
    {
        const string IntroDoneFlag = "intro.done";
        const string ControlLock = "ui.intro_sequence";

        [SerializeField] CanvasGroup blackScreen;
        [SerializeField] PlayerController player;
        [SerializeField] DialogueRunner dialogueRunner;
        [SerializeField] DialogueLine[] lines;

        void Start()
        {
            if (blackScreen == null || player == null || dialogueRunner == null || !dialogueRunner.HasLineListener || !HasValidLines())
            {
                Debug.LogError("IntroSequence requires a black screen, PlayerController, four valid lines, DialogueRunner, and enabled DialogueBox listener.", this);
                enabled = false;
                return;
            }

            player.AddLock(ControlLock);
            blackScreen.alpha = 1f;
            blackScreen.blocksRaycasts = true;
            StartCoroutine(BeginIntro());
        }

        bool HasValidLines()
        {
            if (lines == null || lines.Length != 4) return false;
            foreach (var line in lines)
                if (line == null || string.IsNullOrWhiteSpace(line.id) || string.IsNullOrWhiteSpace(line.text)) return false;
            return true;
        }

        IEnumerator BeginIntro()
        {
            // Let SaveCoordinator restore flags from disk before checking the intro gate.
            yield return null;
            if (!DialogueSequence.CanStartIntro(GameState.Has(IntroDoneFlag)))
            {
                blackScreen.alpha = 0f;
                blackScreen.blocksRaycasts = false;
                player.RemoveLock(ControlLock);
                yield break;
            }

            yield return dialogueRunner.Play(new DialogueSequence(lines));
            if (!dialogueRunner.LastPlaySucceeded) yield break;
            GameState.Set(IntroDoneFlag);
            blackScreen.alpha = 0f;
            blackScreen.blocksRaycasts = false;
            player.RemoveLock(ControlLock);
        }
    }
}
