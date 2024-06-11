using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenScript : MonoBehaviour
{
    private GameObject titleScreen;
    private GameObject selectWorldScreen;
	private GameObject createNewWorldScreen;
	private GameObject worldNameErrorText;
    private Button playSelectedWorldButton;
    private TextMeshProUGUI playSelectedWorldButtonText;
	private TMP_InputField worldNameInput;
	private GameObject worldsParent;

	private WorldItemScript selectedWorld; // which world is selected on the select world screen

	// Start is called before the first frame update
	void Start()
    {
        titleScreen = transform.Find("TitleScreen").gameObject;
		selectWorldScreen = transform.Find("SelectWorldScreen").gameObject;
		createNewWorldScreen = transform.Find("CreateNewWorldScreen").gameObject;
		playSelectedWorldButton = transform.Find("SelectWorldScreen").Find("Play Selected World Button").GetComponent<Button>();
		playSelectedWorldButtonText = playSelectedWorldButton.transform.Find("PlayText").GetComponent<TextMeshProUGUI>();
		worldNameInput = createNewWorldScreen.transform.Find("WorldNameInput").GetComponent<TMP_InputField>();
		worldNameErrorText = createNewWorldScreen.transform.Find("ErrorText").gameObject;
		worldsParent = selectWorldScreen.transform.Find("DarkBackground").Find("Scroll").Find("View").Find("Content").gameObject;


		togglePlaySelectedWorldButtonEnabled(false);
		fillSelectWorldScrollView();
	}

	private void fillSelectWorldScrollView()
	{
		GameObject worldItem = worldsParent.transform.Find("WorldItem").gameObject;
		GameObject emptyWorldItem = worldsParent.transform.Find("EmptyWorldItem").gameObject;

		if (Directory.Exists(Application.persistentDataPath + "\\worlds\\")) // if worlds folder exists
		{
			string[] directories = Directory.GetDirectories(Application.persistentDataPath + "\\worlds\\"); // get all folder names in worlds directory

			foreach (string directory in directories) // create world item for each folder name
			{
				GameObject worldItemClone = Instantiate(worldItem);
				worldItemClone.transform.Find("WorldName").GetComponent<TextMeshProUGUI>().text = Path.GetFileName(directory); // set name to folder name
				worldItemClone.transform.SetParent(worldsParent.transform, false);
			}

			// this is a bad fix to make the 1 or 2 world items be at the top because i cant 
			// figure out a way to do it in the UI
			if (directories.Length == 1 || directories.Length == 2) 
			{
				int howManyToCreate = directories.Length == 1 ? 2 : 1;

				for(int _ = 0; _ < howManyToCreate; _++)
				{
					GameObject emptyItemClone = Instantiate(emptyWorldItem);
					emptyItemClone.transform.SetParent(worldsParent.transform, false);
				}
			}
		}

		Destroy(emptyWorldItem);
		Destroy(worldItem);
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(selectWorldScreen.activeSelf) backToTitleScreen();
			else if (createNewWorldScreen.activeSelf) backToSelectWorldScreen();
		} 
	}

	/**
     * runs when the user clicks on the play button on the title screen.
     * this takes you to the "Select World" menu screen.
     */
	public void play()
    {
        titleScreen.SetActive(false);
        selectWorldScreen.SetActive(true);
    }
	/**
     * runs when the user clicks on the options button on the title screen.
     * this changes the scene to the options scene.
     */
	public void options()
    {
         // TODO
    }
	/**
     * runs when the user clicks on the quit game button on the title screen.
     */
	public void quitGame()
    {
		// If the game is running in the Unity editor
#if UNITY_EDITOR
		// Stop playing the scene
		UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the application
        Application.Quit();
#endif
	}

	//////////////////////////////////////////////////////////////////////
	///                         SELECT WORLD BUTTONS
	//////////////////////////////////////////////////////////////////////

	public void backToTitleScreen()
	{
		titleScreen.SetActive(true);
		selectWorldScreen.SetActive(false);
	}

    /**
     * sets the "Play Selected World" button to active/inactive, because the button
     * can only be active when there is a selected world
     */
    private void togglePlaySelectedWorldButtonEnabled(bool enabled)
    {
        playSelectedWorldButton.interactable = enabled; // disable/enable button
        if(enabled) playSelectedWorldButtonText.color = new Color(1, 1, 1, 1);
		else playSelectedWorldButtonText.color = new Color(1, 1, 1, 0.24f);
	}

	public void gotToCreateNewWorldScreen()
	{
		selectWorldScreen.SetActive(false);
		createNewWorldScreen.SetActive(true);
	}

	public void playSelectedWorld()
	{
		setWorld(selectedWorld.getWorldName());

		SceneManager.LoadScene("Minecraft");
	}

	/**
	 * gets called from a WorldItemScript when a world gets selected.
	 */
	public void setSelectedWorld(WorldItemScript world)
	{
		if (selectedWorld != null) selectedWorld.deselectWorld();
		selectedWorld = world;
		togglePlaySelectedWorldButtonEnabled(true);
	}

	//////////////////////////////////////////////////////////////////////
	///                         CREATE NEW WORLD BUTTONS
	//////////////////////////////////////////////////////////////////////
	public void backToSelectWorldScreen()
	{
		selectWorldScreen.SetActive(true);
		createNewWorldScreen.SetActive(false);
		worldNameInput.text = "";
	}

	public void createWorldAndPlay()
	{
		string worldName = worldNameInput.text.Trim();
		string errorMessage = validateWorldName(worldName);
		if(!errorMessage.Equals("")) // if there is an error with the input
		{
			worldNameErrorText.SetActive(true);
			worldNameErrorText.GetComponent<TextMeshProUGUI>().text = errorMessage;
			return;
		}

		setWorld(worldName);

		SceneManager.LoadScene("Minecraft");
	}

	public void onChangeWorldNameInput()
	{
		if(worldNameErrorText.activeSelf) worldNameErrorText.SetActive(false);
	}
	/**
	 * when we play a world then we need the next scene to know which world to load,
	 * so we tell JsonDataService and SaveChunk which folder the world is in.
	 */
	private void setWorld(string worldName)
	{
		if (!Directory.Exists(Application.persistentDataPath + "\\worlds\\")) // create worlds folder if it doesn't exist
		{
			Directory.CreateDirectory(Application.persistentDataPath + "\\worlds\\");
		}
		Directory.CreateDirectory(Application.persistentDataPath + "\\worlds\\" + worldName + "\\"); // create folder for the world

		JsonDataService.Instance.setWorldFolder(worldName);
		SaveChunk.setWorldFolder(worldName);
	}

	private string validateWorldName(string worldName)
	{
		// Check if the name has more than 0 characters
		if (string.IsNullOrWhiteSpace(worldName))
		{
			return "World name cannot be empty.";
		}

		// Check if the name contains only letters of the English alphabet, numbers, underscores, or spaces
		foreach (char c in worldName)
		{
			if (!(char.IsDigit(c) || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) && c != '_' && c != ' ')
			{
				return "World name can only contain letters of the english alphabet, numbers, underscores, or spaces.";
			}
		}

		// check if there already exists a world with this name
		if (Directory.Exists(Application.persistentDataPath + "\\worlds\\"))
		{
			string[] directories = Directory.GetDirectories(Application.persistentDataPath + "\\worlds\\");

			foreach (string directory in directories)
			{
				if (Path.GetFileName(directory).Equals(worldName)) return "There is already a world named " + worldName;
			}
		}

		// If the name passes all validations, return an empty string (indicating no errors)
		return "";
	}

}
