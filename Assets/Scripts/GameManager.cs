using UnityEngine;

/// <summary>
/// Game manager singleton class.
/// </summary>
public class GameManager : MonoBehaviour
{
    static GameManager _Instance = null;
    public static GameManager Instance => _Instance;

    void Start()
    {
        if (_Instance != null)
        {
            Debug.LogError("Duplicate game manager!!");
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (_Instance == this)
            _Instance = null;
    }

    void Update()
    {
        
    }
}
