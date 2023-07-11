using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class PlayerMover : MonoBehaviour
{
    [SerializeField]
    private TimeCounter _TimeCounter;

    private float moveSpeed = 10;

    // Start is called before the first frame update
    void Start()
    {
        _TimeCounter.OnTimeChanged.Where(x => x == 0).Subscribe(_ =>
        {
            transform.position = Vector3.zero;
        }).AddTo(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += new Vector3(1, 0, 0) * moveSpeed * Time.deltaTime;
        }

        if (transform.position.x > 10)
            Destroy(gameObject);
    }
}