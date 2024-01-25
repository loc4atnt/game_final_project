
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int STATE_DELAY = 80000;
    //
    public static int GAME_STAGE_NOT_START = 0;
    public static int GAME_STAGE_PLAYING = 5;
    public static int GAME_STAGE_PRE_TO_END = 10;
    public static int GAME_STAGE_END = 15;

    public static int GAME_STATE_PREPARE_TO_NEXT_MOVEMENT = 0;
    public static int GAME_STATE_PLAYING = 1;

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

    public int getCurrentRequestState()
    {
        return nextStates[0];
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
        gameStage = GAME_STAGE_PLAYING;
        score = 0;
    }

    // Please call this method before the song ending 4*STATE_DELAY seconds
    public void prepareToEndGame()
    {
        gameStage = GAME_STAGE_PRE_TO_END;
    }

    // Start is called before the first frame update
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        if (gameStage == GAME_STAGE_PLAYING || gameStage == GAME_STAGE_PRE_TO_END)
        {
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
                // print(state);
                // print("" + nextStates[0] + " - " + nextStates[1] + " - " + nextStates[2] + " - " + nextStates[3]);
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
                            CallGotState();
                            this.score += 1;

                            pushNextState(gameStage == GAME_STAGE_PLAYING);
                            lastChangeStateTs = ts;
                        }
                        else
                        {
                            // CallFailedState();
                        }
                    }
                }

                // show current next states
                print(curReqState + " - " + currentState + " [" + score + "]");
                sendStateToSampleController(curReqState, this.currentStateCtrller);
            }
        }
    }
}
