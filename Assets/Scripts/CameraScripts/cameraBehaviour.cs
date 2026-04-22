using UnityEngine;

public class cameraBehaviour : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject cameraAlert;
    [SerializeField] LayerMask target;

    [SerializeField] float distance;
    [SerializeField] float angle;
    private float realAngle;
    [SerializeField] LayerMask walls;

    void Start()
    {
        
        realAngle = angle / 2;
    }


    private void Update()
    {
        cameraAlert.SetActive(Sigth());

        if (cameraAlert.activeSelf == true)
        {
            followPlayer();
        }
    }

    private void followPlayer()
    {
        var dir = player.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
        
    }
    private bool Sigth()
    {
        var dir = player.transform.position-transform.position;
        if(dir.magnitude > distance) return false;
        if (Vector3.Angle(transform.forward, dir) > realAngle) return false;
        if(Physics.Raycast(transform.position, dir.normalized, dir.magnitude, walls)) return false;
        return true;
    }

}
