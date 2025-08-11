using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private Transform groundTransform;
    [SerializeField] private Transform coinTransform;

    private Transform agentTransform;

    public void SetAgent(Transform agent)
    {
        agentTransform = agent;
    }

    public void spawnCoin()
    {
        if (coinTransform == null || groundTransform == null || agentTransform == null) return;

        Vector3 center = groundTransform.localPosition;
        Vector3 size = groundTransform.localScale;

        float halfX = size.x * 0.5f;
        float halfZ = size.z * 0.5f;

        float coinY = center.y + size.y * 0.5f + 0.5f;

        Vector3 randomPos;
        int tries = 0;
        const int maxTries = 20;

        do
        {
            float randX = Random.Range(-halfX, halfX);
            float randZ = Random.Range(-halfZ, halfZ);
            randomPos = new Vector3(randX, coinY, randZ);
            tries++;
        }
        while (Vector3.Distance(randomPos, agentTransform.localPosition) < 1.0f && tries < maxTries);

        coinTransform.localPosition = randomPos;
    }
}
