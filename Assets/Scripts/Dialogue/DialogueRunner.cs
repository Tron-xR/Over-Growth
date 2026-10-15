using System;
using System.Collections;
using UnityEngine;

namespace Game.Dialogue
{
    public class DialogueRunner : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;

        public event Action<DialogueLine, Action<bool>> OnLineRequested;
        public bool HasLineListener => OnLineRequested != null;
        public bool LastPlaySucceeded { get; private set; }

        public IEnumerator Play(DialogueSequence sequence)
        {
            LastPlaySucceeded = true;
            if (sequence == null) yield break;

            while (sequence.TryGetNext(out var line))
            {
                if (line.audio != null && audioSource != null) audioSource.PlayOneShot(line.audio);
                bool completed = false;
                bool lineSucceeded = false;
                OnLineRequested?.Invoke(line, succeeded => { lineSucceeded = succeeded; completed = true; });
                if (OnLineRequested == null)
                {
                    LastPlaySucceeded = false;
                    Debug.LogError("DialogueRunner has no DialogueBox listener.", this);
                    yield break;
                }
                while (!completed) yield return null;
                if (!lineSucceeded)
                {
                    LastPlaySucceeded = false;
                    yield break;
                }
            }
        }
    }
}
