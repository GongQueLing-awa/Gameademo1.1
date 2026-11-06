using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatBar : MonoBehaviour
{
    public Image healthImage;

    public void OnHealthChange(float persentage)
    {
        healthImage.fillAmount = persentage;
    }



}
