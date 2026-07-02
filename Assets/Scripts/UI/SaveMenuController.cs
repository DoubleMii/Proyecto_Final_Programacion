using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveMenuController : MonoBehaviour
{
    public Button[] slotButtons;
    public Button saveButton;
    public Button loadButton;
    public Button newGameButton;
    public Button resetSlotButton;
    public Button deleteSlotButton;
    public Button quitButton;
    public TextMeshProUGUI statusText;

    private void Start()
    {
        AutoAssignButtons();

        if (saveButton != null) saveButton.onClick.AddListener(SaveGame);
        if (loadButton != null) loadButton.onClick.AddListener(LoadGame);
        if (newGameButton != null) newGameButton.onClick.AddListener(NewGame);
        if (resetSlotButton != null) resetSlotButton.onClick.AddListener(ResetSlot);
        if (deleteSlotButton != null) deleteSlotButton.onClick.AddListener(DeleteSlot);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

        for (int i = 0; slotButtons != null && i < slotButtons.Length; i++)
        {
            int slotIndex = i;

            if (slotButtons[i] != null)
            {
                slotButtons[i].onClick.AddListener(() => SelectSlot(slotIndex));
            }
        }

        UpdateSlotVisuals();
    }

    private void AutoAssignButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            string buttonName = button.name.ToLowerInvariant();

            if (saveButton == null && (buttonName.Contains("guardar") || buttonName.Contains("save")))
                saveButton = button;
            else if (loadButton == null && (buttonName.Contains("cargar") || buttonName.Contains("load")))
                loadButton = button;
            else if (newGameButton == null && (buttonName.Contains("nueva") || buttonName.Contains("newgame") || buttonName.Contains("new_game")))
                newGameButton = button;
            else if (deleteSlotButton == null && (buttonName.Contains("borrar") || buttonName.Contains("delete")))
                deleteSlotButton = button;
            else if (resetSlotButton == null && (buttonName.Contains("reset") || buttonName.Contains("reiniciar")))
                resetSlotButton = button;
            else if (quitButton == null && (buttonName.Contains("salir") || buttonName.Contains("quit") || buttonName.Contains("exit")))
                quitButton = button;
        }
    }

    public void SelectSlot(int index)
    {
        if (PersistenceManager.Instance != null)
        {
            PersistenceManager.Instance.activeSlot = index;
            UpdateSlotVisuals();
            ShowStatus($"Slot {index + 1} seleccionado");
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

            bool saved = PersistenceManager.Instance.TrySave();
            ShowStatus(saved ? "Partida guardada" : "Error al guardar");
            UpdateSlotVisuals();
        }
    }

    public void LoadGame()
    {
        if (PersistenceManager.Instance != null)
        {
            if (SaveSystem.SaveExists(PersistenceManager.Instance.activeSlot))
            {
                bool loaded = PersistenceManager.Instance.TryLoad();

                ShowStatus(loaded ? "Partida cargada" : "Error al cargar");

                if (loaded && GameManager.Instance != null)
                    GameManager.Instance.ResumeGame();
            }
            else
            {
                ShowStatus("Slot vacio");
            }
        }
    }

    public void NewGame()
    {
        if (PersistenceManager.Instance != null)
        {
            PersistenceManager.Instance.NewGame();
            ShowStatus("Nueva partida");
            UpdateSlotVisuals();
        }
    }

    public void ResetSlot()
    {
        if (PersistenceManager.Instance != null)
        {
            PersistenceManager.Instance.ResetActiveSlot();
            ShowStatus("Partida reiniciada");
            UpdateSlotVisuals();
        }
    }

    public void DeleteSlot()
    {
        if (PersistenceManager.Instance != null)
        {
            int slot = PersistenceManager.Instance.activeSlot;
            PersistenceManager.Instance.DeleteActiveSlot();
            ShowStatus($"Slot {slot + 1} borrado");
            UpdateSlotVisuals();
        }
    }

    public void QuitGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }

    private void UpdateSlotVisuals()
    {
        if (PersistenceManager.Instance == null || slotButtons == null)
            return;

        int activeSlot = PersistenceManager.Instance.activeSlot;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null)
                continue;

            bool exists = SaveSystem.SaveExists(i);

            TextMeshProUGUI btnText =
                slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            if (btnText != null)
            {
                string baseText = $"Slot {i + 1}";

                btnText.text = exists
                    ? $"{baseText}\n(Guardado)"
                    : $"{baseText}\n(Vacio)";
            }

            Image img = slotButtons[i].GetComponent<Image>();

            if (img != null)
            {
                img.color = (i == activeSlot)
                    ? new Color(0.6f, 1f, 0.6f)
                    : Color.white;
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
