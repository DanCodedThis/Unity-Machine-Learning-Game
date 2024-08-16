using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class RunFromMoverAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer ownMeshRenderer;

    public override void OnEpisodeBegin()
    {
        transform.localRotation = new Quaternion();
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-0.5f, 4f));
        targetTransform.localPosition = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, -2f));
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float rotateY = actions.ContinuousActions[0];
        float moveSpeed = 0.5f + 2.5f * Mathf.Abs(actions.ContinuousActions[1]);
        float rotateSpeed = 5f;
        transform.Rotate(0f, rotateY * rotateSpeed, 0f);
        transform.localPosition += transform.forward * Time.deltaTime * moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(+1f);
            ownMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        if (other.TryGetComponent<Mover>(out Mover mover))
        {
            SetReward(-1f);
            ownMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}
