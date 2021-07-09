using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonballExplosion : MonoBehaviour
{
    public LayerMask m_BoatMask;                        // Used to filter what the explosion affects, this should be set to "Players".
    public ParticleSystem m_ExplosionParticles;         // Reference to the particles that will play on explosion.
    public AudioSource m_ExplosionAudio;                // Reference to the audio that will play on explosion.
    public float m_MaxDamage = 100f;                    // The amount of damage done if the explosion is centred on a tank.
    public float m_ExplosionForce = 1000f;              // The amount of force added to a tank at the centre of the explosion.
    public float m_MaxLifeTime = 2f;                    // The time in seconds before the shell is removed.
    public float m_ExplosionRadius = 5f;                // The maximum distance away from the explosion tanks can be and are still affected.
    public BoatAgent m_AgentLauncher;                   // The Agent who launched this shell (used to calculate the reward

    public TrailRenderer m_trailRenderer;
    public ParticleSystem m_destroyParicles;

    private void Start()
    {
        // If it isn't destroyed by then, destroy the shell after it's lifetime.
        Destroy(gameObject, m_MaxLifeTime);
    }


    private void OnTriggerEnter(Collider collision)
    {
        Rigidbody targetRigidbody = collision.GetComponent<Rigidbody>();
        if (targetRigidbody)
        {
            // Find the BoatHealth script associated with the rigidbody.
            BoatHealth targetHealth = targetRigidbody.GetComponent<BoatHealth>();

            // If there is no BoatHealth script attached to the gameobject, go on to the next collider.
            if (targetHealth)
            {
                // Deal this damage to the tank.
                targetHealth.TakeDamage(50f);
                m_AgentLauncher.AddReward(20f);
                // Unparent the particles from the shell.
                m_ExplosionParticles.transform.parent = null;                

                // Play the particle system.
                m_ExplosionParticles.Play();

                // Once the particles have finished, destroy the gameobject they are on.
                ParticleSystem.MainModule mainModule = m_ExplosionParticles.main;
                Destroy(m_ExplosionParticles.gameObject, mainModule.duration);
            }
            else
            {
                m_AgentLauncher.AddReward(-5f);
            }

            // Destroy the shell.
            Destroy(gameObject);
        }
        else if (!targetRigidbody)
        {
            // Destroy the cannonball
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        m_destroyParicles.transform.parent = null;
        m_destroyParicles.Play();
        Destroy(m_destroyParicles.gameObject, m_destroyParicles.main.duration);

        m_trailRenderer.transform.parent = null;
        Destroy(m_trailRenderer.gameObject, m_trailRenderer.time);
    }
        

}

