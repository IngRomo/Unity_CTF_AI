/// AIFlagCollector.cs
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/// <summary>
/// Example Agent that feeds ML-Agents outputs into AIInputProvider.
/// </summary>
[RequireComponent(typeof(AIInputProvider))]
public class AIFlagCollector : Agent {
    private AIInputProvider inputProvider;
    private Rigidbody rb;

    public override void Initialize() {
        inputProvider = GetComponent<AIInputProvider>();
        rb = GetComponentInChildren<Rigidbody>();
    }

    public override void OnEpisodeBegin() {
        
    }

    public override void OnActionReceived(ActionBuffers actions) {
        inputProvider.movementInput = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        inputProvider.rotationInput = actions.ContinuousActions[2];
        inputProvider.jumpHeld = actions.DiscreteActions[0] == 1;
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(rb.transform.localPosition);  //* +3
        sensor.AddObservation(rb.transform.forward);        //* +3
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        // TODO: Player controlled
    }

    void OnTriggerEnter(Collider other) {
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
