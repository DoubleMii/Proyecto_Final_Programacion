using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveMenuController : MonoBehaviour
{
    public Button[] slotButtons; 
    public Button saveButton;
    public Button loadButton;
    public Button newGameButton;
    public TextMeshProUGUI statusText;

    private void Start()
    {
        if (saveButton != null) saveButton.onClick.AddListener(SaveGame);
        if (loadButton != null) loadButton.onClick.AddListener(LoadGame);
        if (newGameButton != null) newGameButton.onClick.AddListener(NewGame);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i; 
            if (slotButtons[i] != null)
            {
                slotButtons[i].onClick.AddListener(() => SelectSlot(slotIndex));
            }
        }

        UpdateSlotVisuals();
    }

    public void SelectSlot(int index)
    {
        if (PersistenceManager.Instance != null)
        {
            PersistenceManager.Instance.activeSlot = index;
            UpdateSlotVisuals();
            ShowStatus($"Slot {index + 1}");
        }
    }

    public void SaveGame()
    {
        if (PersistenceManager.Instance != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PersistenceManager.Instance.UpdatePlayerPosition(player.transform);
            }

            PersistenceManager.Instance.Save();
            ShowStatus("Saved");
            UpdateSlotVisuals();
        }
    }

    public void LoadGame()
    {
        if (PersistenceManager.Instance != null)
        {
            if (SaveSystem.SaveExists(PersistenceManager.Instance.activeSlot))
            {
                PersistenceManager.Instance.Load();
                ShowStatus("Loaded");
                if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            }
            else
            {
                ShowStatus("Empty slot");
            }
        }
    }

    public void NewGame()
    {
        if (PersistenceManager.Instance != null)
        {
            PersistenceManager.Instance.NewGame();
            ShowStatus("New Game");
            UpdateSlotVisuals();
        }
    }

    private void UpdateSlotVisuals()
    {
        if (PersistenceManager.Instance == null) return;

        int activeSlot = PersistenceManager.Instance.activeSlot;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null) continue;

           
            bool exists = SaveSystem.SaveExists(i);
            
            TextMeshProUGUI btnText = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                string baseText = $"Slot {i + 1}";
              
                btnText.text = baseText;
            }

            Image img = slotButtons[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = (i == activeSlot) ? new Color(0.6f, 1f, 0.6f) : Color.white;
            }
        }
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}