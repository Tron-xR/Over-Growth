using UnityEngine;

namespace Game.Dialogue
{
    [CreateAssetMenu(menuName = "Game/Dialogue Line")]
    public class DialogueLine : ScriptableObject
    {
        public string id;
        [TextArea] public string text;
        public AudioClip audio;
    }
}
