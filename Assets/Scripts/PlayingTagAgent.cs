using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Unity.MLAgents.Policies;
using UnityEditor;

public class PlayingTagAgent : Agent
{
    private static List<GameObject> players = new List<GameObject>();
    private static int activePlayers = 0;
    private static List<int> activePlayerIds = new List<int>();
    [HideInInspector] public bool isTagger;
    private static bool isFirstTime = true;
    private float cooldownTime;
    private float destroyTime;
    private float moveSpeed;
    private float rotateSpeed;
    private BehaviorParameters bp;
    [SerializeField] private Material runnerMaterial;
    [SerializeField] private Material taggerMaterial;
    [SerializeField] public MeshRenderer ownMeshRenderer;
    public override void OnEpisodeBegin()
    {
        if (isFirstTime)
        {
            players.Add(gameObject);
        }
        isTagger = false;
        cooldownTime = 0f;
        destroyTime = 5f;
        moveSpeed = 2.5f;
        rotateSpeed = 5f;
        activePlayers++;
        bp = gameObject.GetComponent<BehaviorParameters>();
        bp.TeamId = activePlayers;
        activePlayerIds.Add(bp.TeamId - 1);
        gameObject.AddComponent<Runner>();
        gameObject.AddComponent<Tagger>();
        gameObject.GetComponent<Tagger>().enabled = false;
        gameObject.tag = "Runner";
        ownMeshRenderer.material = runnerMaterial;
        if (activePlayers == 10)
        {
            isFirstTime = false;
            int random = Random.Range(0, 10);
            players[random].GetComponent<Runner>().enabled = false;
            players[random].GetComponent<PlayingTagAgent>().isTagger = true;
            players[random].GetComponent<Tagger>().enabled = true;
            players[random].tag = "Tagger";
            players[random].GetComponent<PlayingTagAgent>().ownMeshRenderer.material = taggerMaterial;
        }
        transform.localPosition = new Vector3(Random.Range(-2f, 22f), 0f, Random.Range(-1f, 26f));
        transform.localRotation = Quaternion.identity;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(isTagger);
        sensor.AddObservation(cooldownTime);
        sensor.AddObservation(destroyTime);
        sensor.AddObservation(activePlayers);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        if(gameObject.activeSelf == true)
        {
            transform.Rotate(0f, actions.ContinuousActions[0] * rotateSpeed, 0f);
            float fullMoveSpeed;
            if (isTagger)
            {
                fullMoveSpeed = 0.5f + moveSpeed;
                if (actions.DiscreteActions[0] == 1 && cooldownTime <= 0f)
                {
                    cooldownTime = 5f;
                }
                if (cooldownTime > 3f)
                {
                    fullMoveSpeed *= 2f;
                }
                if (destroyTime > 0)
                {
                    destroyTime -= Time.deltaTime;
                }
                else
                {
                    AddReward(-100f);

                    activePlayers--;
                    activePlayerIds.Remove(bp.TeamId - 1);
                    if (activePlayers > 1)
                    {
                        int random = activePlayerIds[Random.Range(0, activePlayerIds.Count)];
                        players[random].GetComponent<Runner>().enabled = false;
                        players[random].GetComponent<PlayingTagAgent>().isTagger = true;
                        players[random].GetComponent<PlayingTagAgent>().ownMeshRenderer.material = taggerMaterial;
                        players[random].GetComponent<Tagger>().enabled = true;
                        players[random].tag = "Tagger";
                    }

                    gameObject.SetActive(false);
                }
            }
            else
            {
                fullMoveSpeed = 0.5f + moveSpeed / 2;
                if (actions.DiscreteActions[0] == 1 && cooldownTime <= 0f)
                {
                    cooldownTime = 10f;
                }
                if (cooldownTime > 8f)
                {
                    fullMoveSpeed *= 2f;
                }
            }
            transform.localPosition += transform.forward * Time.deltaTime * fullMoveSpeed;
            if (cooldownTime > 0)
            {
                cooldownTime -= Time.deltaTime;
            }
            if (activePlayers == 1)
            {
                AddReward(+100f);
                activePlayerIds.Clear();
                activePlayers = 0;
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].SetActive(false);
                    players[i].SetActive(true);
                }

            }
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        discreteActions[0] = (int)Mathf.Abs(Mathf.Floor(Input.GetAxisRaw("Vertical")));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isTagger)
        {
            if (other.TryGetComponent(out Wall wall))
            {
                AddReward(-1f);
            }
        }
        else
        {
            if (other.TryGetComponent(out Tagger tagger) && tagger.enabled == true)
            {
                AddReward(-10f);
                ownMeshRenderer.material = taggerMaterial;
                isTagger = true;
                gameObject.GetComponent<Tagger>().enabled = true;
                gameObject.tag = "Tagger";
                gameObject.GetComponent<Runner>().enabled = false;
                cooldownTime = 0f;
                destroyTime = 5f;
                other.gameObject.GetComponent<PlayingTagAgent>().AddReward(+10f);
                other.gameObject.GetComponent<PlayingTagAgent>().ownMeshRenderer.material = runnerMaterial;
                other.gameObject.GetComponent<PlayingTagAgent>().isTagger = false;
                other.gameObject.GetComponent<Runner>().enabled = true;
                other.gameObject.tag = "Runner";
                other.gameObject.GetComponent<Tagger>().enabled = false;
                other.gameObject.GetComponent<PlayingTagAgent>().cooldownTime = 0f;
                other.gameObject.GetComponent<PlayingTagAgent>().destroyTime = 5f;
                bool realValue = other.isTrigger;
                other.isTrigger = !realValue;
                other.isTrigger = realValue;
            }
            if (other.TryGetComponent(out Runner runner) && runner.enabled == true)
            {
                AddReward(-1f);
            }
            if (other.TryGetComponent(out Wall wall))
            {
                AddReward(-1f);
            }
        }
    }
}
