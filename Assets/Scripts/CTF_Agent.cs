using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.Rendering;

public class CTF_Agent : Agent
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform flag;

    [SerializeField] private SpawnManager agentSpawnPlataform;
    [SerializeField] private FlagSpawner flagSpawnPlataform;

    [SerializeField] private float moveSpeed = 3f;

    private float gravityMultiplier = 2f;
    private float distance;
    private float minDistance;

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        //* Snappier gravity application
        rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        distance = Vector3.Distance(rb.transform.localPosition, flag.localPosition);
        if (distance >= minDistance)
        { //* If further than previous update or not moved
            AddReward(-0.01f);
        }
        else
        {
            minDistance = distance;
        }
    }

    void spawnAgent()
    {
        Vector3 spawnPos = agentSpawnPlataform.GetRandomPointOnSurface();
        transform.position = spawnPos;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void spawnFlag()
    {
        Vector3 spawnPos = flagSpawnPlataform.GetRandomPointOnSurface();
        flag.position = spawnPos;
    }

    public override void OnEpisodeBegin()
    {
        spawnAgent();
        spawnFlag();

        minDistance = Vector3.Distance(rb.transform.localPosition, flag.localPosition);

        Debug.Log("Finished episode");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rb.transform.localPosition);  //* +3 (3)
        sensor.AddObservation(flag.localPosition);          //* +3 (6)
        sensor.AddObservation(distance);                    //* +1 (7)
        sensor.AddObservation(rb.transform.forward);        //* +3 (10)
        sensor.AddObservation(minDistance);                 //* +1 (11)
        Debug.Log("Obs size = " + sensor.ObservationSize());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotateY = actions.ContinuousActions[2];

        //* Apply movement
        Vector3 movement = new Vector3(moveX, 0, moveZ);
        Vector3 localMovement = transform.TransformDirection(movement) * moveSpeed;
        rb.velocity = new Vector3(localMovement.x, rb.velocity.y, localMovement.z);

        //* Apply rotation (Y-axis / yaw)
        float rotationSpeed = 100f; //* Degrees per second
        transform.Rotate(0f, rotateY * rotationSpeed * Time.fixedDeltaTime, 0f);

        //* Rewards
        AddReward(-0.01f); //* Punish over time
        AddReward(-0.005f * distance); //* Punish distance
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        continuousActions[2] = Input.GetKey(KeyCode.Q) ? -1f : Input.GetKey(KeyCode.E) ? 1f : 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        switch (other.name) {
            case "Flag":
                AddReward(100.0f);
                EndEpisode();
                break;
            case "Void":
                AddReward(-10.0f);
                EndEpisode();
                break;
            default:
                Debug.Log("Entered unknown");
                break;
        }
    }
}
