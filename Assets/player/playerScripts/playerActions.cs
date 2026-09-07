using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class playerActions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem lightningParticles;
    [SerializeField] private InputActionReference shieldAction;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private Slider energySlider; // <-- New field for your UI bar

    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy = 100f;
    [SerializeField] private float drainRate = 20f;       
    [SerializeField] private float movementGainRate = 10f; 

    public bool isShieldActive { get; private set; } = false;

    void OnEnable()
    {
        if (shieldAction != null)
        {
            shieldAction.action.Enable();
            shieldAction.action.performed += OnShieldButtonPressed;
        }
        if (moveAction != null) moveAction.action.Enable();
    }

    void OnDisable()
    {
        if (shieldAction != null)
        {
            shieldAction.action.performed -= OnShieldButtonPressed;
            shieldAction.action.Disable();
        }
        if (moveAction != null) moveAction.action.Disable();
    }

    void Start()
    {
        currentEnergy = maxEnergy;
        if (lightningParticles != null) lightningParticles.Stop();
        UpdateUI();
    }

    void Update()
    {
        HandleEnergyLogic();
        UpdateUI(); // <-- Updates the bar display every single frame
    }

    private void OnShieldButtonPressed(InputAction.CallbackContext context)
    {
        if (!isShieldActive && currentEnergy <= 0) return;
        ToggleShield();
    }

    private void HandleEnergyLogic()
    {
        if (isShieldActive)
        {
            currentEnergy -= drainRate * Time.deltaTime;
            if (currentEnergy <= 0)
            {
                currentEnergy = 0;
                ToggleShield(); 
            }
        }

        if (moveAction != null)
        {
            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude > 0.01f)
            {
                currentEnergy += movementGainRate * Time.deltaTime;
                currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            }
        }
    }

    private void ToggleShield()
    {
        isShieldActive = !isShieldActive;
        if (isShieldActive && lightningParticles != null) lightningParticles.Play();
        else if (!isShieldActive && lightningParticles != null) lightningParticles.Stop();
    }

    // New helper method to refresh the UI value
    private void UpdateUI()
    {
        if (energySlider != null)
        {
            // Divides current by max to give a clean 0.0 to 1.0 percentage
            energySlider.value = currentEnergy / maxEnergy; 
        }
    }
}
