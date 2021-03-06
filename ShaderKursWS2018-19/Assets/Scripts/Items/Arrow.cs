﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    [SerializeField]
    [Tooltip("The max speed the arrow moves forward.")]
    float maxSpeed = 5;

    [Space]
    [SerializeField]
    [Tooltip("Particle System that is played when the arrow hits.")]
    ParticleSystem hit;


    Rigidbody rigid;
    GameObject mesh;

    public int Lifetime { get; private set; }   // despawns when lifetime is zero, decreases when leaving the room


    //---------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------------------------------------//
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        mesh = transform.GetChild(0).gameObject;
    }

    private void LateUpdate()
    {
        if(rigid)
        transform.rotation = Quaternion.LookRotation(rigid.velocity, Vector3.up);

        if(transform.position.y < -1)
        {
            Lifetime = 0;
            DeactivateArrow();
        }
    }

    // called when leaving the room
    public void DecreaseLifetime()
    {
        Lifetime--;

        if (Lifetime <= 0)
        {
            DeactivateArrow();
        }
    }

    public void Shoot(Transform origin, float strength)
    {
        Lifetime = 2;

        mesh.SetActive(true);

        transform.position = origin.position;

        rigid.velocity = origin.forward * maxSpeed * strength;
    }

    private void OnTriggerEnter(Collider other)
    {
        Hit();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Hit();
    }

    void Hit()
    {
        Lifetime = 0;
        DeactivateArrow();

        // hit effect
        hit.transform.position = transform.position + Vector3.up * .5f;
        hit.Emit(1);
        hit.GetComponent<AudioSource>().Play();
    }

    public void DeactivateArrow()
    {
        mesh.SetActive(false);
    }

    public bool Ready()
    {
        return !mesh.activeInHierarchy;
    }
}
