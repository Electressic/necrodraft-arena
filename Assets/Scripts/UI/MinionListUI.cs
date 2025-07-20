using UnityEngine;
using System.Collections.Generic;

public class MinionListUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject minionListContainer;
    public GameplayMinionListEntry minionEntryPrefab;

    private List<GameplayMinionListEntry> currentEntries = new List<GameplayMinionListEntry>();

    void Start()
    {
        if (minionListContainer == null || minionEntryPrefab == null)
        {
            Debug.LogError("[MinionListUI] UI references are not set in the inspector!");
            this.enabled = false;
            return;
        }

        MinionManager.OnRosterChanged += RefreshMinionList;
        RefreshMinionList();
    }

    void OnDestroy()
    {
        MinionManager.OnRosterChanged -= RefreshMinionList;
    }

    private void RefreshMinionList()
    {
        foreach (var entry in currentEntries)
        {
            Destroy(entry.gameObject);
        }
        currentEntries.Clear();

        List<Minion> roster = MinionManager.GetMinionRoster();

        foreach (Minion minion in roster)
        {
            GameplayMinionListEntry newEntry = Instantiate(minionEntryPrefab, minionListContainer.transform);
            newEntry.Initialize(minion);
            currentEntries.Add(newEntry);
        }
    }
} 