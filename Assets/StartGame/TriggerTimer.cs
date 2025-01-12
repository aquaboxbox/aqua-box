using UnityEngine;

public class TriggerTimer : MonoBehaviour
{
    public float triggerTime = 3f; // Time in seconds to trigger the function
    private float timer = 0f;
    private bool isObjectInTrigger = false;
    private bool started = false;
    public ObjectControl tracker;

    private void OnTriggerEnter(Collider other)
    {   
        //Debug.Log($"Object with tag {other.tag} entered the trigger!");
        if (other.CompareTag("Aquabox")) 
        {   
            SoundEffectsManager.Instance.PlaySound("loading");
            isObjectInTrigger = true;
            timer = 0f; // Reset the timer when the object enters
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log($"Object with tag {other.tag} exited the trigger!");
        if (other.CompareTag("Aquabox"))
        {   
            SoundEffectsManager.Instance.StopAllSounds();
            isObjectInTrigger = false;
            timer = 0f; // Reset the timer when the object exits
        }
    }

    private void Update()
    {
        if (!started && isObjectInTrigger)
        {
            timer += Time.deltaTime;

            if (timer >= triggerTime)
            {
                TriggerFunction();
                started = true;
                timer = 0f; // Reset the timer after triggering
            }
        }
    }

    private void TriggerFunction()
    {
        SoundEffectsManager.Instance.PlaySound("start", 1.5f);
        tracker.EnableParticle();
        Debug.Log("Maze is active");
        gameObject.SetActive(false);
    }
}
