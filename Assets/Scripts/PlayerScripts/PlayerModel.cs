using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour, IMove, ISpin
{
    Rigidbody _rb;
    public float speed;
    bool _isDetectable = true;

    ////()=>()
    //public delegate void MyDelegate();
    //public delegate void MyDelegate2();
    //public MyDelegate OnSpin;
    //public Func<float> OnSpin3;

    Action _onSpin = delegate { };

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    //M: Model
    //V: View
    //C: Controller

    public void Test()
    {

    }
    public void Move(Vector3 dir)
    {
        dir = dir.normalized;
        dir *= speed;
        dir.y = _rb.linearVelocity.y;
        _rb.linearVelocity = dir;
    }
    public void Look(Vector3 dir)
    {
        transform.forward = dir;
    }
    public void Look(Transform target)
    {
        //A->B
        //B-A
        //A: Yo
        //B: Target
        Vector3 dir = target.position - transform.position;
        Look(dir);
    }
    public void Spin()
    {
        _isDetectable = !_isDetectable;
        _onSpin();
    }
    public bool IsDetectable => _isDetectable;

    Action ISpin.OnSpin { get => _onSpin; set => _onSpin = value; }
}
