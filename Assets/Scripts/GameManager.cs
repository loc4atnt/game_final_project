
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static int STATE_DELAY = 80000;
    public static int DELAY_TIME = 5000;
    public static int MAX_PLAY_TIME = 60000;
    //
    public static int GAME_STAGE_NOT_START = 0;
    public static int GAME_STAGE_PLAYING = 5;
    public static int GAME_STAGE_PRE_TO_END = 10;
    public static int GAME_STAGE_END = 15;
    public static int GAME_STAGE_PICK_SONG = 20;

    public static int GAME_STATE_PREPARE_TO_NEXT_MOVEMENT = 0;
    public static int GAME_STATE_PLAYING = 1;
    public static int GAME_STATE_FREE_DANCE = 2;

    public GameObject kinectModel;
    public GameObject danceModel;
    public AudioManager audioManager;
    public Text comboCount;
    public UISprite comboEffect;
    public Camera mainCam;
    public Camera subCam;
    public GameObject menuPanel;
    public GameObject comboPanel;
    public GameObject[] hitboxes;
    public Text timer;
    public GameObject missed;
    public GameObject success;
    public long startTime;
    public ScoreBoard scoreBoard;
    public GameObject readyPopup;

    private int currentState = 0b00000000;

    private int gameStage = GAME_STAGE_NOT_START;// 0 = not start, 5 = playing, 10 = prepare to end, 15 = end
    private int gameState = GAME_STATE_PLAYING;
    private int score;
    //
    private System.Random rd = new System.Random();
    private int[] nextStates = { -1,-1,-1,-1 };
    private long lastChangeStateTs;

    private SampleStateIKController currentStateCtrller = null;

    private long lastChangeStateAnimationTS;

    int totalCombo = 0;
    int continuousCombo = 0;
    int missedCnt = 0;
    int maxCombo = 0;
    bool failCombo = false;
    long specialStageTime = 0;
    long lastSuccessTime = -1;
    bool doneReady;
    Vector3 mainCamPos;
    Vector3 mainCamRot;

    public int getCurrentRequestState()
    {
        return nextStates[0];
    }

    public void pickSong(int id)
    {
        gameStage = GAME_STAGE_PLAYING;
        startTime = now();
        audioManager.play(id);
        comboPanel.SetActive(true);
        menuPanel.SetActive(false);
        timer.gameObject.SetActive(true);
        long tempTime = now();

        readyPopup.SetActive(true);
        doneReady = false;
    }

    private long now()
    {
        DateTime currentTime = DateTime.UtcNow;
        long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();
        return unixTime;
    }

    private int pushNextState(bool genNew=true)
    {
        int nstate = nextStates[0];
        for (int i = 0; i < nextStates.Length-1; i++)
        {
            nextStates[i] = nextStates[i + 1];
        }
        nextStates[nextStates.Length - 1] = genNew ? genRandState() : -1;
        return nstate;
    }

    private int genRandState()
    {
        // left leg 4, right leg 3
        int leg = 0b00000000;
        int legState = rd.Next(3);
        leg = setBitForVar(leg, 4, legState == 1);
        leg = setBitForVar(leg, 3, legState == 2);

        // left hand 0/2, right hand 0/1
        int hand = 0b00000000;
        bool twoHand = rd.Next(2) == 1;
        if (twoHand)
        {
            int twoHandState = rd.Next(2);
            hand = setBitForVar(hand, 0, twoHandState == 1);
        } else
        {
            int leftHandState = rd.Next(2);
            int rightHandState = rd.Next(2);
            hand = setBitForVar(hand, 2, leftHandState == 1);
            hand = setBitForVar(hand, 1, rightHandState == 1);
        }

        int finalState = (hand | leg);
        return finalState != 0 ? finalState : genRandState();
    }

    private int setBitForVar(int var, int index, bool tg)
    {
        if (index < 0 || index > 4) return var;
        if (tg) var |= 1 << index;
        else var &= ~(1 << index);
        return var;
    }

    private void sendStateToSampleController(int s, SampleStateIKController ctrller)
    {
        if (!ctrller) return;

        bool rightLegStretching = (s & 0b00001000) > 0;
        bool leftLegStretching = (s & 0b00010000) > 0;
        bool rightHandRaising = (s & 0b00000001) > 0;
        bool leftHandRaising = (s & 0b00000001) > 0;
        bool rightHandStretching = (s & 0b00000100) > 0;
        bool leftHandStretching = (s & 0b00000010) > 0;

        ctrller.moveLeftHand(leftHandRaising ? SampleStateIKController.STATE_RAISE_HAND : (leftHandStretching ? SampleStateIKController.STATE_STRETCH_HAND : SampleStateIKController.STATE_REST_HAND));
        ctrller.moveRightHand(rightHandRaising ? SampleStateIKController.STATE_RAISE_HAND : (rightHandStretching ? SampleStateIKController.STATE_STRETCH_HAND : SampleStateIKController.STATE_REST_HAND));
        ctrller.moveLeftLeg(leftLegStretching ? SampleStateIKController.STATE_STRETCH_LEG : SampleStateIKController.STATE_REST_LEG);
        ctrller.moveRightLeg(rightLegStretching ? SampleStateIKController.STATE_STRETCH_LEG : SampleStateIKController.STATE_REST_LEG);
    }

    public void toggleState(int index, bool tg)
    {
        this.currentState = setBitForVar(this.currentState, index, tg);
    }

    public void startGame()
    {
        for (int i = 0; i < nextStates.Length; i++) nextStates[i] = genRandState();
        lastChangeStateTs = now();
        gameStage = GAME_STAGE_PICK_SONG;
        score = 0;
        missedCnt = 0;
        maxCombo = 0;
    }

    // Please call this method before the song ending 4*STATE_DELAY seconds
    public void prepareToEndGame()
    {
        gameStage = GAME_STAGE_PRE_TO_END;
    }

    // Start is called before the first frame update
    void Start()
    {
        lastSuccessTime = now();
        mainCamRot = new Vector3(mainCam.transform.rotation.x, mainCam.transform.rotation.y, mainCam.transform.rotation.z);
        mainCamPos = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, mainCam.transform.position.z);
        GameObject currentSampleStateObj = GameObject.Find("CurrentSampleState");
        this.currentStateCtrller = currentSampleStateObj.GetComponent<SampleStateIKController>();
        startGame();
    }

    // Be called when a movement is failed
    void CallFailedState()
    {

    }

    // Be called when a movement is got
    void CallGotState()
    {
        currentStateCtrller.makeCorrectAnimation();
        gameState = GAME_STATE_PREPARE_TO_NEXT_MOVEMENT;
        lastChangeStateAnimationTS = now();
    }

    // Be called when a game end
    void CallEndGame()
    {

    }

    void updateCombo(int value)
    {
        comboCount.text = value.ToString();
        comboEffect.playAnimationOnce();
    }
    // Update is called once per frame
    void Update()
    {
        if (gameStage == GAME_STAGE_PICK_SONG)
        {
            return;
        }

        if (gameStage == GAME_STAGE_PLAYING || gameStage == GAME_STAGE_PRE_TO_END)
        {
            if (now() - startTime < 2000)
            {
                return;
            } else if (!doneReady)
            {
                readyPopup.SetActive(false);
                lastSuccessTime = now();
                doneReady = true;
            }
            if (continuousCombo == 5)
            {
                success.SetActive(true);
                //kinectModel.SetActive(false);
                danceModel.SetActive(true);
                score += 3;
                //for (int i = 0; i < hitboxes.Length; ++i)
                    //hitboxes[i].SetActive(false);
                danceModel.gameObject.GetComponent<Animator>().SetInteger("danceMode", rd.Next(1,5));


                if (now() - specialStageTime < 5000)
                    return;
                continuousCombo = 0;
                danceModel.SetActive(false);
                //kinectModel.SetActive(true);

                //mainCam.transform.position = new Vector3(mainCamPos.x, mainCamPos.y, mainCamPos.z);
                mainCam.transform.Rotate(new Vector3(0f, -180f, 0f));
                //for (int i = 0; i < hitboxes.Length; ++i)
                    //hitboxes[i].SetActive(true);
                lastSuccessTime = now();
                
            }

            long tempTime = now();
            if (tempTime - lastSuccessTime > DELAY_TIME)
            {
                print("MISSSS");
                continuousCombo = 0;
                totalCombo = 0;
                updateCombo(totalCombo);
                CallGotState();
                pushNextState(gameStage == GAME_STAGE_PLAYING);
                lastSuccessTime = now();
                sendStateToSampleController(getCurrentRequestState(), this.currentStateCtrller);
                missed.SetActive(true);
                ++missedCnt;
                return;
            } else
            {
                timer.text = ((DELAY_TIME * 1f - (tempTime - lastSuccessTime)) / 1000f).ToString() + "s";
            }

            if (gameState == GAME_STATE_PREPARE_TO_NEXT_MOVEMENT)
            {
                if (now() - lastChangeStateAnimationTS > 500)
                {
                    currentStateCtrller.makeNormalAnimation();
                    gameState = GAME_STATE_PLAYING;
                }
                return;
            }
            else if (gameState == GAME_STATE_PLAYING)
            {
                long ts = now();
                int curReqState = getCurrentRequestState();

                if (curReqState < 0) // end game
                {
                    gameStage = GAME_STAGE_END;
                    CallEndGame();
                    return;
                }

                if (ts - lastChangeStateTs > STATE_DELAY)
                {
                    CallFailedState();
                    pushNextState(gameStage == GAME_STAGE_PLAYING);
                    lastChangeStateTs = ts;
                }
                else
                {
                    if (currentState != 0)
                    {
                        if (curReqState == currentState)
                        {
                            lastSuccessTime = now();
                            CallGotState();
                            this.score += 1;
                            pushNextState(gameStage == GAME_STAGE_PLAYING);
                            lastChangeStateTs = ts;
                            ++continuousCombo;
                            ++totalCombo;
                            if (totalCombo > maxCombo)
                                maxCombo = totalCombo;
                            specialStageTime = now();
                            updateCombo(totalCombo);
                            
                            if (continuousCombo == 5)
                            {
                                mainCam.transform.Rotate(new Vector3(0f, 180f, 0f));
                            }
                        }
                    }
                }
                sendStateToSampleController(curReqState, this.currentStateCtrller);
            }

            if (!audioManager.isPlaying() || now() - startTime > 60000)
            {
                gameStage = GAME_STAGE_PICK_SONG;
                totalCombo = 0;
                continuousCombo = 0;
                scoreBoard.setData(score, maxCombo, missedCnt);
                scoreBoard.gameObject.SetActive(true);
                audioManager.stopAudio();
            }
        }
        else print("stopped");
    }
}
