using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // сохраняем объект между сценами
        }
        else
        {
            Destroy(gameObject); // если объект уже есть, удаляем дубликат
        }
    }
}