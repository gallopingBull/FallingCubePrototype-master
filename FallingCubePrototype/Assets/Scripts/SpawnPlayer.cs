using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{

    [SerializeField] private float m_MaxDistance = 30;
    private bool HitDetect;
    private bool m_HitDetect;

    public float BoxColliderSize = 30;
    private Collider m_Collider;
    private RaycastHit m_Hit;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
