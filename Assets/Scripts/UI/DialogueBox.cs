using System;
using System.Collections;
using TMPro;
using Game.Dialogue;
using UnityEngine;

namespace Game.UI
{
    /// Bottom-of-screen thought/dialogue text with typewriter effect.
    public class DialogueBox : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        [SerializeField] float charsPerSecond = 40f;
        [SerializeField] DialogueRunner runner;
        Coroutine display;
        Action<bool> completeLine;

        void OnEnable()
        {
            if (runner != null) runner.OnLineRequested += ShowLine;
        }

        void OnDisable()
        {
            if (runner != null) runner.OnLineRequested -= ShowLine;
            if (display != null) StopCoroutine(display);
            display = null;
            if (text != null) text.text = "";
            CompleteLine(false);
        }

        void ShowLine(DialogueLine line, Action<bool> onComplete)
        {
            if (text == null)
            {
                Debug.LogError("DialogueBox requires a text reference.", this);
                onComplete?.Invoke(false);
                return;
            }
            if (display != null) StopCoroutine(display);
            CompleteLine(false);
            completeLine = onComplete;
            display = StartCoroutine(DisplayLine(line.text));
        }

        IEnumerator DisplayLine(string line)
        {
            yield return Show(line);
            display = null;
            CompleteLine(true);
        }

        void CompleteLine(bool succeeded)
        {
            var callback = completeLine;
            completeLine = null;
            callback?.Invoke(succeeded);
        }

        public IEnumerator Show(string line, float holdSeconds = 1.5f)
        {
            text.text = "";
            float delay = 1f / charsPerSecond;
            foreach (char c in line) { text.text += c; yield return new WaitForSeconds(delay); }
            yield return new WaitForSeconds(holdSeconds);
            text.text = "";
        }
    }
}
