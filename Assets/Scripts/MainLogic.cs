using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Net.Http;

public class MainLogic : MonoBehaviour
{
    public GameObject canvas;

    public GameObject cam1;
    public GameObject cam2;

    public InputField prolificCodeText;

    public Texture[] textures1;
    public Texture[] textures2;
    public GameObject body2;
    public Texture[] textures3;
    public Texture[] textures4;
    public GameObject body4;

    enum ScreenName {PRE, LOGIN, DEMOGRAPHICS, CONDITIONS, TOPICCHOOSE, HELP, AGENT, BEFROEVERBAL, VERBAL, THANKS};
    private ScreenName currentScreen = ScreenName.PRE;
    private string[] topicArray = { "Quantum Computing", "Blockchain", "Transformer Architectures", "Quantum Mechanics", "String Theory", "General Relativity" };
    private string topicName = "Test";
    private string agentName = "";

    private int maxTokenUsageLimit = 100_000; // 1000 tokens ~ $0.002

    private OpenAIAPI api;
    private List<ChatMessage> messages;

    private string currentUserEmail;
    private int currentVariation;
    private int currentTokenUsage;

    [Header("Dialogue UI")]
    public InputField dialogueInputField;
    public Button dialogueSayButton;
    public Text agentResponseText;
    public Text dialogueInputText;
    private bool askingEnabled = false;
    private bool agentTalking = false;

    [Header("Login Page")]
    public GameObject loginError;
    public GameObject loginPanel;
    public InputField userMailText;

    [Header("Demographics Page")]
    public GameObject demographicsPanel;
    public InputField ageText;
    public InputField genderText;
    public InputField countryText;

    [Header("Panel Background")]
    public GameObject panelBackground;

    [Header("Loading Panel")]
    public GameObject loadingPanel;

    [Header("Study Questions")]
    public GameObject questionsPanelContent;
    public Text qText;
    public GameObject buttonGroup;
    public Button completeSurveyButton;
    public Text answeredQuestionCountText;
    public GameObject questionsPanel;
    public GameObject toggleQuestionsPanelButton;
    public Text askedAtLeast10Text;
    public Text askedAtLeast10OnScreenText;
    private bool studyQuestionsOn = false;

    [Header("Chat History Part")]
    public GameObject chatContent;
    public GameObject oneButton;
    public GameObject chatPanel;
    public GameObject toggleChatHistoryButton;
    private bool chatHistoryOn = false;

    [Header("Quit Part")]
    public GameObject quitSurePanel;

    [Header("Conditions Panel")]
    public GameObject conditionsPanel;

    [Header("Verbal Questions Panel")]
    public GameObject verbalQuestionsPanel;
    public InputField vqa1;
    public InputField vqa2;
    public InputField vqa3;
    public InputField vqa4;
    public InputField vqa5;
    public Text vq1;
    public Text vq2;
    public Text vq3;
    public Text vq4;
    public Text vq5;
    public Button verbalQuestionsSubmitButton;
    public Text verbalQuestionsAnswer5Text;
    public GameObject verbalQuestionsBeforePanel;

    [Header("Thanks Panel")]
    public GameObject thanksPanel;

    [Header("Help Panel")]
    public GameObject helpPanel;

    [Header("Agent LipSync")]
    public MyVRLipSyncPasser lipSyncPasser;
    public GameObject agent2;
    public GameObject agent4;
    public AnimatorInspector animatorInspector;

    [Header("Topic Choose Panel")]
    public GameObject topicChoosePanel;
    public GameObject topicButtonsPanel;
    public Button topicButton;

    [Header("Subtitles")]
    public Button subtitlesButton;
    private bool subtitlesOn = true;
    public Text subtitlesHelpText;

    [Header("New Response Text")]
    public Text line1;
    public Text line2;
    public Text line3;
    public Text line4;
    public Text line5;
    public GameObject newResponsePanel;

    [Header("Personality")]
    [Range(-1f, 1f)] public float personality;

    [Header("Scene Lights")]
    public GameObject sceneLights;

    [Header("No Voice Panel")]
    public GameObject noVoicePanel;

    private List<Button> buttonArrayList = new List<Button>();
    private int questionsAskedToAgent = 0;
    private int requiredAskLimit = 5;

    private bool deterministicMode = false;

    string[] surveyQuestions = {
        "This person is someone who tends to be quiet.", // 1
        "This person is someone who is compassionate, has a soft heart.", // 2
        "This person is someone who tends to be disorganized.", // 3
        "This person is someone who worries a lot.", // 4
        "This person is someone who is fascinated by art, music, or literature.", // 5
        "This person is someone who is dominant, acts as a leader.", // 6
        "This person is someone who is sometimes rude to others.", // 7
        "This person is someone who has difficulty getting started on tasks.", // 8
        "This person is someone who tends to feel depressed, blue.", // 9
        "This person is someone who has little interest in abstract ideas.", // 10
        "This person is someone who is full of energy.", // 11
        "This person is someone who assumes the best about people.", // 12
        "This person is someone who is reliable, can always be counted on.", // 13
        "This person is someone who is emotionally stable, not easily upset.", // 14
        "This person is someone who is original, comes up with new ideas.", // 15
        /*"This person is someone who is outgoing, sociable.", // 16
        "This person is someone who can be cold and uncaring.", // 17
        "This person is someone who keeps things neat and tidy.", // 18
        "This person is someone who is relaxed, handles stress well.", // 19
        "This person is someone who has few artistic interests.", // 20
        "This person is someone who prefers to have others take charge.", // 21
        "This person is someone who is respectful, treats others with respect.", // 22
        "This person is someone who is persistent, works until the task is finished.", // 23
        "This person is someone who feels secure, comfortable with self.", // 24
        "This person is someone who is complex, a deep thinker.", // 25
        "This person is someone who is less active than other people.", // 26
        "This person is someone who tends to find fault with others.", // 27
        "This person is someone who can be somewhat careless.", // 28
        "This person is someone who is temperamental, gets emotional easily.", // 29
        "This person is someone who has little creativity.", // 30
        */
        "Working with the conversational agent helped me learn.", // 31
        "The feedback from the conversational agent helped me learn.", // 32
        "The graphics and animations of the conversational agent helped me learn. ", // 33
        "The conversational agent helped teach me a new concept.", // 34
        "Overall, the conversational agent helped me learn.", // 35
        "The help features of the system were useful.", // 36
        "The instructions of the system were easy to follow.", // 37
        "The conversational agent was easy to use.", // 38
        "The system was well organized.", // 39
        "I liked the overall theme of the conversational agent system.", // 40
        "I found the conversational agent motivating.", // 41
        "I would like to use the conversational agent again." // 42
    };

