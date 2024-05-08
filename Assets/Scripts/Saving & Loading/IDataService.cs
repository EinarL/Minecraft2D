

public interface IDataService
{
	bool saveData<T>(string filename, T data);

	bool appendToData(string filename, object[] data);

	T loadData<T>(string filename);

	bool exists(string filename);
}
