

public interface IDataService
{
	bool saveData<T>(string filename, T data, bool global = false);

	bool appendToData(string filename, object[] data);

	T loadData<T>(string filename, bool global = false);

	bool exists(string filename, bool global = false);
}
