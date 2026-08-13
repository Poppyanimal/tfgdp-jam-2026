using TMPro;
using UnityEngine;
using ge = GlobalEvents;
using pv = PlayerVars;

public class playerUI : MonoBehaviour
{
    public Animator anims;
    public TMP_Text ammoText, ammoTextBG;
    public GameObject healthpercentobj;
    public float healthmaxY, healthminY;

    public float debugHealth = 1f;


    void Start()
    {
        ge.get().playerHurt.AddListener(updateHealth);
        ge.get().playerHeal.AddListener(updateHealth);
        ge.get().ammoChanged.AddListener(updateAmmo);
    }

    public void updateHealth()
    {
        float ratio = pv.get().getHealthRatio();
        if(ratio <= 0)
            anims.SetBool("showHealth", false);
        else
            anims.SetBool("showHealth", true);

        Vector3 pos = healthpercentobj.transform.localPosition;
        pos.y = healthminY + (healthmaxY - healthminY) * ratio;
        healthpercentobj.transform.localPosition = pos;
    }

    [ContextMenu("debug Health")]
    public void debugUpdateHealth()
    {
        float ratio = debugHealth / 5f;
        if(ratio <= 0)
            anims.SetBool("showHealth", false);
        else
            anims.SetBool("showHealth", true);

        Vector3 pos = healthpercentobj.transform.localPosition;
        pos.y = healthminY + (healthmaxY - healthminY) * ratio;
        healthpercentobj.transform.localPosition = pos;
    }

    public void updateAmmo()
    {
        int a = pv.get().getAmmo();
        if(a > 0)
            anims.SetBool("showAmmo", true);
        anims.SetTrigger("ammoChanged");
        ammoText.text = a.ToString();
        ammoTextBG.text = a.ToString();
    }
}
