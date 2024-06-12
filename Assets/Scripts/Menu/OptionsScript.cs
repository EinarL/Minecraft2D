using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsScript : MonoBehaviour
{
    private TextMeshProUGUI MusicVolumeText;
    private Slider musicVolumeSlider;

    private JsonDataService jsonDataService = JsonDataService.Instance;
    private OptionsManager optionsManager = OptionsManager.Instance;

    // Start is called before the first frame update
    void Start()
    {
        MusicVolumeText = transform.Find("OptionsScreen").Find("MusicSlider").Find("MusicSliderText").GetComponent<TextMeshProUGUI>();
		musicVolumeSlider = transform.Find("OptionsScreen").Find("MusicSlider").GetComponent<Slider>();

        loadOptions();
	}

    private void loadOptions()
    {
        if (!jsonDataService.exists("options.json", true)) return; // no saved options

        int[] options = jsonDataService.loadData<int[]>("options.json", true);

        musicVolumeSlider.value = options[0];
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) // go out of options
        {
            done();
        }
    }

    public void onChangeMusicSlider()
    {
        MusicVolumeText.text = $"Music: {musicVolumeSlider.value}%";
        optionsManager.setMusicVolume(musicVolumeSlider.value / 100f);
    }

    /**
     * runs when the user exits the options menu
     * 
     * it saves the options data in the form of:
     * new int[]{ musicVolume };
     */
    public void done()
    {
        jsonDataService.saveData("options.json",new int[] { (int)musicVolumeSlider.value }, true);

        // go to previous scene
        SceneManager.UnloadSceneAsync("Settings");
    }
}
