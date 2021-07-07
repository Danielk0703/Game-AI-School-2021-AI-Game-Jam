using UnityEngine;
using UnityEngine.UI;

//namespace Complete
//{
public class BoatHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;               // The amount of health each tank starts with.
    public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
    public Image m_FillImage;                           // The image component of the slider.
    public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
    public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
    public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.


    private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
    private ParticleSystem m_ExplosionParticles;        // The particle system the will play when the tank is destroyed.
    public float m_CurrentHealth;                      // How much health the tank currently has.
    public float m_NormalizedCurrentHealth;            // Normalized CurrentHealth
    private bool m_Dead;                                // Has the tank been reduced beyond zero health yet?

    public BoatAgent m_Agent;


    private void Awake()
    {
        // Instantiate the explosion prefab and get a reference to the particle system on it.
        //m_ExplosionParticles = Instantiate (m_ExplosionPrefab).GetComponent<ParticleSystem> ();

        // Get a reference to the audio source on the instantiated prefab.
        //m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource> ();

        // Disable the prefab so it can be activated when it's required.
        //m_ExplosionParticles.gameObject.SetActive (false);
    }


    public void ResetHealth()
    {
        m_CurrentHealth = m_StartingHealth;
        m_NormalizedCurrentHealth = m_StartingHealth;
        // Normalize
        m_NormalizedCurrentHealth = normalize(m_CurrentHealth);
        m_Dead = false;

        // Update the health slider's value and color.
        SetHealthUI();
    }

    public float GetHealthStatus()
    {
        //return m_CurrentHealth;
        return m_NormalizedCurrentHealth;
    }

    public float normalize(float m_CurrentHealth)
    {
        return ((m_CurrentHealth - 0) / 100);
    }

    public void TakeDamage(float amount)
    {
        // Reduce current health by the amount of damage done.
        m_CurrentHealth -= amount;

        // Give 0.25 to the one not attacked, -0.25 the one receive the boulder (not the best way)
       
        //distributeDamageReward();

        // Change the UI elements appropriately.
        SetHealthUI();

        // If the current health is at or below zero and it has not yet been registered, call OnDeath.
        if (m_CurrentHealth <= 0f && !m_Dead)
        {
            // Normalize
            m_NormalizedCurrentHealth = 0.0f;
            OnDeath();
        }

        // Normalize
        m_NormalizedCurrentHealth = normalize(m_CurrentHealth);
    }

    private void SetHealthUI()
    {
        // Set the slider's value appropriately.
        m_Slider.value = m_CurrentHealth;

        // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }

    private void OnDeath()
    {
        // Set the flag so that this function is only called once.
        m_Dead = true;

        // Call
        m_Agent = gameObject.GetComponent<BoatAgent>();
        m_Agent.beingKilled();
    }

    private void distributeDamageReward()
    {
        // Call
        m_Agent = gameObject.GetComponent<BoatAgent>();
        m_Agent.distributeDamageReward();
    }
}