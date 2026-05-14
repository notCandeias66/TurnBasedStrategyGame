using UnityEngine;

public class Arrow : Throwable
{
    private void Awake()
    {
        speed = 1f;
        height = 0.005f;
        progress = 0f;
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        progress += Time.deltaTime * speed;
        if (progress > 1f)
        {
            Destroy(gameObject); // Destroy arrow when it reaches the target
            return;
        }

        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
        currentPos.y += height * Mathf.Sin(Mathf.PI * progress); // Add a ballistic arc
        transform.position = currentPos;

        Vector3 direction = (targetPosition - startPosition).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
