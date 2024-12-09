using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [System.Serializable]
    public class UISequenceStep
    {
        public GameObject uiElement;
        public float fadeInDuration = 1f;
        public bool waitForEvent = false;
        public float displayDuration = 2f;
        public UnityEngine.Events.UnityEvent eventToWaitFor;
        public float fadeOutDuration = 1f;
    }

    [System.Serializable]
    public class UISequence
    {
        public string sequenceName;
        public List<UISequenceStep> steps = new List<UISequenceStep>();
    }

    public List<UISequence> sequences = new List<UISequence>();

    public void StartSequence(string sequenceName)
    {
        UISequence sequence = sequences.Find(s => s.sequenceName == sequenceName);
        if (sequence != null)
        {
            StartCoroutine(RunSequence(sequence));
        }
        else
        {
            Debug.LogError("Sequence not found: " + sequenceName);
        }
    }

    private IEnumerator RunSequence(UISequence sequence)
    {
        foreach (UISequenceStep step in sequence.steps)
        {
            if (step.uiElement == null)
            {
                continue;
            }

            // Ensure UI element has necessary components
            CanvasGroup canvasGroup = step.uiElement.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = step.uiElement.AddComponent<CanvasGroup>();
            }

            // Set alpha to 1 when a sequence entry starts
            canvasGroup.alpha = 1f;
            step.uiElement.SetActive(true);

            // Fade in
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, step.fadeInDuration));

            // Wait for duration or event
            if (step.waitForEvent && step.eventToWaitFor != null)
            {
                bool eventTriggered = false;
                UnityEngine.Events.UnityAction action = () => { eventTriggered = true; };
                step.eventToWaitFor.AddListener(action);

                yield return new WaitUntil(() => eventTriggered);

                step.eventToWaitFor.RemoveListener(action);
            }
            else
            {
                yield return new WaitForSeconds(step.displayDuration);
            }

            // Fade out
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, step.fadeOutDuration));

            step.uiElement.SetActive(false);
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float duration)
    {
        float counter = 0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, counter / duration);
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}