    private int[] surveyAnswers;
    private int answeredSurveyQuestions = 0;
    private float timeElapsed;

    void Start()
    {
        
    }

    public void OnEnable()
    {
        Speaker.Instance.OnVoicesReady += voicesReady;
        Speaker.Instance.OnSpeakCurrentWord += speakCurrentWordMethod;
    }

    public void OnDisable()
    {
        Speaker.Instance.OnVoicesReady -= voicesReady;
        Speaker.Instance.OnSpeakCurrentWord -= speakCurrentWordMethod;
    }

    private bool hasMale = false;
    private bool hasFemale = false;
    private bool hasEnglish = false;
    private bool hasVoice = false;
    private bool hasRegular = false;

    private List<Voice> voiceList;
    private List<Voice> supportedVoices;

    // private string[] supportedNames = { "Zira", "David", "Hazel" }; // "Mark", "George", "Susan", "Sean"
    private Voice selectedVoice;

    private void voicesReady()
    {
        voiceList = Speaker.Instance.VoicesForLanguage(SystemLanguage.English);
        supportedVoices = new List<Voice>();

        foreach (Voice voice in voiceList)
        {
            supportedVoices.Add(voice);
            /*
            foreach (string s in supportedNames)
            {
                if (voice.Name.Contains(s) && !voice.Name.Contains("Natural"))
                {
                    
                }
            }*/
        }

        bool decision = supportedVoices.Count > 0;

        if (decision)
        {
            // choose a random voice
            if(deterministicMode)
            {
                selectedVoice = supportedVoices[0];
                isMale = false;
                ChangeAgent(1);
                initAll();
            }
            else
            {
                int vIndex = Random.Range(0, supportedVoices.Count);
                selectedVoice = supportedVoices[vIndex];

                if (selectedVoice.Gender == Crosstales.RTVoice.Model.Enum.Gender.MALE)
                {
                    isMale = true;
                    ChangeAgent(Random.Range(3, 5));
                }
                else
                {
                    isMale = false;
                    ChangeAgent(Random.Range(1, 3));
                }

                initAll();
            }
        }
        else
        {
            noVoicePanel.SetActive(true);
        }

    }

    private void initAll()
    {
        timeElapsed = 0;
        DisableAsking();
        initSurveyQuestions();

        api = new OpenAIAPI("insert-your-api-key-here");
        sceneLights.SetActive(true);

        currentScreen = ScreenName.LOGIN;
        loginPanel.SetActive(true);
    }

    void initSurveyQuestions()
    {
        // init topic buttons
        
        for(int i = 0; i < topicArray.Length; i++)
        {
            Button tButton = Instantiate(topicButton);
            tButton.GetComponent<RectTransform>().SetParent(topicButtonsPanel.transform);
            tButton.GetComponent<RectTransform>().localScale = buttonGroup.GetComponent<RectTransform>().localScale;
            int buttonNoForTopic = i;
            tButton.onClick.AddListener(() => TopicButtonClicked(buttonNoForTopic));
            tButton.GetComponentInChildren<Text>().text = topicArray[i];
            tButton.gameObject.SetActive(true);
        }

        // completing survey requires answering all questions
        completeSurveyButton.interactable = false;

        surveyAnswers = new int[surveyQuestions.Length];
        for (int i = 0; i < surveyQuestions.Length; i++)
        {
            surveyAnswers[i] = -1;
        }
        answeredSurveyQuestions = 0;

        int btnCode = 0;
        for (int i = 0; i < surveyQuestions.Length; i++)
        {
            Text t2 = Instantiate(qText);
            t2.rectTransform.SetParent(questionsPanelContent.transform);
            t2.rectTransform.localPosition = qText.rectTransform.localPosition + new Vector3(0, -200 * i, 0);
            t2.rectTransform.localScale = qText.rectTransform.localScale;
            t2.text = (i+1) + ". " + surveyQuestions[i];

            GameObject b2 = Instantiate(buttonGroup);
            b2.GetComponent<RectTransform>().SetParent(questionsPanelContent.transform);
            b2.GetComponent<RectTransform>().localPosition = buttonGroup.GetComponent<RectTransform>().localPosition + new Vector3(0, -200 * i, 0);
            b2.GetComponent<RectTransform>().localScale = buttonGroup.GetComponent<RectTransform>().localScale;

            Button[] btns = b2.GetComponentsInChildren<Button>();
            foreach (Button bb in btns)
            {
                int btnCodeCpy = btnCode;
                bb.onClick.AddListener(() => ButtonClicked(btnCodeCpy));
                buttonArrayList.Add(bb);
                btnCode++;
            }
        }

        questionsPanelContent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 200 * (surveyQuestions.Length + 1));

        qText.gameObject.SetActive(false);
        buttonGroup.SetActive(false);

