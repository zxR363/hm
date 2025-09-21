using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FridgeController : MonoBehaviour
{
    public Animator fridgeAnimator;

    //nesnelerin ilk olusturulurken ki startFlag
    //yoksa ilk olayda gözükmüyor
    private bool objectStartFlag = false; 
    
    private bool isOpen = false;
    private bool hasShownFood = false;
    private bool hasHiddenFood = false;


    public List<GameObject> foodPrefabs;
    public Transform spawnParent;
    public List<FoodItemController> spawnedFoods = new List<FoodItemController>();

    void Update()
    {
        //AnimatorStateInfo state = fridgeAnimator.GetCurrentAnimatorStateInfo(0);

        //if (state.IsName("Open_Fridge") && state.normalizedTime >= 1f && !hasShownFood)
        //{
        //    ShowFoodAfterAnimation();
        //    hasShownFood = true;
        //    hasHiddenFood = false;
        //}

        //if (state.IsName("Idle_Fridge") && state.normalizedTime >= 1f && !hasHiddenFood)
        //{
        //    HideFoodAfterAnimation();
        //    hasHiddenFood = true;
        //    hasShownFood = false;
        //}
    }


    public void ToggleFridge()
    {
        animationFridgeIsOpen();

    }

    private void animationFridgeIsOpen()
    {
        if (fridgeAnimator != null)
        {
            isOpen = fridgeAnimator.GetBool("IsOpen");
            fridgeAnimator.SetBool("IsOpen", !isOpen);
        }
    }


    private void ShowFoodAfterAnimation()
    {
        if (spawnedFoods.Count == 0)
        {
            foreach (var prefab in foodPrefabs)
            {
                GameObject food = Instantiate(prefab, spawnParent);
                var controller = food.GetComponent<FoodItemController>();
                if (controller != null)
                {
                    spawnedFoods.Add(controller);
                    controller.ShowFully();
                    Debug.Log("SHOW BURASI CALISIYOR");
                }
            }
        }
        else
        {
            foreach (var item in spawnedFoods)
            {
                item.ShowFully();
            }
        }
    }

    private void HideFoodAfterAnimation()
    {
        foreach (var item in spawnedFoods)
        {
            item.HideCompletely();
        }
    }


    //Animation'da AnimationEvent ile tetikletme sağladık
    //Bu sayede bloklanma oluşmadan görünür yaptık
    public void OnFridgeOpened()
    {
        if(objectStartFlag == false)
        {
            ShowFoodAfterAnimation();
            objectStartFlag = true;
        }

        if (hasShownFood == false)
        {
            ShowFoodAfterAnimation();
            hasShownFood = true;

        }
    }

    //Animation'da AnimationEvent ile tetikletme sağladık
    //Bu sayede bloklanma oluşmadan görünür yaptık
    public void OnFridgeClosing()
    {
        if(hasShownFood == true)
        {
            foreach (var item in spawnedFoods)
            {
                item.HideCompletely(); // Anında gizle
            }

            hasHiddenFood = true;
            hasShownFood = false;

            Debug.Log("OnFridgeClosing tetiklendi, yiyecekler gizlendi.");
        }
    }


}
