using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    IMove _move;
    ISpin _spin;
    void Start()
    {
        _move = GetComponent<IMove>();
        _spin = GetComponent<ISpin>();
    }
    void Update()
    {
        if (_spin.IsDetectable)
        {
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");

            Vector3 dir = new Vector3(h, 0, v);
            _move.Move(dir.normalized);
            if (h != 0 || v != 0) _move.Look(dir);
        }
        if (Input.GetKeyDown(KeyCode.Space)) _spin.Spin();
    }
}
