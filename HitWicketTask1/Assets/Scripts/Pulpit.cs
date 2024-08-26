using System.Collections;
using TMPro;
using UnityEngine;

public class Pulpit : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float destroyTime;
    private GameManager gameManager;

    public void Initialize(float time, GameManager manager)
    {
        destroyTime = time;
        gameManager = manager;
        Debug.Log($"Pulpit initialized with destroy time: {destroyTime}");
        StartCoroutine(CountdownTimer());
    }

    private IEnumerator CountdownTimer()
    {
        while (destroyTime > 0)
        {
            destroyTime -= Time.deltaTime;
            UpdateTimerDisplay();
            yield return null;
        }
        Debug.Log($"Pulpit {gameObject.name} countdown finished. Calling PulpitDestroyed.");
        gameManager.PulpitDestroyed(this);
        Destroy(gameObject);
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = destroyTime.ToString("F1");
        }
    }
}