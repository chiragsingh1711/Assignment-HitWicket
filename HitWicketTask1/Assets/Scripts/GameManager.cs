using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject doofusPrefab;
    public GameObject pulpitPrefab;
    public TextMeshProUGUI  scoreText;
    // public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public GameObject startPanel;
    public Canvas worldSpaceCanvas;

    public Camera mainCamera;

    private GameObject currentPulpit;
    private GameObject nextPulpit;
    private GameObject doofus;
    private Rigidbody doofusRb;
    private float pulpitDestroyTime;
    private float pulpitSpawnTime;
    private float playerSpeed;
    private int score = 0;
    private bool isGameOver = false;
    private bool isGameStarted = false;

    [System.Serializable]
    private class DoofusDiary
    {
        public PlayerData player_data;
        public PulpitData pulpit_data;
    }

    [System.Serializable]
    private class PlayerData
    {
        public float speed;
    }

    [System.Serializable]
    private class PulpitData
    {
        public float min_pulpit_destroy_time;
        public float max_pulpit_destroy_time;
        public float pulpit_spawn_time;
    }

    void Start()
    {
        LoadDoofusDiary();
        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);

        if (scoreText == null)
        {
            Debug.LogError("ScoreText is not assigned in the Inspector. Please assign it.");
        }
        else
        {
            // UpdateScoreUI(); // Initialize the score display
        }
    }

    void LoadDoofusDiary()
    {
        string jsonFilePath = Path.Combine(Application.streamingAssetsPath, "doofus_diary.json");
        if (File.Exists(jsonFilePath))
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            DoofusDiary diary = JsonUtility.FromJson<DoofusDiary>(jsonContent);

            playerSpeed = diary.player_data.speed;
            pulpitSpawnTime = diary.pulpit_data.pulpit_spawn_time;
            pulpitDestroyTime = Random.Range(diary.pulpit_data.min_pulpit_destroy_time, diary.pulpit_data.max_pulpit_destroy_time);
        }
        else
        {
            Debug.LogError("Doofus Diary JSON file not found!");
        }
    }

    public void StartGame()
    {
        isGameStarted = true;
        startPanel.SetActive(false);
        SpawnDoofus();
        SpawnInitialPulpit();
        score = 0;
        // UpdateScoreUI(); 
        StartCoroutine(SpawnNextPulpit());
    }

    void SpawnDoofus()
    {
        doofus = Instantiate(doofusPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        doofusRb = doofus.GetComponent<Rigidbody>();
    }


    void FindScoreText()
    {
        scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        if (scoreText == null)
        {
            Debug.LogError("Could not find ScoreText in the scene. Please ensure it exists and has a TextMeshProUGUI component.");
        }
    }

    void SpawnInitialPulpit()
    {
        currentPulpit = SpawnPulpit(Vector3.zero);
    }

    GameObject SpawnPulpit(Vector3 position)
    {
        GameObject pulpit = Instantiate(pulpitPrefab, position, Quaternion.identity);
        Pulpit pulpitScript = pulpit.AddComponent<Pulpit>();
        
        // Create a world space UI Text for the timer
        GameObject timerObj = new GameObject("TimerText");
        timerObj.transform.SetParent(worldSpaceCanvas.transform);
        TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();
        timerText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF"); 
        timerText.fontSize = 20;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = Color.white;

        // Position the timer above the pulpit
        timerObj.transform.position = position + new Vector3(0, 2, 0);
        timerObj.transform.rotation = Quaternion.Euler(90, 0, 0);

        pulpitScript.timerText = timerText;
        pulpitScript.Initialize(pulpitDestroyTime, this);

        Debug.Log($"Pulpit spawned at {position}. Destroy time: {pulpitDestroyTime}");

        return pulpit;
    }

    IEnumerator SpawnNextPulpit()
    {
        while (!isGameOver)
        {
            Debug.Log($"Waiting to spawn next pulpit. Current pulpit: {currentPulpit}, Next pulpit: {nextPulpit}");
            yield return new WaitForSeconds(pulpitDestroyTime - pulpitSpawnTime);

            Debug.Log("Spawning next pulpit");
            Vector3 spawnPosition = GetNextPulpitPosition();
            nextPulpit = SpawnPulpit(spawnPosition);
            Debug.Log($"Next pulpit spawned at {spawnPosition}");

            yield return new WaitForSeconds(pulpitSpawnTime);
            
            Debug.Log("Incrementing score and updating pulpits");
            score++;

            if (currentPulpit == null && nextPulpit != null)
            {
                currentPulpit = nextPulpit;
                nextPulpit = null;
            }

            // Check if the player is on the current pulpit
            if (doofus != null && currentPulpit != null)
            {
                float distance = Vector3.Distance(doofus.transform.position, currentPulpit.transform.position);
                if (distance > 5f) 
                {
                    Debug.Log("Player not on current pulpit. Game over.");
                    GameOver();
                    yield break;
                }
            }
        }
    }

    Vector3 GetNextPulpitPosition()
    {
        Vector3[] possiblePositions = new Vector3[]
        {
            new Vector3(9, 0, 0),   // Right
            new Vector3(-9, 0, 0),  // Left
            new Vector3(0, 0, 9),   // Forward
            new Vector3(0, 0, -9)   // Backward
        };

        Vector3 basePosition = currentPulpit != null ? currentPulpit.transform.position : Vector3.zero;
        Debug.Log($"Getting next pulpit position. Current pulpit: {currentPulpit}, Base position: {basePosition}");

        return basePosition + possiblePositions[Random.Range(0, possiblePositions.Length)];
        // return currentPulpit.transform.position + possiblePositions[Random.Range(0, possiblePositions.Length)];
    }

    public void PulpitDestroyed(Pulpit destroyedPulpit)
    {
        Debug.Log($"PulpitDestroyed called. Destroyed pulpit: {destroyedPulpit}, Current pulpit: {currentPulpit}, Next pulpit: {nextPulpit}");
        if (destroyedPulpit.gameObject == currentPulpit)
        {
            Debug.Log("Destroyed pulpit was the current pulpit. Updating references.");
            if (nextPulpit != null)
            {
                currentPulpit = nextPulpit;
                nextPulpit = null;
            }
            else
            {
                Debug.LogWarning("Next pulpit is null. Setting current pulpit to null.");
                currentPulpit = null;
            }
        }
        else
        {
            Debug.LogWarning("Destroyed pulpit was not the current pulpit.");
        }
    }

    void Update()
    {
        if (scoreText == null)
        {
            Debug.LogWarning("ScoreText is null in Update. Attempting to find it.");
            FindScoreText();
        }

        if (isGameStarted && !isGameOver)
        {
            MoveDoofus();
            CheckDoofusFall();
        }
    }

    void MoveDoofus()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Move the player
        doofus.transform.Translate(Vector3.right * moveHorizontal * Time.deltaTime * 10);
        doofus.transform.Translate(Vector3.forward * moveVertical * Time.deltaTime * 10);

        mainCamera.transform.position = doofus.transform.position + new Vector3(0, 5, -7);
    }

    void CheckDoofusFall()
    {
        if (doofus.transform.position.y < -5)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
    }

    void UpdateScoreUI()
    {
        Debug.Log("Updating Score UI. Current score: " + score);
        if (scoreText != null)
        {
            Debug.Log("ScoreText is not null, updating text");
            scoreText.text = "Score: " + score;
        }
        else
        {
            Debug.LogError("ScoreText is null in UpdateScoreUI. Attempting to find it.");
            FindScoreText();
            if (scoreText != null)
            {
                scoreText.text = "Score: " + score;
            }
            else
            {
                Debug.LogError("Failed to find ScoreText. Please ensure it exists in the scene.");
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}