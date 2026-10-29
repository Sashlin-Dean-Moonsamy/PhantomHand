using UnityEngine;

namespace QuestSystem
{
    /// <summary>
    /// Attach to any UI element (button, panel, icon) you want the tutorial system
    /// to be able to point at or glow. Registers itself with TutorialHighlighter
    /// using targetId, which should match a QuestStepSO.targetId for a highlightTarget step.
    /// </summary>
    public class TutorialHighlightTarget : MonoBehaviour
    {
        [Tooltip("Must match the targetId set on the corresponding QuestStepSO.")]
        public string targetId;

        void OnEnable()
        {
            TutorialHighlighter.Register(targetId, transform as RectTransform);
        }

        void OnDisable()
        {
            TutorialHighlighter.Unregister(targetId);
        }
    }
}
