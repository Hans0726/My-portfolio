using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("���� �޴�")]
    [SerializeField] private Button btnGameStart;
    [SerializeField] private Button btnDeck;
    [SerializeField] private Button btnOption;
    [SerializeField] private Button btnQuit;

    [Space(5), Header("�ɼ�")]
    [SerializeField] private UIPopup_Matching gameStart;
    [SerializeField] private UIPopup option;
    [SerializeField] private UIPopup_Deck deck;
    
    [SerializeField]
    private Dropdown dropdownDisplayMode;

    [SerializeField] private Dropdown dropdownResolution;
    private Resolution[] resolutions;
    private List<string> resolutionOptions = new List<string>();

    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private Slider sliderMasterVolume;
    [SerializeField] private Toggle toggleMasterMute;

    [SerializeField] private Slider sliderBgmVolume;
    [SerializeField] private Toggle toggleBgmMute;

    [SerializeField] private Slider sliderSfxVolume;
    [SerializeField] private Toggle toggleSfxMute;

    private void Start()
    {
        btnGameStart.onClick.AddListener(gameStart.OpenPopup);
        btnDeck.onClick.AddListener(deck.OpenPopup);
        btnOption.onClick.AddListener(option.OpenPopup);
        btnQuit.onClick.AddListener(Application.Quit);

        SetupDropdowns();
        LoadVolumeSettings();

        sliderMasterVolume.onValueChanged.AddListener((float val) => { SetVolume(val, "Master"); });
        toggleMasterMute.onValueChanged.AddListener((bool isOn) => { SetMute(isOn, "Master"); });
        sliderBgmVolume.onValueChanged.AddListener((float val) => { SetVolume(val, "BGM"); });
        toggleBgmMute.onValueChanged.AddListener((bool isOn) => { SetMute(isOn, "BGM"); });
        sliderSfxVolume.onValueChanged.AddListener((float val) => { SetVolume(val, "SFX"); });
        toggleSfxMute.onValueChanged.AddListener((bool isOn) => { SetMute(isOn, "SFX"); });
    }

    #region ȭ����, �ػ�
    private void SetupDropdowns()
    {
        dropdownDisplayMode.onValueChanged.AddListener(SetDisplayMode);

        // resolutions�� ���⼭ �� ���� �����ͼ� ��� (��� ������ �����صΰ� ��Ȱ��)
        resolutions = Screen.resolutions;
        if (resolutions == null || resolutions.Length == 0)
        {
            Debug.LogError("Screen.resolutions returned no resolutions!");
            return;
        }

        dropdownResolution.options.Clear(); // ���� �ɼ� ��� ����

        List<string> uniqueResolutionStrings = new List<string>();
        List<Resolution> uniqueResolutionsForSelection = new List<Resolution>(); // ���� ���ÿ� ����� Resolution ��ü ����Ʈ

        // �ػ󵵸� �������� ��ȸ�ϸ鼭 ������ "�ʺ� x ����" ���ڿ��� �߰�
        // Screen.resolutions�� ���� ���� �ػ󵵺��� ���� �ػ� ������ ���ĵǾ� ����
        // �ڿ������� ��ȸ�ϸ� ���� �ػ� �� ���� ���� �ֻ����� ���� ���� ���� ������ �� ���ɼ��� ���� (�׻� ������� ����)
        for (int i = resolutions.Length - 1; i >= 0; i--)
        {
            Resolution currentRes = resolutions[i];
            string option = currentRes.width + " x " + currentRes.height;

            // �̹� �߰��� "�ʺ� x ����" ���ڿ����� Ȯ��
            if (!uniqueResolutionStrings.Contains(option))
            {
                uniqueResolutionStrings.Add(option);
                uniqueResolutionsForSelection.Add(currentRes); // �ش� Resolution ��ü�� ����
            }
        }

        // ����Ƽ �����Ϳ����� resolutions ������ ����� �ٸ� �� �����Ƿ�,
        // ���ڿ� ����Ʈ�� �ٽ� �����ϰų�, uniqueResolutionsForSelection�� �ʺ�/���� �������� ������ �� ����.
        // ���⼭�� �ϴ� �߰��� ������� ��� (���� ���� �ػ󵵰� ���� �߰���)
        // �ʿ��ϴٸ� uniqueResolutionStrings.Sort() �Ǵ� Reverse() ���� ���

        dropdownResolution.AddOptions(uniqueResolutionStrings); // ��Ӵٿ ������ �ػ� ���ڿ� �߰�
        dropdownResolution.onValueChanged.RemoveAllListeners(); // ���� ������ ���� (�ߺ� ����)
        dropdownResolution.onValueChanged.AddListener(index => SetResolutionByIndex(index, uniqueResolutionsForSelection)); // ������ ������ ����

        // ���� �ػ󵵿� �´� ��Ӵٿ� �� ���� (���� ����)
        int currentResolutionIndex = -1;
        string currentScreenOption = Screen.currentResolution.width + " x " + Screen.currentResolution.height;
        for (int i = 0; i < uniqueResolutionStrings.Count; ++i)
        {
            if (uniqueResolutionStrings[i] == currentScreenOption)
            {
                currentResolutionIndex = i;
                break;
            }
        }
        if (currentResolutionIndex != -1)
        {
            dropdownResolution.value = currentResolutionIndex;
            dropdownResolution.RefreshShownValue(); // ���� ���õ� ������ UI ������Ʈ
        }
    }

    // SetResolution �Լ��� �ε����� �Բ� ���� �ػ� ����Ʈ�� �޵��� ����
    public void SetResolutionByIndex(int uniqueResolutionListIndex, List<Resolution> uniqueResolutions)
    {
        if (uniqueResolutionListIndex < 0 || uniqueResolutionListIndex >= uniqueResolutions.Count)
        {
            Debug.LogError($"Invalid resolution index: {uniqueResolutionListIndex}");
            return;
        }

        Resolution selectedRes = uniqueResolutions[uniqueResolutionListIndex];

        // ���� �ػ󵵿� �ٸ� ��쿡�� ���� (���ʿ��� ���� ����)
        // �ֻ����� selectedRes�� ���Ե� ���� ����ϰų�, Screen.currentResolution.refreshRate�� ������ �� ����
        // ���⼭�� selectedRes�� ���Ե� �ֻ��� ���
        if (Screen.width != selectedRes.width || Screen.height != selectedRes.height || Screen.currentResolution.refreshRateRatio.value != selectedRes.refreshRateRatio.value)
        {
            Debug.Log($"Setting resolution to: {selectedRes.width}x{selectedRes.height} @ {selectedRes.refreshRateRatio}Hz");
            Screen.SetResolution(selectedRes.width, selectedRes.height, Screen.fullScreenMode, selectedRes.refreshRateRatio);
        }
    }



    public void SetDisplayMode(int displayModeIndex)
    {
        switch (displayModeIndex)
        {
            case 0: // ��ü ȭ��
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1: // â ���
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 2: // �׵θ� ���� â ���
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }
    }

    #endregion

    #region ����
    public void SetVolume(float volume, string type)
    {
        audioMixer.SetFloat(type, Mathf.Log10(volume) * 20);
    }

    public void SetMute(bool mute, string type)
    {
        float volume = mute ? -80f : sliderMasterVolume.value;
        audioMixer.SetFloat(type, Mathf.Log10(volume) * 20);
    }

    private void LoadVolumeSettings()
    {
        // ����� ���� �� �ε�
        float masterVolume, bgmVolume, sfxVolume;
        audioMixer.GetFloat("Master", out masterVolume);
        audioMixer.GetFloat("BGM", out bgmVolume);
        audioMixer.GetFloat("SFX", out sfxVolume);

        sliderMasterVolume.value = Mathf.Pow(10f, masterVolume / 20f);
        sliderBgmVolume.value = Mathf.Pow(10f, bgmVolume / 20f);
        sliderSfxVolume.value = Mathf.Pow(10f, sfxVolume / 20f);
        toggleMasterMute.isOn = toggleBgmMute.isOn = toggleSfxMute.isOn = false;
    }
    #endregion
}
