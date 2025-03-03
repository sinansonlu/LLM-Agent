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
using TMPro;

public class AltLogic : MonoBehaviour
{
    public TextMeshProUGUI textLeft;
    public TextMeshProUGUI textRight;
    public TextMeshProUGUI numberState;
    private int isLeftMode = 0;

    public GameObject leftHold;
    public GameObject rightHold;

    public TextMeshProUGUI top_textLeft;
    public TextMeshProUGUI top_textRight;

    bool working = true;
    float timer_wait = 1f;
    int state = 0;

    public Texture[] textures1;
    public Texture[] textures2;
    public GameObject body2;
    public GameObject body2_copy;
    public Texture[] textures3;
    public Texture[] textures4;
    public GameObject body4;
    public GameObject body4_copy;
  
    [Header("Agent LipSync")]
    public MyVRLipSyncPasser lipSyncPasser;
    public GameObject agent2;
    public GameObject agent4;
    public GameObject agent2copy;
    public GameObject agent4copy;
    public AnimatorInspector animatorInspector;

    [Header("Personality")]
    [Range(-1f, 1f)] public float personality;

    [Header("Scene Lights")]
    public GameObject sceneLights;

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
            selectedVoice = supportedVoices[0];
            isMale = false;
            ChangeAgent(1);
            initAll();
            
        }
    }

    private void initAll()
    {
        sceneLights.SetActive(true);
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

        agentCopy.C_Fluctuation = true;
        agentCopy.C_LabanIK = true;
        agentCopy.C_LabanRotation = true;
        agentCopy.C_SpeedAdjust = true;

        agentCopy.Map_OCEAN_to_Additional = true;
        agentCopy.Map_OCEAN_to_LabanEffort = true;
        agentCopy.Map_OCEAN_to_LabanShape = true;
    }

    public AgentController agent;
    public AgentController agentCopy;
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
    private int deterministicAnimationNoCopy = 0;

    bool agentTalking;

    private void AgentStartTalkAnimation()
    {
        agentTalking = true;
        if(agent != null)
        {
            agent.SetAnimation(deterministicAnimationNo + 1);
            agentCopy.SetAnimation(deterministicAnimationNoCopy + 1);
            deterministicAnimationNo = ((deterministicAnimationNo + 1) % 8);
            deterministicAnimationNoCopy = ((deterministicAnimationNoCopy + 1) % 8);
            // agent.SetAnimationParameter("FaceNo", 0);
        }
    }
    private void AgentEndSpeakAnimation()
    {
        working = false;
        
        agentTalking = false;
        if (agent != null)
        {
            agent.SetAnimation(0);
            agentCopy.SetAnimation(0);
            /*
            if (personality > 0)
            {
                agent.SetAnimationParameter("FaceNo", 1);
                agentCopy.SetAnimationParameter("FaceNo", 2);
            }
            else
            {
                agent.SetAnimationParameter("FaceNo", 2);
                agentCopy.SetAnimationParameter("FaceNo", 1);
            }
            */
        }
    }

    private void speakCurrentWordMethod(Crosstales.RTVoice.Model.Wrapper wrapper, string[] speechTextArray, int wordIndex)
    {
        if (wordIndex == speechTextArray.Length -1) {
            Invoke("SpeakPartialEnd", 0.8f);
        }
    }

    public bool isMale;
    public void Speak(string line)
    {
        if(selectedVoice != null)
        {
            if (isLeftMode == 0)
            {
                textLeft.text = line;
                top_textLeft.text = line;
                textRight.text = "";
                top_textRight.text = "";
            }

            if (isLeftMode == 1)
            {
                textLeft.text = "";
                top_textLeft.text = "";
                textRight.text = line;
                top_textRight.text = line;
            }

            if(isLeftMode == 2)
            {
                textLeft.text = line;
                top_textLeft.text = line;
                textRight.text = line;
                top_textRight.text = line;
            }

            Speaker.Instance.Silence();
            Speaker.Instance.StopAllCoroutines();

            Speaker.Instance.Speak(line, aSource, selectedVoice);
            Speaker.Instance.SpeakNative(line, selectedVoice, volume: 0);
        }

        if(!agentTalking) AgentStartTalkAnimation();

    }


    public void SpeakB1()
    {
        /*string line = inText.text;
        SpeakWithRT(line);*/

    }

    public void TestSpeak()
    {
        Speak("Hey, hello!");

        /*string line = inText.text;
        SpeakWithSpeechGeneration(line);*/
    }

    float waitTimer1, waitTimer2;

    void setMode(int mode)
    {
        deterministicAnimationNo = 0;
        deterministicAnimationNoCopy = 0;

        SetPersonality(personality);

        isLeftMode = mode;

        if(openHolds)
        {
            if (mode == 0)
            {
                leftHold.SetActive(false);
                rightHold.SetActive(true);
            }
            else if (mode == 1)
            {
                leftHold.SetActive(true);
                rightHold.SetActive(false);
            }
            else if (mode == 2)
            {
                leftHold.SetActive(false);
                rightHold.SetActive(false);
            }
        }
        /*else
        {
            leftHold.SetActive(true);
            rightHold.SetActive(true);
        }*/
    }

    bool reversed = false;

    void Update()
    {
        if (!working) {
            timer_wait -= Time.deltaTime;
            numberState.text = "" + ((state+1) / 2);

            if (timer_wait > 3 && timer_wait < 4)
            {
                if (state % 2 == 0)
                {
                    numberState.enabled = true;
                }
            }

            if (timer_wait < 1)
            {
                numberState.enabled = false;
            }

            if (timer_wait < 0)
            {
                state++;

                if (state % 2 == 1)
                {
                    timer_wait = 1f;
                }
                else
                {
                    timer_wait = 5f;
                }
                
                working = true;

                if (!shortcrc)
                {
                    if (!reversed)
                    {
                        switch (state)
                        {
                            case 1:
                                setMode(0);
                                SpeakPartial("Hello! Quantum computing is an exciting and cutting-edge branch of information technology that utilizes principles of quantum mechanics to perform computations. Unlike classical computers that use bits to represent information as either 0 or 1, quantum computers use quantum bits, or qubits, which can represent 0, 1, or any superposition of these states, allowing for incredibly fast and complex calculations. It's like harnessing the power of quantum physics to solve problems that are currently beyond the capabilities of classical computers. Exciting stuff, right? Let me know if you have any other questions!");
                                break;
                            case 2:
                                setMode(1);
                                SpeakPartial("Quantum computing is a type of computing that uses quantum-mechanical phenomena, like superposition and entanglement, to perform operations on data.");
                                break;
                            case 3:
                                setMode(0);
                                SpeakPartial("Hello! A qubit, short for quantum bit, is the basic unit of quantum information in quantum computing. It is similar to a classical bit, but can exist in a state of 0, 1, or both simultaneously due to principles of quantum superposition and entanglement. This allows qubits to perform complex computations at a much faster rate than classical bits. Isn't that fascinating? If you have any more questions, feel free to ask!");
                                break;
                            case 4:
                                setMode(1);
                                SpeakPartial("A qubit is the fundamental unit of quantum information, analogous to a classical bit but with the ability to exist in multiple states simultaneously due to superposition.");
                                break;
                            case 5:
                                setMode(0);
                                SpeakPartial("Hello, and great question! A blockchain is a decentralized, distributed ledger that records transactions across a network of computers. Each block in the chain contains a number of transactions, and every time a new transaction occurs, it is added to every participant's ledger. This creates a secure and transparent record of transactions that cannot be altered or deleted. It's like a digital ledger that keeps track of all transactions in a tamper-proof way. Let me know if you have any more questions!");
                                break;
                            case 6:
                                setMode(1);
                                SpeakPartial("A blockchain is a decentralized and distributed digital ledger that records transactions across multiple computers in a secure and transparent manner.");
                                break;
                            case 7:
                                setMode(0);
                                SpeakPartial("Hello! String theory is a theoretical framework in physics that suggests that the most basic building blocks of the universe are not particles, as we traditionally think of them, but tiny, one-dimensional strings. These strings vibrate at different frequencies, giving rise to all the particles and forces we observe in the universe. It's a fascinating and complex topic that aims to unify the conflicting theories of quantum mechanics and general relativity. Isn't that amazing? Do you have any specific questions about string theory?");
                                break;
                            case 8:
                                setMode(1);
                                SpeakPartial("String theory is a theoretical framework in physics that proposes that the fundamental building blocks of the universe are tiny, one-dimensional strings rather than point-like particles.");
                                break;
                        }
                    }
                    else
                    {
                        switch (state)
                        {
                            case 1:
                                setMode(0);
                                SpeakPartial("Quantum computing is a type of computing that uses quantum-mechanical phenomena, like superposition and entanglement, to perform operations on data.");
                                break;
                            case 2:
                                setMode(1);
                                SpeakPartial("Hello! Quantum computing is an exciting and cutting-edge branch of information technology that utilizes principles of quantum mechanics to perform computations. Unlike classical computers that use bits to represent information as either 0 or 1, quantum computers use quantum bits, or qubits, which can represent 0, 1, or any superposition of these states, allowing for incredibly fast and complex calculations. It's like harnessing the power of quantum physics to solve problems that are currently beyond the capabilities of classical computers. Exciting stuff, right? Let me know if you have any other questions!");
                                break;
                            case 3:
                                setMode(0);
                                SpeakPartial("A qubit is the fundamental unit of quantum information, analogous to a classical bit but with the ability to exist in multiple states simultaneously due to superposition.");
                                break;
                            case 4:
                                setMode(1);
                                SpeakPartial("Hello! A qubit, short for quantum bit, is the basic unit of quantum information in quantum computing. It is similar to a classical bit, but can exist in a state of 0, 1, or both simultaneously due to principles of quantum superposition and entanglement. This allows qubits to perform complex computations at a much faster rate than classical bits. Isn't that fascinating? If you have any more questions, feel free to ask!");
                                break;
                            case 5:
                                setMode(0);
                                SpeakPartial("A blockchain is a decentralized and distributed digital ledger that records transactions across multiple computers in a secure and transparent manner.");
                                break;
                            case 6:
                                setMode(1);
                                SpeakPartial("Hello, and great question! A blockchain is a decentralized, distributed ledger that records transactions across a network of computers. Each block in the chain contains a number of transactions, and every time a new transaction occurs, it is added to every participant's ledger. This creates a secure and transparent record of transactions that cannot be altered or deleted. It's like a digital ledger that keeps track of all transactions in a tamper-proof way. Let me know if you have any more questions!");
                                break;
                            case 7:
                                setMode(0);
                                SpeakPartial("String theory is a theoretical framework in physics that proposes that the fundamental building blocks of the universe are tiny, one-dimensional strings rather than point-like particles.");
                                break;
                            case 8:
                                setMode(1);
                                SpeakPartial("Hello! String theory is a theoretical framework in physics that suggests that the most basic building blocks of the universe are not particles, as we traditionally think of them, but tiny, one-dimensional strings. These strings vibrate at different frequencies, giving rise to all the particles and forces we observe in the universe. It's a fascinating and complex topic that aims to unify the conflicting theories of quantum mechanics and general relativity. Isn't that amazing? Do you have any specific questions about string theory?");
                                break;
                        }
                    }
                }
                else
                {
                    setMode(2);
                    SpeakPartial("Hello. Please answer the following question. Based on the video you are watching, which digital person looks more extroverted, enthusiastic, sympathetic, and warm. Please enter your answer using the buttons below.");
                }

            }
        }


        waitTimer1 -= Time.deltaTime;
        waitTimer2 -= Time.deltaTime;

        if (agent != null && agentOn)
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

                if(agentCopy != null)
                {
                    float reversePersonality = -personality;

                    agentCopy.IKFAC_side = reversePersonality * 0.25f;
                    agentCopy.IKFAC_up = reversePersonality * 0.4f;
                    agentCopy.space = reversePersonality;
                    agentCopy.weight = reversePersonality;
                    agentCopy.time = reversePersonality * 0.6f;
                    agentCopy.flow = reversePersonality * 0.3f;

                    agentCopy.spine_bend = -reversePersonality * .25f;
                    agentCopy.head_bend = -reversePersonality;
                }
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

                if (agentCopy != null) {
                    float reversePersonality = -personality;

                    agentCopy.IKFAC_side = reversePersonality * 0.3f;
                    agentCopy.IKFAC_up = reversePersonality * 0.3f;
                    agentCopy.space = reversePersonality;
                    agentCopy.weight = reversePersonality;
                    agentCopy.time = reversePersonality * 0.2f;
                    agentCopy.flow = reversePersonality * 0.3f;

                    agentCopy.spine_bend = -reversePersonality * .1f;
                    agentCopy.head_bend = reversePersonality;
                }
            }
            
            agent.C_LabanRotation = modifications;
            agent.C_LabanIK = modifications;
            agent.C_Fluctuation = modifications;
            agent.C_SpeedAdjust = modifications;

            if (agentCopy != null) {
                agentCopy.C_LabanRotation = modifications;
                agentCopy.C_LabanIK = modifications;
                agentCopy.C_Fluctuation = modifications;
                agentCopy.C_SpeedAdjust = modifications;
            }

            if (waitTimer1 < 0 && agent.GetAnimator() != null && agent.GetAnimator().GetCurrentAnimatorStateInfo(1).normalizedTime >= 1.0)
            {
                waitTimer1 = 1f;
                if (agentTalking)
                {
                    agent.SetAnimation(deterministicAnimationNo+1);
                    deterministicAnimationNo = ((deterministicAnimationNo + 1) % 8);
                }
                else
                {
                    agent.SetAnimation(0);
                }
            }

            if (waitTimer2 < 0 && agentCopy.GetAnimator() != null && agentCopy.GetAnimator().GetCurrentAnimatorStateInfo(1).normalizedTime >= 1.0)
            {
                waitTimer2 = 1f;
                if (agentTalking)
                {
                    agentCopy.SetAnimation(deterministicAnimationNoCopy+1);
                    deterministicAnimationNoCopy = ((deterministicAnimationNoCopy + 1) % 8);
                }
                else
                {
                    agentCopy.SetAnimation(0);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            reversed = false;
            setAgentMode(0);
            working = false;
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            reversed = false;
            setAgentMode(1);
            working = false;
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            reversed = false;
            setAgentMode(2);
            working = false;
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            reversed = true;
            setAgentMode(3);
            working = false;
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            reversed = true;
            setAgentMode(0);
            working = false;
        }
        else if (Input.GetKeyDown(KeyCode.F6))
        {
            reversed = true;
            setAgentMode(1);
            working = false;
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            shortcrc = true;
            reversed = false;
            setAgentMode(4);
            working = false;
        }
        else if (Input.GetKeyDown(KeyCode.F8))
        {
            shortcrc = true;
            reversed = false;
            setAgentMode(5);
            working = false;
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            ChangeAgent(1);
        }
        else if (Input.GetKeyDown(KeyCode.F10))
        {
            ChangeAgent(2);
        }
        else if (Input.GetKeyDown(KeyCode.F11))
        {

            ChangeAgent(3);

        }
        else if (Input.GetKeyDown(KeyCode.F12))
        {
            ChangeAgent(4);
        }
    }

    bool openHolds = false;
    bool shortcrc = false;

    void setAgentMode(int aMod)
    {
        switch (aMod)
        {
            case 0: // agent with no mod
                openHolds = true;
                SetModifications(false);
                top_textLeft.enabled = false;
                top_textRight.enabled = false;
                break;
            case 1: // no agent no mod
                openHolds = false;
                SetModifications(false);
                top_textLeft.enabled = true;
                top_textRight.enabled = true;
                leftHold.SetActive(true);
                rightHold.SetActive(true);
                break;
            case 2: // mod agent left high
                openHolds = true;
                SetModifications(true);
                SetPersonality(1);
                top_textLeft.enabled = false;
                top_textRight.enabled = false;
                break;
            case 3: // mod agent right high
                openHolds = true;
                SetModifications(true);
                SetPersonality(-1);
                top_textLeft.enabled = false;
                top_textRight.enabled = false;
                break;
            case 4: // mod agent left high
                openHolds = false;
                SetModifications(true);
                SetPersonality(1);
                top_textLeft.enabled = false;
                top_textRight.enabled = false;
                leftHold.SetActive(false);
                rightHold.SetActive(false);
                break;
            case 5: // mod agent right high
                openHolds = false;
                SetModifications(true);
                SetPersonality(-1);
                top_textLeft.enabled = false;
                top_textRight.enabled = false;
                leftHold.SetActive(false);
                rightHold.SetActive(false);
                break;
        }
    }

    public void speakMessage()
    {
        SpeakPartial("Hello. Please answer the following question, based on the video you are watching. which digital person looks more extroverted, enthusiastic, sympathetic, and warm; please enter your answer using the buttons below.");
    }

    private void SetPersonality(float personality)
    {
        this.personality = personality;
        if (personality == 0)
        {
            if (agent != null)
            {
                agent.SetAnimationParameter("FaceNo", 0);
                agentCopy.SetAnimationParameter("FaceNo", 0);
            }
        }
        else if (personality > 0)
        {
            if(agent != null)
            {
                agent.SetAnimationParameter("FaceNo", 1);
                agentCopy.SetAnimationParameter("FaceNo", 2);
            }
        }
        else
        {
            if (agent != null)
            {
                agent.SetAnimationParameter("FaceNo", 2);
                agentCopy.SetAnimationParameter("FaceNo", 1);
            }
        }
    }

    private bool modifications;
    private void SetModifications(bool modifications)
    {
        this.modifications = modifications;
        if (!modifications)
        {
            agent.SetAnimationParameter("FaceNo", 0);
            agentCopy.SetAnimationParameter("FaceNo", 0);
        }
    }

    private bool agentOn = true;
    private void SetAgentOn(bool agentOn)
    {
        this.agentOn = agentOn;
        if(agentOn)
        {
            if (agent != null) agent.gameObject.SetActive(true);
        }
        else
        {
            if (agent != null) agent.gameObject.SetActive(false);
        }
    }

    private int currentAgent = 0;

    public void ChangeAgent(int agentNo) {
        currentAgent = agentNo;

        agent2.SetActive(false);
        agent4.SetActive(false);
        agent2copy.SetActive(false);
        agent4copy.SetActive(false);

        if (currentAgent == 1)
        {
            agent2.SetActive(true);
            lipSyncPasser.currentFace = agent2.GetComponent<FaceScriptCC>();
            agent = agent2.GetComponent<AgentController>();
            body2.gameObject.GetComponent<Renderer>().materials[0].SetTexture("_DiffuseMap", textures1[0]);
            body2.gameObject.GetComponent<Renderer>().materials[1].SetTexture("_DiffuseMap", textures1[1]);
            body2.gameObject.GetComponent<Renderer>().materials[2].SetTexture("_DiffuseMap", textures1[2]);
            body2.gameObject.GetComponent<Renderer>().materials[3].SetTexture("_DiffuseMap", textures1[3]);

            agent2copy.SetActive(true);
            lipSyncPasser.currentFace2 = agent2copy.GetComponent<FaceScriptCC>();
            agentCopy = agent2copy.GetComponent<AgentController>();
            body2_copy.gameObject.GetComponent<Renderer>().materials[0].SetTexture("_DiffuseMap", textures1[0]);
            body2_copy.gameObject.GetComponent<Renderer>().materials[1].SetTexture("_DiffuseMap", textures1[1]);
            body2_copy.gameObject.GetComponent<Renderer>().materials[2].SetTexture("_DiffuseMap", textures1[2]);
            body2_copy.gameObject.GetComponent<Renderer>().materials[3].SetTexture("_DiffuseMap", textures1[3]);
            
            selectedVoice = supportedVoices[2];
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

            agent2copy.SetActive(true);
            lipSyncPasser.currentFace2 = agent2copy.GetComponent<FaceScriptCC>();
            agentCopy = agent2copy.GetComponent<AgentController>();
            body2_copy.gameObject.GetComponent<Renderer>().materials[0].SetTexture("_DiffuseMap", textures2[0]);
            body2_copy.gameObject.GetComponent<Renderer>().materials[1].SetTexture("_DiffuseMap", textures2[1]);
            body2_copy.gameObject.GetComponent<Renderer>().materials[2].SetTexture("_DiffuseMap", textures2[2]);
            body2_copy.gameObject.GetComponent<Renderer>().materials[3].SetTexture("_DiffuseMap", textures2[3]);

            selectedVoice = supportedVoices[2];
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

            body4_copy.gameObject.GetComponent<Renderer>().materials[0].SetTexture("_DiffuseMap", textures3[0]);
            body4_copy.gameObject.GetComponent<Renderer>().materials[1].SetTexture("_DiffuseMap", textures3[1]);
            body4_copy.gameObject.GetComponent<Renderer>().materials[2].SetTexture("_DiffuseMap", textures3[2]);
            body4_copy.gameObject.GetComponent<Renderer>().materials[3].SetTexture("_DiffuseMap", textures3[3]);

            agent4copy.SetActive(true);
            lipSyncPasser.currentFace2 = agent4copy.GetComponent<FaceScriptCC>();
            agentCopy = agent4copy.GetComponent<AgentController>();

            selectedVoice = supportedVoices[0];
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

            body4_copy.gameObject.GetComponent<Renderer>().materials[0].SetTexture("_DiffuseMap", textures4[0]);
            body4_copy.gameObject.GetComponent<Renderer>().materials[1].SetTexture("_DiffuseMap", textures4[1]);
            body4_copy.gameObject.GetComponent<Renderer>().materials[2].SetTexture("_DiffuseMap", textures4[2]);
            body4_copy.gameObject.GetComponent<Renderer>().materials[3].SetTexture("_DiffuseMap", textures4[3]);

            agent4copy.SetActive(true);
            lipSyncPasser.currentFace2 = agent4copy.GetComponent<FaceScriptCC>();
            agentCopy = agent4copy.GetComponent<AgentController>();

            selectedVoice = supportedVoices[0];
        }

      
        animatorInspector.anim = agent.GetComponent<Animator>();
        // agent.lookObject = Camera.current.gameObject;

        SetPersonality(personality);
        // agent.C_LookIK = true;
    }
}
