using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTRadialMenuOption : BTDaisyWheel
{
    BTDaisyWheel DaisyWheel;

    GameObject optionObject;
    Image mainImage;
    Image childImage;
    Transform childTransform;

    public int optionNum;

    float animationDurration;

    Coroutine animateCoroutine;


    public enum OptionStates
    {
        Hidden,
        Unselected,
        Selected,
        Length     //leave at end
    };


    public void Setup(BTDaisyWheel btDaisyWheel, int _optionNum)
    {
        optionNum = _optionNum;
        optionObject = this.gameObject;

        //optionObject.AddComponent<Image>();
        //GameObject optionIcon = Instantiate(btDaisyWheel.optionIcons[optionNum], optionObject.transform, false);
    }

    public void SetState(OptionStates state, bool animated)
    {

        if (!animated)
        {
            //set values directly
            return;
        }

        if (animateCoroutine != null)
        {
            StopCoroutine(animateCoroutine);
        }

        animateCoroutine = StartCoroutine(AnimateOption());
    }


    private IEnumerator AnimateOption()
    {
        float time = 0;

        float start = 0f;
        float target = 1f;

        while (time < animationDurration)
        {
            time += Time.deltaTime;
            float progress = time / animationDurration;

            float bgAlpha = Mathf.Lerp(start, target, progress);

            yield return null;
        }

        // Ensure the final alpha is exactly the target

        Debug.Log($"finished");
    }
}
