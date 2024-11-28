using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatController : CharacterStatController
{

    [SerializeField] RectTransform HealthBar;
    [SerializeField] RectTransform StaminaBar;
    [SerializeField] RectTransform ManaBar;
    void Start()
    {
        base.Start();
    }
    void Update()
    {
        UpdateStats();

        UpdateCanvas();
    }
    private void UpdateCanvas()
    {
        HealthBar.localScale = new Vector3(Mathf.Clamp((Health / baseHealth), 0, 1), 1, 1);
        StaminaBar.localScale = new Vector3(Mathf.Clamp((Stamina / baseStamina), 0, 1), 1, 1);
    }
}
