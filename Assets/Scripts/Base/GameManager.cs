using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = GameObject.Find("GameManager");
                instance = go.GetComponent<GameManager>() ?? go.AddComponent<GameManager>();
                instance.Awake();
            }
            return instance;
        }
    }

    public GameObject CurrentState
    {
        get
        {
            return States[currentState];
        }
    }

    public event Action<string> OnStateChanged;

    public List<GameObject> States;
    
    int currentState = -1;
    bool changingState;
    Stack<int> historial = new Stack<int>();

    
    private void Awake()
    {
        Application.targetFrameRate = -1;

        //if (!Application.isEditor)
        //   Cursor.visible = false;

        instance = this;
    }

    void Start()
    {
        

        for (int i = 0; i < States.Count; i++)
               States[i].SetActive(false);
        /*
        if (Config.Instance.GetBoolean("hide_mouse", true) && !Application.isEditor)
            Cursor.visible = false;
        */

        GotoFirstState();
    }

    private void Update()
    {
        CheckExitCombination();
        CheckConfigCombination();
    }

    int _configCombination = 0;
    private void CheckConfigCombination()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Rect r;
            Vector2 m = Input.mousePosition;
            m.y = Screen.height - m.y;


            r = new Rect(0, 0, 150, 150);

            if (m.x > r.x && m.y > r.y && m.x < r.x + r.width && m.y < r.y + r.height)
                _configCombination++;
            else
                _configCombination = 0;

            if (_configCombination >= 5)
            {
                _configCombination = 0;
                GameManager.instance.GotoState("Config");
            }
        }
    }

    int _exitCombination = 0;
    private void CheckExitCombination()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Rect r;
            Vector2 m = Input.mousePosition;
            m.y = Screen.height - m.y;

            #region Exit

            r = new Rect(0, 0, 150, 150);

            switch (_exitCombination)
            {
                case 0: break;
                case 1: r.x = Screen.width - r.width; break;
                case 2: r.y = Screen.height - r.height; break;
                case 3: r.x = Screen.width - r.width; r.y = Screen.height - r.height; break;
            }


            if (m.x > r.x && m.y > r.y && m.x < r.x + r.width && m.y < r.y + r.height)
                _exitCombination++;
            else
                _exitCombination = 0;

            if (_exitCombination >= 4)
                Application.Quit();

            #endregion
        }
    }

    public void GotoState( string stateName )
    {
        for (int i = 0; i < States.Count; i++)
        {
            if (States[i].name == stateName)
            {
                StartCoroutine(ChangeState(i));
                return;
            }
        }
    }

    public void GotoFirstState()
    {
        GotoState(States[0].name);
    }

    public void GotoPrevState()
    {
        if (historial.Count > 0)
        {
            int p = historial.Pop();

            if (historial.Count > 0 && p == currentState)
                p = historial.Pop();

            StartCoroutine(ChangeState(p));
        }
    }
    public void GotoNextState()
    {
        StartCoroutine(ChangeState(currentState + 1));
    }

    IEnumerator ChangeState( int stateIndex )
    {
        if (changingState)
            yield return null;

        changingState = true;

        if (stateIndex < 0)  stateIndex = 0;
        if (stateIndex > States.Count - 1) stateIndex = States.Count - 1;

		if (stateIndex != currentState) {
			int lastState = currentState;
			currentState = stateIndex;

            if (currentState != -1)
                historial.Push(currentState);

            States[currentState].gameObject.SetActive (true);


            if (OnStateChanged != null)
                OnStateChanged(States[currentState].name);


            AudioManager.instance.PlaySound("wosh", 0.85f, false, AudioManager.PlayMode.PlayOver);

			if (lastState != -1) { 
                
				float changeStateTime = 0.5f;
				float t;
				Vector2 temp;
				float p;

				t = 0;
				temp = new Vector2 (States [currentState].transform.position.x, States [currentState].transform.position.y);
				while (t < changeStateTime) {
					t += Time.deltaTime;
					if (t > changeStateTime)
						t = changeStateTime;

					p = t / changeStateTime;

					//last
					if (currentState > lastState) {
						temp.x = -Screen.width * p + Screen.width * .5f;
						temp.y = States [lastState].transform.position.y;
						States [lastState].transform.position = temp;

						temp.x = Screen.width * (1 - p) + Screen.width * .5f;
						temp.y = States [currentState].transform.position.y;
						States [currentState].transform.position = temp;
					} else if (currentState < lastState) {
						temp.x = Screen.width * p + Screen.width * .5f;
						temp.y = States [lastState].transform.position.y;
						States [lastState].transform.position = temp;

						temp.x = -Screen.width * (1 - p) + Screen.width * .5f;
						temp.y = States [currentState].transform.position.y;
						States [currentState].transform.position = temp;
					}
	 
					yield return new WaitForEndOfFrame ();
				}
                
				States [lastState].gameObject.SetActive (false);
			}
		}

       // if (OnStateChanged != null)
        //\    OnStateChanged(States[currentState].name);

        changingState = false;
    }
}
