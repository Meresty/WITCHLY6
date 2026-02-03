using UnityEngine;

public class ScoreOnCollect2D : MonoBehaviour
{
    [SerializeField] private string collectorTag = "Player";
    [SerializeField] private int points = 1;
    [SerializeField] private bool destroyOnCollect = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(collectorTag)) return;

        if (MinigameRunController.Instance != null)
            MinigameRunController.Instance.AddScore(points);

        if (destroyOnCollect)
            Destroy(gameObject);
    }
}
