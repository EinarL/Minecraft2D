using System.Collections;
using System.Collections.Generic;
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


		togglePlaySelectedWorldButtonEnabled(false);
	}

	void Update()
	{
		if(selectWorldScreen.activeSelf && Input.GetKeyDown(KeyCode.Escape)) // If in select world screen && user pressed escape
        {
            backToTitleScreen();
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
		// TODO: load the minecraft scene with the selected worlds name to pass in to the scene
		//createWorldObject();
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
		string worldName = worldNameInput.text;
		string errorMessage = validateWorldName(worldName);
		if(!errorMessage.Equals("")) // if there is an error with the input
		{
			worldNameErrorText.SetActive(true);
			worldNameErrorText.GetComponent<TextMeshProUGUI>().text = errorMessage;
			return;
		}

		createWorldObject(worldName);

		SceneManager.LoadScene("Minecraft");
	}

	public void onChangeWorldNameInput()
	{
		if(worldNameErrorText.activeSelf) worldNameErrorText.SetActive(false);
	}
	/**
	 * when we play a world then we need the next scene to know which world to load,
	 * therefore we create a gameobject that persists through the scene changes
	 * this gameobject will have a script which contains the variable for which world to load.
	 */
	private void createWorldObject(string worldName)
	{
		GameObject persistentGameObject = Instantiate(new GameObject());
		persistentGameObject.name = "WorldVariableGameObject";

		persistentGameObject.AddComponent<WorldVariableScript>();
		persistentGameObject.GetComponent<WorldVariableScript>().setWorldName(worldName);

		DontDestroyOnLoad(persistentGameObject);
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

		// If the name passes all validations, return an empty string (indicating no errors)
		return "";
	}

}
