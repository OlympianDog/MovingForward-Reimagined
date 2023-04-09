using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastActivityPanelManager : MonoBehaviour
{
	public GameObject LastActivityItemPrefab;
    public GameObject NoActivityItemPrefab;
	public Transform LastActivityItemContainer;

	public int MaxItems = 3;

    void OnEnable()
    {
        GetLatestActivity();
    }

    void OnDisable()
    {
        // Clear the container
		foreach (Transform child in LastActivityItemContainer)
		{
			Destroy(child.gameObject);
		}
        StopAllCoroutines();
    }

	void GetLatestActivity()
	{
		// Clear the container
		foreach (Transform child in LastActivityItemContainer)
		{
			Destroy(child.gameObject);
		}

		List<DailyScoreStorageItem> dailyScoreStorageItems = DailyScoreStorage.GetDailyScoreStorageItems(MaxItems);

        dailyScoreStorageItems.Reverse();

        if (dailyScoreStorageItems.Count == 0)
        {
            Instantiate(NoActivityItemPrefab, LastActivityItemContainer);
            return;
        }

        StartCoroutine(AnimateItem(dailyScoreStorageItems));
	}

    IEnumerator AnimateItem(List<DailyScoreStorageItem> dailyScoreStorageItems)
    {
        foreach (DailyScoreStorageItem dailyScoreStorageItem in dailyScoreStorageItems)
		{
			GameObject lastActivityItem = Instantiate(LastActivityItemPrefab, LastActivityItemContainer);
			LastActivityItem lastActivityItemScript = lastActivityItem.GetComponent<LastActivityItem>();

			lastActivityItemScript.activityName = GetNameFromType(dailyScoreStorageItem.dailyScoreStorageType);
			lastActivityItemScript.activityScore = dailyScoreStorageItem.score;
			lastActivityItemScript.activityTimestamp = dailyScoreStorageItem.timestamp;

            yield return new WaitForSeconds(0.1f);
		}
    }

	string GetNameFromType(DailyScoreStorageType dailyScoreStorageType)
	{
        switch (dailyScoreStorageType)
        {
            case DailyScoreStorageType.AutoGeneratedDailyTask:
                return "Daily Task";
            case DailyScoreStorageType.Minigame:
                return "Minigame";
            default:
                return "Unknown";
        }
	}
}