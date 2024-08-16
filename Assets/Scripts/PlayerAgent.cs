using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class PlayerAgent : Agent
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private Transform boulder;
    [SerializeField] private Transform boulder1;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    private Rigidbody rb;
    private float force = 500f;
    public override void OnEpisodeBegin()
    {
        rb = GetComponent<Rigidbody>();
        transform.localPosition = new Vector3(-20f, -22.5f, 3.5f);
        boulder.localPosition = new Vector3(0.6f, -4f, boulder.gameObject.GetComponent<Boulder>().RandomPosition());
        boulder.gameObject.GetComponent<Rigidbody>().velocity = -boulder.up * Time.deltaTime * force / 2;
        boulder1.localPosition = new Vector3(0.6f, -4f, boulder.gameObject.GetComponent<Boulder>().RandomPosition());
        boulder1.gameObject.GetComponent<Rigidbody>().velocity = -boulder1.up * Time.deltaTime * force / 2;
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        rb.velocity = transform.right * Time.deltaTime * force;
        if (transform.localPosition.z <= 6.5 && transform.localPosition.z >= 0.5)
        {
            transform.localPosition += transform.forward * Time.deltaTime * speed * actions.ContinuousActions[0];
        }
        if (transform.localPosition.z > 6.5)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 6.5f);
        }
        if (transform.localPosition.z < 0.5)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0.5f);
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = -Input.GetAxisRaw("Horizontal");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Finish finish))
        {
            SetReward(+1f);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        if (other.TryGetComponent(out Boulder boulder))
        {
            SetReward(-1f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}
