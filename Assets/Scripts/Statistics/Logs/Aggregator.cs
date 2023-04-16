using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Aggregator : MonoBehaviour
{
	public static Aggregator instance;

	private List<string> keys = new List<string>();
	private string saveFileName = "aggregatorSaveData";




	private Dictionary<string, DailyTaskAggregate> dailyTaskLogs = new Dictionary<string, DailyTaskAggregate>();
	public Dictionary<string, NumberLocationAggregate> numberLocationLogs = new Dictionary<string, NumberLocationAggregate>();
	public Dictionary<string, WordleAggregate> wordleLogs = new Dictionary<string, WordleAggregate>();




	void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Start()
	{
		LoadAggregator();
	}

	public void LoadAggregator()
	{
		AggregatorSaveData aggregatorSaveData = SaveSystem.Load(saveFileName) as AggregatorSaveData;
		
		if (aggregatorSaveData == null)
		{
			return;
		}

		AggregatorSave aggregatorSave = new AggregatorSave(aggregatorSaveData);

		keys = aggregatorSaveData.keys;

		dailyTaskLogs = aggregatorSave.dailyTaskLogs;
		numberLocationLogs = aggregatorSave.numberLocationLogs;
	}

	public void Publish(DailyTaskCompletedEvent dailyTaskCompletedEvent)
	{
		string key = generateKey();
		DailyTaskAggregate dailyTaskAggregate = dailyTaskCompletedEvent.GetData();
		dailyTaskLogs.Add(key, dailyTaskAggregate);
		DailyScoreCalculator.PublishDailyTask(key, dailyTaskAggregate);

		SaveAggregator();
	}

	public void Publish(NumberLocationCompletedEvent numberLocationCompletedEvent)
	{
		string key = generateKey();
		NumberLocationAggregate numberLocationAggregate = numberLocationCompletedEvent.GetData();
		numberLocationLogs.Add(key, numberLocationAggregate);
		DailyScoreCalculator.PublishNumberLocation(key, numberLocationAggregate);

		SaveAggregator();
	}

	public void Publish(WordleCompletedEvent wordleCompletedEvent)
	{
		string key = generateKey();
		WordleAggregate wordleAggregate = wordleCompletedEvent.GetData();
		wordleLogs.Add(key, wordleAggregate);
		DailyScoreCalculator.PublishWordle(key, wordleAggregate);

		SaveAggregator();
	}

	public void SaveAggregator()
	{
		AggregatorSave aggregatorSave = new AggregatorSave();
		aggregatorSave.setDailyTaskLogs(dailyTaskLogs);
		aggregatorSave.setNumberLocationLogs(numberLocationLogs);

		AggregatorSaveData aggregatorSaveData = new AggregatorSaveData(keys,aggregatorSave);

		SaveCSV(dailyTaskLogs, "dailyTaskLogs");
		SaveCSV(numberLocationLogs, "numberLocationLogs");

		SaveSystem.Save(saveFileName, aggregatorSaveData);
	}

	public void SaveCSV<T>(Dictionary<string, T> data, string fileName) where T : IAggregate
	{
		string path = Application.persistentDataPath + "/" + fileName + ".csv";
		string csv = string.Empty;

		// add header
		foreach (KeyValuePair<string, T> entry in data)
		{
			csv += entry.Value.GetCSVHeader() + "\n";
			break;
		}

		foreach (KeyValuePair<string, T> entry in data)
		{
			csv += entry.Value.GetCSVData() + "," + entry.Key + "\n";
		}

		StreamWriter writer = new StreamWriter(path, false);
		writer.Write(csv);
		writer.Flush();
		writer.Close();
		// Debug.Log("Saved CSV: " + path);
	}

	public Dictionary<string, DailyTaskAggregate> GetTodaysDailyTaskLogs()
	{
		Dictionary<string, DailyTaskAggregate> todaysDailyTaskLogs = new Dictionary<string, DailyTaskAggregate>();

		foreach (KeyValuePair<string, DailyTaskAggregate> entry in dailyTaskLogs)
		{
			if (new System.DateTime(entry.Value.timestamp).Date == System.DateTime.Now.Date)
			{
				todaysDailyTaskLogs.Add(entry.Key, entry.Value);
			}
		}

		return todaysDailyTaskLogs;
	}

	public string generateKey()
	{
		while (true)
		{
			string key = KeyGenerator.GetKey();
			if (!keys.Contains(key))
			{
				keys.Add(key);
				return key;
			}
		}
	}
}

public interface IAggregate
{
	string GetCSVHeader();
	string GetCSVData();
}
