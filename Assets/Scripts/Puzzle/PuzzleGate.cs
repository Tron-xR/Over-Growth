using System.Collections;
using Game.Dialogue;
using UnityEngine;
using Game.Core;

namespace Game.Puzzle
{
    /// Opens on interaction when all required flags are set.
    public class PuzzleGate : MonoBehaviour
    {
        [SerializeField] string[] requiredFlags = { "item.mobility_device", "item.access_card" };
        [SerializeField] string flagOnOpen = "door.room_exit";
        [SerializeField] GameObject closedVisual;
        [SerializeField] Collider blocker;
        [SerializeField] DialogueRunner dialogueRunner;

        const string LockedThoughtId = "thought.room_exit_need_items";
        const string LockedThought = "I need a way to move... and a way out";
        Coroutine thoughtRoutine;
        DialogueLine thoughtLine;
        public bool IsOpen { get; private set; }

        IEnumerator Start()
        {
            yield return null; // SaveCoordinator restores the saved door flag in Start.
            if (!string.IsNullOrEmpty(flagOnOpen) && GameState.Has(flagOnOpen)) ApplyOpen();
        }

        void OnDisable()
        {
            if (thoughtRoutine != null) StopCoroutine(thoughtRoutine);
            ClearThought();
        }

        public void Interact()
        {
            if (IsOpen) return;
            if (string.IsNullOrWhiteSpace(flagOnOpen))
            {
                Debug.LogError("PuzzleGate requires a flag to persist its open state.", this);
                return;
            }

            if (!PuzzleGateLogic.RequirementsMet(requiredFlags, GameState.Has))
            {
                if (thoughtRoutine != null) return;
                if (dialogueRunner == null || !dialogueRunner.HasLineListener)
                {
                    Debug.LogError("PuzzleGate requires an active DialogueRunner listener to show its locked thought.", this);
                    return;
                }

                thoughtRoutine = StartCoroutine(ShowLockedThought());
                return;
            }

            ApplyOpen();
            GameState.Set(flagOnOpen);
        }

        IEnumerator ShowLockedThought()
        {
            thoughtLine = ScriptableObject.CreateInstance<DialogueLine>();
            thoughtLine.id = LockedThoughtId;
            thoughtLine.text = LockedThought;
            try
            {
                yield return dialogueRunner.Play(new DialogueSequence(new[] { thoughtLine }));
            }
            finally
            {
                ClearThought();
                thoughtRoutine = null;
            }
        }

        void ClearThought()
        {
            if (thoughtLine != null) Destroy(thoughtLine);
            thoughtLine = null;
        }

        void ApplyOpen()
        {
            if (IsOpen) return;
            IsOpen = true;
            if (blocker) blocker.enabled = false;
            if (closedVisual) closedVisual.SetActive(false);
        }
    }
}
