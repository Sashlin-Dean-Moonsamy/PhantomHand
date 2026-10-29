using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    /// <summary>
    /// Singleton that positions an arrow/glow overlay over whichever UI element is
    /// currently registered under the active tutorial step's targetId.
    ///
    /// Put this on a Canvas overlay object alongside your arrow/glow RectTransform,
    /// and assign highlightVisual in the Inspector.
    /// </summary>
    public class TutorialHighlighter : MonoBehaviour
    {
        public static TutorialHighlighter Instance { get; private set; }

        [Tooltip("The arrow/glow RectTransform that gets moved on top of the target.")]
        [SerializeField] private RectTransform highlightVisual;

        [Tooltip("Optional offset so the arrow doesn't sit exactly on top of the target.")]
        [SerializeField] private Vector2 offset = new Vector2(0f, 40f);

        private static readonly Dictionary<string, RectTransform> registeredTargets = new Dictionary<string, RectTransform>();
        private string currentTargetId;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (highlightVisual != null)
                highlightVisual.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (string.IsNullOrEmpty(currentTargetId) || highlightVisual == null) return;

            if (registeredTargets.TryGetValue(currentTargetId, out var target) && target != null)
            {
                highlightVisual.position = (Vector2)target.position + offset;
            }
            else
            {
                // Target isn't on screen/registered right now (e.g. menu closed) - hide until it appears.
                highlightVisual.gameObject.SetActive(false);
            }
        }

        public static void Register(string targetId, RectTransform target)
        {
            if (string.IsNullOrEmpty(targetId) || target == null) return;
            registeredTargets[targetId] = target;

            // If the highlighter is already waiting for this target, show it now.
            if (Instance != null && Instance.currentTargetId == targetId && Instance.highlightVisual != null)
                Instance.highlightVisual.gameObject.SetActive(true);
        }

        public static void Unregister(string targetId)
        {
            if (registeredTargets.ContainsKey(targetId))
                registeredTargets.Remove(targetId);
        }

        public void HighlightTarget(string targetId)
        {
            currentTargetId = targetId;
            bool found = registeredTargets.ContainsKey(targetId);
            if (highlightVisual != null)
                highlightVisual.gameObject.SetActive(found);
        }

        public void ClearHighlight()
        {
            currentTargetId = null;
            if (highlightVisual != null)
                highlightVisual.gameObject.SetActive(false);
        }
    }
}
