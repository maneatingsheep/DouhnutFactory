using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sparkle : MonoBehaviour
{

    internal Collider DonutCollider;
    internal Collider FloorCollider;

    Vector3 _rotation;
    float _speed;
    bool _isFalling = true;

    void Start()
    {
        transform.rotation = Quaternion.Euler(Random.value * 360, Random.value * 360, Random.value * 360);
        _rotation = new Vector3(Random.value * 360, Random.value * 360, Random.value * 360);

        _speed = Random.Range(1.5f, 1.8f);

        
    }

    public void UpdateColor(Color c) {
        GetComponent<MeshRenderer>().material.color = c;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isFalling) return;

        transform.position = new Vector3(transform.position.x, transform.position.y - _speed * Time.deltaTime, transform.position.z);
        transform.Rotate(_rotation * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if (other == DonutCollider || other == FloorCollider) {
            _isFalling = false;
            transform.parent = other.transform;

        }
    }
}
