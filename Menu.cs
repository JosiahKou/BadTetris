using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public AudioMixer music;
    public AudioMixer sFX;
    public Slider SongSlider;
    public Slider SFXSlider;
    [SerializeField] GameObject SettingsPage;
    Resolution[] resolutions;
    public Dropdown resolutionDropdown;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayGame();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SettingsPage.SetActive(!SettingsPage.activeSelf);
        }
    }
    private void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        music.SetFloat("Volume", PlayerPrefs.GetFloat("Music", 0));
        sFX.SetFloat("SFX", PlayerPrefs.GetFloat("SFX", 0));
        SongSlider.value = PlayerPrefs.GetFloat("Music", 0);
        SFXSlider.value = PlayerPrefs.GetFloat("SFX", 0);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void SetMusic(float volume)
    {
        music.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat("Music", volume);

    }

    public void SetSFX(float volume)
    {
        sFX.SetFloat("SFX", volume);
        PlayerPrefs.SetFloat("SFX", volume);

    }
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
