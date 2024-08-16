using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    private static List<float> positions = new List<float>();
    // Start is called before the first frame update
    void Start()
    {
        Positions();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localPosition.x > 0.6f)
        {
            transform.localPosition = new Vector3(0.6f, transform.localPosition.y, transform.localPosition.z);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out End end)) {
            transform.localPosition = new Vector3(0.6f, -4f, RandomPosition());
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
    public float RandomPosition()
    {
        float random = positions[Random.Range(0, positions.Count)];
        positions.Remove(random);
        if (positions.Count == 1)
        {
            Positions();
        }
        return random;
    }
    private void Positions()
    {
        positions.Clear();
        float pos = 0.5f;
        for (int i = 0; i < 3; i++)
        {
            positions.Add(pos);
            pos += 3f;
        }
    }
}
