

public interface IDataService
{
	bool saveData<T>(string filename, T data);

	T loadData<T>(string filename);

	bool exists(string filename);
}