        SurveyButtonsSetInteract(false);
    }
    class ChatHistory
    {
        public string message;
        public bool isComputer;

        public ChatHistory(string message, bool isComputer)
        {
            this.message = message;
            this.isComputer = isComputer;
        }
    }

    private List<ChatHistory> chatHistory = new List<ChatHistory>();
    private List<Button> compChatHistoryButtons = new List<Button>();
    void AddChatHistory(string content, bool isComp)
    {
        chatHistory.Add(new ChatHistory(content, isComp));

        GameObject b2 = Instantiate(oneButton);
        b2.GetComponent<RectTransform>().SetParent(chatContent.transform);
        
        // b2.GetComponent<RectTransform>().localPosition = oneButton.GetComponent<RectTransform>().localPosition + new Vector3(0, -130 * (chatHistory.Count-1), 0);
        b2.GetComponent<RectTransform>().localScale = oneButton.GetComponent<RectTransform>().localScale;

        int btnCodeCpy = chatHistory.Count - 1;
        b2.GetComponent<Button>().onClick.AddListener(() => ChatButtonClicked(btnCodeCpy));
        if(isComp)
        {
            if(askingEnabled)
            {
                b2.GetComponent<Button>().interactable = true;
            }
            else
            {
                b2.GetComponent<Button>().interactable = false;
            }
            
            compChatHistoryButtons.Add(b2.GetComponent<Button>());
        }
        else 
        {
            b2.GetComponent<Button>().interactable = false;
        }
        b2.GetComponentInChildren<Text>().text = chatHistory[chatHistory.Count-1].message;

        b2.gameObject.SetActive(true);

        // chatContent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 130 * (chatHistory.Count + 1));
    }

    void ChatButtonClicked(int buttonNo)
    {
        SpeakPartialEnd();
        DisableAsking();
        SpeakPartial(chatHistory[buttonNo].message);
        Button_ToggleChatHistory();
    }

    void SurveyButtonsSetInteract(bool isInteractable)
    {
        for(int i = 0; i < buttonArrayList.Count; i++)
        {
            buttonArrayList[i].interactable = isInteractable;
        }
    }

    void TopicButtonClicked(int buttonNo)
    {
        topicName = topicArray[buttonNo];

        vq1.text = "What is " + topicName + ", based on your conversation with the system?";
        vq2.text = "Why is " + topicName + " important, please discuss briefly?";
        vq3.text = "Did you learn anything new about " + topicName + " after your conversation with the system? If so, what did you learn?";
        vq4.text = "Do you think the behavior of the system was influential on your learning experience?";
        vq5.text = "Please briefly describe your conversation with the system. Was it interesting? Did you encounter any problems?";

        Debug.Log("Topic:" + topicName);
        AfterChoosingTopic();
    }

    void ButtonClicked(int buttonNo)
    {
        int choice = buttonNo % 5;
        int groupno = buttonNo / 5;

        // reset others of this group
        for(int i = 0; i < 5; i++)
        {
            buttonArrayList[(groupno * 5) + i].image.color = Color.white;
        }

        buttonArrayList[buttonNo].image.color = Color.green;

        if(surveyAnswers[groupno] == -1)
        {
            answeredSurveyQuestions++;
        }

        surveyAnswers[groupno] = choice;
        Debug.Log("Answer " + groupno + " - Choice: " + choice);

        // check for survey end
        answeredQuestionCountText.text = "You answered " + answeredSurveyQuestions + " of the " + surveyAnswers.Length + " questions.";
        if(answeredSurveyQuestions == surveyAnswers.Length)
        {
            completeSurveyButton.interactable = true;
            completeSurveyButton.gameObject.GetComponent<Image>().color = new Color(72f / 255, 255f / 255, 72f / 255, 101f / 255);
        }
    }

    IEnumerator PostWeb()
    {
        WWWForm form = new WWWForm();
        form.AddField("usermail", "testt");

        using (UnityWebRequest www = UnityWebRequest.Post("https://animated-personality.com.tr/unity_add_user.php", form))
        {
            www.SetRequestHeader("User-Agent", "DefaultBrowser");
            www.SetRequestHeader("Cookie", string.Format("DummyCookie"));
            www.chunkedTransfer = false;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    IEnumerator AddTokenRoutine(int usage)
    {
        WWWForm form = new WWWForm();

        currentUserEmail = userMailText.text;
        form.AddField("usermail", currentUserEmail);
        form.AddField("usage", usage);

        using (UnityWebRequest www = UnityWebRequest.Post("https://animated-personality.com.tr/unity_add_usage_for_given_user.php", form))
        {
            www.SetRequestHeader("User-Agent", "DefaultBrowser");
            www.SetRequestHeader("Cookie", string.Format("DummyCookie"));
            www.chunkedTransfer = false;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
        }
    }    
    
    IEnumerator CheckTokenRoutine()
    {
        WWWForm form = new WWWForm();

        currentUserEmail = userMailText.text;
        form.AddField("usermail", currentUserEmail);

        using (UnityWebRequest www = UnityWebRequest.Post("https://animated-personality.com.tr/unity_get_usage_sum_for_given_user.php", form))
        {
            www.SetRequestHeader("User-Agent", "DefaultBrowser");
            www.SetRequestHeader("Cookie", string.Format("DummyCookie"));
            www.chunkedTransfer = false;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                int.TryParse(www.downloadHandler.text, out currentTokenUsage);
                Debug.Log("Current Usage: " + currentTokenUsage);
            }
        }
    }

    IEnumerator AddAllowedUserRoutine()
    {
        WWWForm form = new WWWForm();

        currentUserEmail = userMailText.text;
        if(currentUserEmail.Length > 0)
        {
            form.AddField("usermail", currentUserEmail);

            using (UnityWebRequest www = UnityWebRequest.Post("https://animated-personality.com.tr/unity_add_allowed_user.php", form))
            {
                www.SetRequestHeader("User-Agent", "DefaultBrowser");
                www.SetRequestHeader("Cookie", string.Format("DummyCookie"));
                www.chunkedTransfer = false;

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    CheckUserAllowed();
                }
            }
        }
    }

    IEnumerator CheckUserAllowedRoutine()
    {
        loginError.SetActive(false);

        WWWForm form = new WWWForm();

        currentUserEmail = userMailText.text;
        form.AddField("usermail", currentUserEmail);

        using (UnityWebRequest www = UnityWebRequest.Post("https://animated-personality.com.tr/unity_check_user_allowed.php", form))
        {
            www.SetRequestHeader("User-Agent", "DefaultBrowser");
            www.SetRequestHeader("Cookie", string.Format("DummyCookie"));
            www.chunkedTransfer = false;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                if (www.downloadHandler.text.Equals("no"))
                {
                    UserNotAllowed();
                }
                else
                {
                    string[] parts = www.downloadHandler.text.Split("|");
                    if (parts.Length > 1 && parts[0].Equals("yes"))
                    {
                        int.TryParse(parts[1], out currentVariation);
                        Debug.Log("Current Variation: " + currentVariation);
                        UserAllowed();
                    }
                }
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    IEnumerator AddDialogueRoutine(string content)
    {
        WWWForm form = new WWWForm();

        form.AddField("usermail", currentUserEmail);
        form.AddField("content", content);
        Debug.Log(currentUserEmail  + " " + content);

        using (UnityWebRequest www = UnityWebRequest.Post("https://animated-personality.com.tr/unity_add_dialogue.php", form))
        {
            www.SetRequestHeader("User-Agent", "DefaultBrowser");
            www.SetRequestHeader("Cookie", string.Format("DummyCookie"));
            www.chunkedTransfer = false;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
        }
    }

    IEnumerator AddResultsRoutine(bool isSurvey)
    {
        WWWForm form = new WWWForm();

        string answersString = "";

        if(isSurvey)
        {
            for (int i = 0; i < surveyAnswers.Length; i++)
            {
                answersString += (i + ":"+ surveyAnswers[i] + ",");
            }

            answersString += ("agent:" + currentAgent + ",");
            answersString += ("variation:" + currentVariation + ",");
            answersString += ("topic:" + topicName + ",");
            answersString += ("time:" + Mathf.RoundToInt(timeElapsed) + ",");
            answersString += ("voice:" + selectedVoice.Name + ".");

            // Debug.Log(answersString);
        }
        else
        {
            answersString += "1:" + vqa1.text + "#";
            answersString += "2:" + vqa2.text + "#";
            answersString += "3:" + vqa3.text + "#";
            answersString += "4:" + vqa4.text + "#";
            answersString += "5:" + vqa5.text + "#";

            // replace new lines for better formatting
            //surveyAnswers.Replace("\r", string.Empty);
            //surveyAnswers.Replace("\n", string.Empty);

            answersString += ("agent:" + currentAgent + ",");
            answersString += ("variation:" + currentVariation + ",");
            answersString += ("topic:" + topicName + ",");
            answersString += ("time:" + Mathf.RoundToInt(timeElapsed) + ",");
            answersString += ("voice:" + selectedVoice.Name + ".");
        }

        form.AddField("usermail", currentUserEmail);
        form.AddField("surveyanswers", answersString);

        using (UnityWebRequest www = UnityWebRequest.Post("https://animated-personality.com.tr/unity_add_results.php", form))
        {
            www.SetRequestHeader("User-Agent", "DefaultBrowser");
            www.SetRequestHeader("Cookie", string.Format("DummyCookie"));
            www.chunkedTransfer = false;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                if(isSurvey)
                {
                    CompletedSurveyQuestionsGoToVerbalQuestions();
                }
                else
                {
                    CompletedStudyThanks();
                }
            }
        }
    }

    IEnumerator CheckUserHasDemographicsRoutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("usermail", currentUserEmail);

        using (UnityWebRequest www = UnityWebRequest.Post("https://animated-personality.com.tr/unity_check_user_has_demographics.php", form))
        {
            www.SetRequestHeader("User-Agent", "DefaultBrowser");
            www.SetRequestHeader("Cookie", string.Format("DummyCookie"));
            www.chunkedTransfer = false;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                if(www.downloadHandler.text.Equals("no"))
                {
                    UserDoesNotHaveDemographics();
                }
                else if (www.downloadHandler.text.Equals("yes"))
                {
                    UserHasDemographics();
                }
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    IEnumerator EnterDemographicsPost()
    {
        int age = 0;
        string gender = "none";
        string country = "none";

        if (!ageText.text.Equals(""))
        {
            string numString = ageText.text;
            int.TryParse(numString, out age);
        }

        if (!genderText.text.Equals(""))
        {
            gender = genderText.text;
        }

        if (!countryText.text.Equals(""))
        {
            country = countryText.text;
        }

        WWWForm form = new WWWForm();
        form.AddField("usermail", currentUserEmail);
        form.AddField("age", age);
        form.AddField("gender", gender);
        form.AddField("country", country);

        using (UnityWebRequest www = UnityWebRequest.Post("https://animated-personality.com.tr/unity_add_user_demographics.php", form))
        {
            www.SetRequestHeader("User-Agent", "DefaultBrowser");
            www.SetRequestHeader("Cookie", string.Format("DummyCookie"));
            www.chunkedTransfer = false;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                // AfterEnteringDemographics();
            }
        }
    }

    public void CompletedStudyThanks()
    {
        currentScreen = ScreenName.THANKS;
        verbalQuestionsPanel.SetActive(false);
        thanksPanel.SetActive(true);
    }

    public void CompletedSurveyQuestionsGoToVerbalQuestions()
    {
        currentScreen = ScreenName.BEFROEVERBAL;
        panelBackground.SetActive(true);
        verbalQuestionsBeforePanel.SetActive(true);
    }

    public void SendResults(bool isSurvey)
    {
        StartCoroutine(AddResultsRoutine(isSurvey));
    }

    public void AddToken(int usage)
    {
        StartCoroutine(AddTokenRoutine(usage));
    }

    public void CheckToken()
    {
        StartCoroutine(CheckTokenRoutine());
    } 
    
    public void AddDialogue(string content)
    {
        StartCoroutine(AddDialogueRoutine(content));
    }

    public void CheckUserDemographics()
    {
        StartCoroutine(CheckUserHasDemographicsRoutine());
    }

    public void AddAllowedUser()
    {
        StartCoroutine(AddAllowedUserRoutine());
    }

    public void CheckUserAllowed()
    {
        StartCoroutine(CheckUserAllowedRoutine());
    }

    public void EnterDemographics()
    {
        StartCoroutine(EnterDemographicsPost());
    }

    public void AfterEnteringDemographics()
    {
        currentScreen = ScreenName.TOPICCHOOSE;
        demographicsPanel.SetActive(false);
        topicChoosePanel.SetActive(true);
    }

    public void AfterChoosingTopic()
    {
        topicChoosePanel.SetActive(false);
        loadingPanel.SetActive(true);
        SetForVariation(currentVariation);
    }

    public void SetForVariation(int var)
    {
        switch(var)
        {
            case 0:
                SetAgentOn(false);
                SetPersonality(-1);
                break;
            case 1:
                SetAgentOn(false);
                SetPersonality(1);
                break;
            case 2:
                SetAgentOn(true);
                SetModifications(false);
                SetPersonality(-1);
                break;
            case 3:
                SetAgentOn(true);
                SetModifications(false);
                SetPersonality(1);
                break;
            case 4:
                SetAgentOn(true);
                SetModifications(true);
                SetPersonality(-1);
                break;
            case 5:
                SetAgentOn(true);
                SetModifications(true);
                SetPersonality(1);
                break;
            default:
                SetAgentOn(true);
                SetModifications(true);
                SetPersonality(1);
                break;
        }

        panelBackground.SetActive(false);
        currentScreen = ScreenName.HELP;
        helpPanel.SetActive(true);
    }

    public void UserAllowed()
    {
        EnterDemographics();
        currentScreen = ScreenName.CONDITIONS;
        loginPanel.SetActive(false);
        conditionsPanel.SetActive(true);
    }

    public void UserNotAllowed()
    {
        // if not allowed, show error message
        loginError.SetActive(true);
    }

    public void UserHasDemographics()
    {
        AfterEnteringDemographics();
    }

    public void UserDoesNotHaveDemographics()
    {
        currentScreen = ScreenName.DEMOGRAPHICS;
        loadingPanel.SetActive(false);
        demographicsPanel.SetActive(true);
    }

    private ChatMessage originalMessage;
    private void StartConversation(bool isPositive)
    {
        SetOriginalMessage(isPositive);
        messages = new List<ChatMessage> {
            originalMessage
        };

        dialogueInputField.text = "";
    }

    private void SetOriginalMessage(bool isPositive)
    {
        if(isPositive)
        {
            originalMessage = new ChatMessage(ChatMessageRole.System, "Act as an extraverted teacher teaching about " + topicName + ", give friendly and polite answers, your name is " + agentName + ".");
        }
        else
        {
            originalMessage = new ChatMessage(ChatMessageRole.System, "Act as an intraverted teacher teaching about " + topicName + ", give short and less friendly answers, your name is " + agentName + ".");
        }
    }

    private async void GetResponse()
    {
        if (dialogueInputField.text.Length < 1)
        {
            return;
        }

        if(currentTokenUsage > maxTokenUsageLimit)
        {
            Speak("Sorry, we passed the allocated conversation limit. Please proceed to answer the survey questions.");
            return;
        }

        // Disable the OK button
        DisableAsking();
        
        // Fill the user message from the input field
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = dialogueInputField.text;
        if (userMessage.Content.Length > 500)
        {
            // Limit messages to 500 characters
            userMessage.Content = userMessage.Content.Substring(0, 500);
        }
        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        AddDialogue("U: " + userMessage.Content);
        AddChatHistory(userMessage.Content, false);

        // Add the message to the list
        messages.Add(userMessage);

        // get sublist of last 5 messages
        int lastMessageCount = 5;
        List<ChatMessage> subList = new List<ChatMessage>();
        subList.Add(originalMessage);
        int messagesIndex = messages.Count - lastMessageCount;
        for(int i = 0; i < lastMessageCount; i++)
        {
            if(messagesIndex > 0 && messagesIndex < messages.Count)
            {
                subList.Add(messages[messagesIndex]);
                Debug.Log(messages[messagesIndex].Content);
            }
            messagesIndex++;
        }

        try {
            // Send the entire chat to OpenAI to get the next message
            var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.9,
                MaxTokens = 750,
                Messages = subList
            });

            // Get the response message
            ChatMessage responseMessage = new ChatMessage();
            responseMessage.Role = chatResult.Choices[0].Message.Role;
            responseMessage.Content = chatResult.Choices[0].Message.Content;

            // say it
            AddDialogue("C: " + chatResult.Choices[0].Message.Content);
            AddChatHistory(chatResult.Choices[0].Message.Content, true);
            AddToken(chatResult.Usage.TotalTokens);
            questionsAskedToAgent++;

            if (questionsAskedToAgent >= requiredAskLimit)
            {
                askedAtLeast10Text.color = Color.white;
                askedAtLeast10Text.text = "You asked at least " + requiredAskLimit + " questions, you can fill the survey now.";
                askedAtLeast10OnScreenText.text = "You asked " + questionsAskedToAgent + " questions, you can start answering the survey questions.";
                SurveyButtonsSetInteract(true);
                toggleQuestionsPanelButton.GetComponent<Image>().color = new Color(72f / 255, 255f / 255, 72f / 255, 101f / 255);
            }
            else
            {
                askedAtLeast10OnScreenText.text = "You asked " + questionsAskedToAgent + " questions, ask " + (requiredAskLimit-questionsAskedToAgent) + " more before answering survey questions.";
            }

            SpeakPartial(chatResult.Choices[0].Message.Content);

            Debug.Log("Total Tokens:" + chatResult.Usage.TotalTokens);
            Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));

            // Add the response to the list of messages
            messages.Add(responseMessage);


            CheckToken();
        }
        catch(HttpRequestException e)
        {
            SpeakPartial("There is a problem with the OpenAI servers, please try again later. You can still answer the study questions if you asked at least 5 questions.");
            Debug.Log(e);
        }
        
    }

    public void Button_O_Positive()
    {
        agent.SetPersonality(OCEAN.O_pos);
        OnAgentPersonalityChange();
    }

    public void Button_O_Negative()
    {
        agent.SetPersonality(OCEAN.O_neg);
        OnAgentPersonalityChange();
    }

    public void Button_C_Positive()
    {
        agent.SetPersonality(OCEAN.C_pos);
        OnAgentPersonalityChange();
    }

    public void Button_C_Negative()
    {
        agent.SetPersonality(OCEAN.C_neg);
        OnAgentPersonalityChange();
    }

    public void Button_E_Positive()
    {
        agent.SetPersonality(OCEAN.E_pos);
        OnAgentPersonalityChange();
    }

    public void Button_E_Negative()
    {
        agent.SetPersonality(OCEAN.E_neg);
        OnAgentPersonalityChange();
    }

    public void Button_A_Positive()
    {
        agent.SetPersonality(OCEAN.A_pos);
        OnAgentPersonalityChange();
    }

    public void Button_A_Negative()
    {
        agent.SetPersonality(OCEAN.A_neg);
        OnAgentPersonalityChange();
    }

    public void Button_N_Positive()
    {
        agent.SetPersonality(OCEAN.N_pos);
        OnAgentPersonalityChange();
    }

    public void Button_N_Negative()
    {
        agent.SetPersonality(OCEAN.N_neg);
        OnAgentPersonalityChange();
    }

    private void OnAgentPersonalityChange()
    {
        agent.C_Fluctuation = true;
        agent.C_LabanIK = true;
        agent.C_LabanRotation = true;
        agent.C_SpeedAdjust = true;

        agent.Map_OCEAN_to_Additional = true;
        agent.Map_OCEAN_to_LabanEffort = true;
        agent.Map_OCEAN_to_LabanShape = true;
    }

    public AgentController agent;
    public AudioSource aSource;
    // public uLipSync.uLipSync lipSync;

    private string[] speakParts;
    private int currentPart;

    public void SpeakPartial(string completeSpeech)
    {
        speakParts = Regex.Split(completeSpeech, @"(?<=[a-zA-Z]+[a-zA-Z]+[.!?]|[\n])");
        if(speakParts.Length > 0 )
        {
            currentPart = 0;
            string line = speakParts[currentPart].Trim();
            if(line.Length > 0)
            {
                Speak(line);
            }
            else
            {
                SpeakPartialEnd();
            }
        }
        else
        {
            // nothing to speak
            AgentEndSpeakAnimation();
        }
    }

    public void SpeakPartialEnd()
    {
        line1.text = "";
        line2.text = "";
        line3.text = "";
        line4.text = "";
        line5.text = "";

        currentPart++;
        if (speakParts.Length > currentPart)
        {
            string line = speakParts[currentPart].Trim();
            if (line.Length > 0)
            {
                Speak(line);
            }
            else
            {
                SpeakPartialEnd();
            }
        }
        else
        {
            // nothing to speak
            AgentEndSpeakAnimation();
        }

    }

    private int deterministicAnimationNo = 0;

    private void AgentStartTalkAnimation()
    {
        agentTalking = true;
        if(agent != null)
        {
            if(deterministicMode)
            {
                agent.SetAnimation(deterministicAnimationNo + 1);
                deterministicAnimationNo = ((deterministicAnimationNo + 1) % 8);
                agent.SetAnimationParameter("FaceNo", 0);
            }
            else
            {
                agent.SetAnimation(Random.Range(1, 9));
                agent.SetAnimationParameter("FaceNo", 0);
            }
        }
    }

    private void AgentEndSpeakAnimation()
    {
        EnableAsking();
        agentTalking = false;
        if (agent != null)
        {
            agent.SetAnimation(0);
            if(modifications)
            {
                if (personality > 0)
                {
                    agent.SetAnimationParameter("FaceNo", 1);
                }
                else
                {
                    agent.SetAnimationParameter("FaceNo", 2);
                }
            }
            else
            {
                agent.SetAnimationParameter("FaceNo", 0);
            }
        }
    }

    public void DisableAsking()
    {
        dialogueInputField.interactable = false;
        dialogueSayButton.interactable = false;
        dialogueSayButton.GetComponent<Image>().color = new Color(255f / 255, 72f / 255, 72f / 255, 101f / 255); 
        dialogueInputText.color = Color.green;
        askingEnabled = false;

        for (int i = 0; i < compChatHistoryButtons.Count; i++)
        {
            compChatHistoryButtons[i].interactable = false;
        }
    }

    public void EnableAsking()
    {
        dialogueInputField.interactable = true;
        agentResponseText.text = "";
        dialogueInputField.text = "";
        dialogueSayButton.interactable = true;
        dialogueSayButton.GetComponent<Image>().color = new Color(72f / 255, 255f / 255, 72f / 255, 101f / 255);
        dialogueInputText.color = Color.white;
        askingEnabled = true;

        for(int i = 0; i < compChatHistoryButtons.Count; i++)
        {
            compChatHistoryButtons[i].interactable = true;
        }
    }

    private void speakCurrentWordMethod(Crosstales.RTVoice.Model.Wrapper wrapper, string[] speechTextArray, int wordIndex)
    {
        if(wordIndex - 2 >= 0 && wordIndex - 2 < speechTextArray.Length)
        {
            line1.text = speechTextArray[wordIndex - 2];
        }
        else
        {
            line1.text = "";
        }

        if (wordIndex - 1 >= 0 && wordIndex - 1 < speechTextArray.Length)
        {
            line2.text = speechTextArray[wordIndex - 1];
        }
        else
        {
            line2.text = "";
        }

        line3.text = speechTextArray[wordIndex];

        if (wordIndex + 1 >= 0 && wordIndex + 1 < speechTextArray.Length)
        {
            line4.text = speechTextArray[wordIndex + 1];
        }
        else
        {
            line4.text = "";
        }

        if (wordIndex + 2 >= 0 && wordIndex + 2 < speechTextArray.Length)
        {
            line5.text = speechTextArray[wordIndex + 2];
        }
        else
        {
            line5.text = "";
        }
        /*
        for (int i = wordIndex - 2; i <= wordIndex + 2; i++)
        {
            if(i >= 0 && i < speechTextArray.Length)
            {
                textOnScreen += speechTextArray[i] + " ";
            }

        }

        agentResponseText.text = textOnScreen;*/
    }

    public bool isMale;
    public void Speak(string line)
    {
        if(selectedVoice != null)
        {
            Speaker.Instance.Silence();
            Speaker.Instance.StopAllCoroutines();

            Speaker.Instance.Speak(line, aSource, selectedVoice);
            Speaker.Instance.SpeakNative(line, selectedVoice, volume: 0);
        }

        AgentStartTalkAnimation();

        agentResponseText.text = line;
    }


    public void SpeakB1()
    {
        /*string line = inText.text;
        SpeakWithRT(line);*/

        GetResponse();
    }

    public void TestSpeak()
    {
        Speak(dialogueInputField.text);
        dialogueInputField.text = "";

        /*string line = inText.text;
        SpeakWithSpeechGeneration(line);*/
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if(currentScreen == ScreenName.THANKS)
        {
            prolificCodeText.text = "CW4ANVW1";
        }

        if(agent != null && agentOn)
        {
            if(personality > 0)
            {
                agent.IKFAC_side = personality * 0.3f;
                agent.IKFAC_up = personality * 0.3f;
                agent.space = personality;
                agent.weight = personality;
                agent.time = personality * 0.2f;
                agent.flow = personality * 0.3f;
                
                agent.spine_bend = -personality * .1f;
                agent.head_bend = personality;
            }
            else
            {
                agent.IKFAC_side = personality * 0.25f;
                agent.IKFAC_up = personality * 0.4f;
                agent.space = personality;
                agent.weight = personality;
                agent.time = personality * 0.6f;
                agent.flow = personality * 0.3f;

                agent.spine_bend = -personality * .25f;
                agent.head_bend = -personality;
            }
            
            agent.C_LabanRotation = modifications;
            agent.C_LabanIK = modifications;
            agent.C_Fluctuation = modifications;
            agent.C_SpeedAdjust = modifications;

            if(agent.GetAnimator() != null && agent.GetAnimator().GetCurrentAnimatorStateInfo(1).normalizedTime >= 1.0)
            {
                if(agentTalking)
                {
                    agent.SetAnimation(Random.Range(1, 9));
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (currentScreen == ScreenName.LOGIN)
            {
                AddAllowedUser();
            }
            else if (currentScreen == ScreenName.CONDITIONS)
            {
                Button_AgreeStudyConditions();
            }
            else if (currentScreen == ScreenName.DEMOGRAPHICS)
            {
                EnterDemographics();
            }
            else if (currentScreen == ScreenName.HELP)
            {
                Button_HideHelp();
            }
            else if (currentScreen == ScreenName.BEFROEVERBAL)
            {
                Button_ContinueWithVerbalQuestions();
            }
            else if (currentScreen == ScreenName.AGENT && askingEnabled)
            {
                SpeakB1();
            }

        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Button_Quit();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            if (selectedVoice != null)
            {
                Speaker.Instance.Silence();
                Speaker.Instance.StopAllCoroutines();
                AgentEndSpeakAnimation();
            }
        }

        
        else if (Input.GetKeyDown(KeyCode.F1))
        {
            SetPersonality(-1);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            SetPersonality(1);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            SetModifications(false);
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            SetModifications(true);
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            SetAgentOn(false);
        }
        else if (Input.GetKeyDown(KeyCode.F6))
        {
            SetAgentOn(true);
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            ChangeAgent(1);
        }
        else if (Input.GetKeyDown(KeyCode.F8))
        {
            ChangeAgent(2);
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            ChangeAgent(3);
        }
        else if (Input.GetKeyDown(KeyCode.F10))
        {
            ChangeAgent(4);
        }
        else if (Input.GetKeyDown(KeyCode.F11))
        {
            canvas.SetActive(false);
            deterministicAnimationNo = 0;

            subtitlesOn = false;
            newResponsePanel.SetActive(subtitlesOn);
            if (subtitlesOn)
            {
                subtitlesButton.GetComponentInChildren<Text>().text = "Hide\nSubtitles";
            }
            else
            {
                subtitlesButton.GetComponentInChildren<Text>().text = "Show\nSubtitles";
            }

            deterministicMode = true;
            selectedVoice = supportedVoices[2];
            isMale = false;

            SetPersonality(-1);
            SetModifications(true);
            ChangeAgent(1);

            SpeakPartial("We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis.");
            
        }
        else if (Input.GetKeyDown(KeyCode.F12))
        {
            canvas.SetActive(false);
            deterministicAnimationNo = 0;

            subtitlesOn = false;
            newResponsePanel.SetActive(subtitlesOn);
            if (subtitlesOn)
            {
                subtitlesButton.GetComponentInChildren<Text>().text = "Hide\nSubtitles";
            }
            else
            {
                subtitlesButton.GetComponentInChildren<Text>().text = "Show\nSubtitles";
            }

            deterministicMode = true;
            selectedVoice = supportedVoices[2];
            isMale = false;

            SetPersonality(1);
            SetModifications(true);
            ChangeAgent(1);

            SpeakPartial("We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis. We will publicly share our data and conversational agent system for further studies and analysis.");

        }
        else if( Input.GetKeyDown(KeyCode.Q))
        {
            cam1.SetActive(false);
            cam2.SetActive(true);
            SpeakPartial("Stop.");
        }


    }

    private void SetPersonality(float personality)
    {
        this.personality = personality;
        if (personality > 0)
        {
            StartConversation(true);
            if(agent != null)
            {
                if (!agentTalking && modifications)
                {
                    agent.SetAnimationParameter("FaceNo", 1);
                }
                else
                {
                    agent.SetAnimationParameter("FaceNo", 0);
                }
            }
        }
        else
        {
            StartConversation(false);
            if (agent != null)
            { 
                if (!agentTalking && modifications)
                {
                    agent.SetAnimationParameter("FaceNo", 2);
                }
                else
                {
                    agent.SetAnimationParameter("FaceNo", 0);
                }
            }
        }
    }

    private bool modifications;
    private void SetModifications(bool modifications)
    {
        this.modifications = modifications;
    }

    private bool agentOn = true;
    private void SetAgentOn(bool agentOn)
    {
        this.agentOn = agentOn;
        if(agentOn)
        {
            newResponsePanel.SetActive(true);
            agentResponseText.gameObject.transform.parent.gameObject.SetActive(false);
            subtitlesButton.gameObject.SetActive(true);
            subtitlesHelpText.gameObject.SetActive(true);

            if (agent != null) agent.gameObject.SetActive(true);
        }
        else
        {
            newResponsePanel.SetActive(false);
            agentResponseText.gameObject.transform.parent.gameObject.SetActive(true);
            subtitlesButton.gameObject.SetActive(false);
            subtitlesHelpText.gameObject.SetActive(false);

            if (agent != null) agent.gameObject.SetActive(false);
        }
    }

    private int currentAgent = 0;

    public void ChangeAgent(int agentNo) {
        currentAgent = agentNo;

        agent2.SetActive(false);
        agent4.SetActive(false);

        if(currentAgent == 1)
        {
            agent2.SetActive(true);
            lipSyncPasser.currentFace = agent2.GetComponent<FaceScriptCC>();
            agent = agent2.GetComponent<AgentController>();
            body2.gameObject.GetComponent<Renderer>().materials[0].SetTexture("_DiffuseMap", textures1[0]);
            body2.gameObject.GetComponent<Renderer>().materials[1].SetTexture("_DiffuseMap", textures1[1]);
            body2.gameObject.GetComponent<Renderer>().materials[2].SetTexture("_DiffuseMap", textures1[2]);
            body2.gameObject.GetComponent<Renderer>().materials[3].SetTexture("_DiffuseMap", textures1[3]);
        }
        else if(currentAgent == 2)
        {
            agent2.SetActive(true);
            lipSyncPasser.currentFace = agent2.GetComponent<FaceScriptCC>();
            agent = agent2.GetComponent<AgentController>();
            body2.gameObject.GetComponent<Renderer>().materials[0].SetTexture("_DiffuseMap", textures2[0]);
            body2.gameObject.GetComponent<Renderer>().materials[1].SetTexture("_DiffuseMap", textures2[1]);
            body2.gameObject.GetComponent<Renderer>().materials[2].SetTexture("_DiffuseMap", textures2[2]);
            body2.gameObject.GetComponent<Renderer>().materials[3].SetTexture("_DiffuseMap", textures2[3]);
        }
        else if (currentAgent == 3)
        {
            body4.gameObject.GetComponent<Renderer>().materials[0].SetTexture("_DiffuseMap", textures3[0]);
            body4.gameObject.GetComponent<Renderer>().materials[1].SetTexture("_DiffuseMap", textures3[1]);
            body4.gameObject.GetComponent<Renderer>().materials[2].SetTexture("_DiffuseMap", textures3[2]);
            body4.gameObject.GetComponent<Renderer>().materials[3].SetTexture("_DiffuseMap", textures3[3]);

            agent4.SetActive(true);
            lipSyncPasser.currentFace = agent4.GetComponent<FaceScriptCC>();
            agent = agent4.GetComponent<AgentController>();
        }
        else if (currentAgent == 4)
        {
            body4.gameObject.GetComponent<Renderer>().materials[0].SetTexture("_DiffuseMap", textures4[0]);
            body4.gameObject.GetComponent<Renderer>().materials[1].SetTexture("_DiffuseMap", textures4[1]);
            body4.gameObject.GetComponent<Renderer>().materials[2].SetTexture("_DiffuseMap", textures4[2]);
            body4.gameObject.GetComponent<Renderer>().materials[3].SetTexture("_DiffuseMap", textures4[3]);

            agent4.SetActive(true);
            lipSyncPasser.currentFace = agent4.GetComponent<FaceScriptCC>();
            agent = agent4.GetComponent<AgentController>();
        }

        if(isMale)
        {
            agentName = "John";
        }
        else
        {
            agentName = "Jane";
        }

        animatorInspector.anim = agent.GetComponent<Animator>();
        // agent.lookObject = Camera.current.gameObject;

        SetPersonality(personality);
        // agent.C_LookIK = true;
    }

    public void Button_Quit()
    {
        if(currentScreen == ScreenName.THANKS)
        {
            Application.Quit();
        }
        else
        {
            quitSurePanel.SetActive(true);
        }
    }

    public void Button_QuitSure()
    {
        Application.Quit();
    }

    public void Button_ReturnToStudy()
    {
        quitSurePanel.SetActive(false);
    }
    
    public void Button_ToggleChatHistory()
    {
        chatHistoryOn = !chatHistoryOn;
        chatPanel.SetActive(chatHistoryOn);
        if(chatHistoryOn)
        {
            toggleChatHistoryButton.GetComponentInChildren<Text>().text = "Hide\nChat\nHistory";
            toggleChatHistoryButton.GetComponent<Image>().color = new Color(255f / 255, 72f / 255, 72f / 255, 101f / 255);
            if (studyQuestionsOn)
            {
                Button_ToggleSurveyQuestions();
            }
        }
        else
        {
            toggleChatHistoryButton.GetComponentInChildren<Text>().text = "Show\nChat\nHistory";
            toggleChatHistoryButton.GetComponent<Image>().color = new Color(72f/255, 72f/255, 72f/255, 101f / 255);
        }
    }

    public void Button_ToggleSurveyQuestions()
    {
        studyQuestionsOn = !studyQuestionsOn;
        questionsPanel.SetActive(studyQuestionsOn);
        if (studyQuestionsOn)
        {
            toggleQuestionsPanelButton.GetComponentInChildren<Text>().text = "Hide\nSurvey\nQuestions";
            toggleQuestionsPanelButton.GetComponent<Image>().color = new Color(255f / 255, 72f / 255, 72f / 255, 101f / 255);
            if (chatHistoryOn)
            {
                Button_ToggleChatHistory();
            }
        }
        else
        {
            toggleQuestionsPanelButton.GetComponentInChildren<Text>().text = "Show\nSurvey\nQuestions";
            if (questionsAskedToAgent >= requiredAskLimit)
            {
                toggleQuestionsPanelButton.GetComponent<Image>().color = new Color(72f / 255, 255f / 255, 72f / 255, 101f / 255);
            }
            else
            {
                toggleQuestionsPanelButton.GetComponent<Image>().color = new Color(72f / 255, 72f / 255, 72f / 255, 101f / 255);
            }
        }
    }

    public void Button_ToggleSubtitles()
    {
        subtitlesOn = !subtitlesOn;
        // agentResponseText.gameObject.SetActive(subtitlesOn);
        newResponsePanel.SetActive(subtitlesOn);
        if (subtitlesOn)
        {
            subtitlesButton.GetComponentInChildren<Text>().text = "Hide\nSubtitles";
        }
        else
        {
            subtitlesButton.GetComponentInChildren<Text>().text = "Show\nSubtitles";
        }
    }

    public void Button_AgreeStudyConditions()
    {
        conditionsPanel.SetActive(false);
        AfterEnteringDemographics();
        //CheckUserDemographics();
    }

    public void Button_SubmitSurveyAnswers()
    {
        SendResults(true);
    }

    public void Button_SubmitVerbalAnswers()
    {
        SendResults(false);
    }

    public void InputField_CheckVerbalAnswersFilled()
    {
        if (vqa1.text.Length > 0
            && vqa2.text.Length > 0
            && vqa3.text.Length > 0
            && vqa4.text.Length > 0
            && vqa5.text.Length > 0)
        {
            verbalQuestionsSubmitButton.interactable = true;
            verbalQuestionsSubmitButton.gameObject.GetComponent<Image>().color = new Color(72f / 255, 255f / 255, 72f / 255, 101f / 255);
            verbalQuestionsAnswer5Text.text = "You answered the 5 questions and are now eligible to submit.";
            verbalQuestionsAnswer5Text.color = Color.white;
        }
        else
        {
            verbalQuestionsSubmitButton.interactable = false;
            verbalQuestionsSubmitButton.gameObject.GetComponent<Image>().color = new Color(255f / 255, 72f / 255, 72f / 255, 101f / 255);
            verbalQuestionsAnswer5Text.text = "Please answer all 5 questions";
            verbalQuestionsAnswer5Text.color = Color.red;
        }

    }

    public void Button_HideHelp()
    {
        currentScreen = ScreenName.AGENT;
        SpeakPartial("Hello, I am a conversational agent that will answer your questions on " + topicName.ToLower() + "; please ask your questions about this subject using the input field below. You can view the survey questions using the button on the top right corner. You can answer the survey questions after asking me at least 5 questions. Please tell me, what do you want to know about " + topicName.ToLower() + "?");
        // EnableAsking();
        helpPanel.SetActive(false);
    }

    public void Button_ContinueWithVerbalQuestions()
    {
        Speaker.Instance.Silence();
        currentScreen = ScreenName.VERBAL;
        verbalQuestionsPanel.SetActive(true);
        verbalQuestionsBeforePanel.SetActive(false);
    }

    public void Button_DirectToProlific()
    {
        GUIUtility.systemCopyBuffer = "CW4ANVW1";
        Application.OpenURL("https://app.prolific.com/submissions/complete?cc=CW4ANVW1");
    }

    public void Button_LinkAndClose()
    {
        Application.OpenURL("https://support.microsoft.com/en-us/windows/language-packs-for-windows-a5094319-a92d-18de-5b53-1cfc697cfca8");
        Application.Quit();
    }

}
