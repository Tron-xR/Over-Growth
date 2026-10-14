using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// Bottom-of-screen thought/dialogue text with typewriter effect.
    public class DialogueBox : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        [SerializeField] float charsPerSecond = 40f;

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
