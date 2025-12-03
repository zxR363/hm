using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotUIManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject characterCreationPanel;
    [SerializeField] private GameObject characterSlotPanel;

    [Header("Slot References")]
    [SerializeField] private Transform allSlots;
    [SerializeField] private GameObject deleteButton;

    public void OnConfirm()
    {
        StartCoroutine(HandlePanelSwitch());
    }

    private IEnumerator HandlePanelSwitch()
    {
        // 🔄 Panel geçişi
        characterCreationPanel.SetActive(false);
        characterSlotPanel.SetActive(true);

        yield return new WaitForEndOfFrame(); // prefab ve layout tamamlansın

        RevealDeleteButton();
        FadeInAllSlotComponents();
    }

    private void RevealDeleteButton()
    {
        if (deleteButton == null) return;

        deleteButton.SetActive(true);

        CanvasGroup cg = deleteButton.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = deleteButton.AddComponent<CanvasGroup>();

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void FadeInAllSlotComponents()
    {
        List<CanvasGroup> fadeTargets = new List<CanvasGroup>();

        Transform[] allChildren = characterSlotPanel.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = child.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }

            fadeTargets.Add(cg);
        }

        StartCoroutine(FadeInAllAtOnce(fadeTargets, 0.8f));
    }

    private IEnumerator FadeInAllAtOnce(List<CanvasGroup> targets, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            float alpha = time / duration;
            foreach (CanvasGroup cg in targets)
            {
                cg.alpha = alpha;
            }
            time += Time.deltaTime;
            yield return null;
        }

        foreach (CanvasGroup cg in targets)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }
}