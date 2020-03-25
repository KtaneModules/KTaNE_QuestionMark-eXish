using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Questionmark : MonoBehaviour
{
    public KMBombInfo info;
    public KMSelectable module;
    public SpriteRenderer moduleSprite;
    public Sprite qmarkSprite;
    public Sprite[] itemSprites = new Sprite[15];
    private int[] spritePool = new int[4];
    private int[] releaseTimes;
    private int[] spriteValues = new[] {2, 1, 7, 3, 4, 9, 6, 8, 1, 3, 8, 4, 5, 6, 2};
    private int deathSprite = 4;
    private bool isHeld = false;
    private bool isSolved = false;
    private static int _moduleIdCounter = 1;
    private int _moduleId = 0;

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        module.OnInteract += delegate () { OnPress(); return false; };
        module.OnInteractEnded += OnRelease;
        module.OnCancel += delegate () { isHeld = false; return true; };
        //GetComponent<KMBombModule>().OnPass += delegate () { isSolved = true; return true; };

        GetComponent<KMBombModule>().OnActivate += ActivateModule;
    }

    void ActivateModule()
    {
        Init();
    }
    
    void Init()
    {
        moduleSprite.sprite = qmarkSprite;
        /**module.OnInteract += delegate () { OnPress(); return false; };
        module.OnInteractEnded += OnRelease;
        module.OnCancel += delegate () { isHeld = false; return true; };
        GetComponent<KMBombModule>().OnPass += delegate () { isSolved = true; return true; };*/
    }
    
    void OnPress()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMAudio>().PlaySoundAtTransform("powerupappears", transform);
        if (!isSolved)
        {
            module.AddInteractionPunch();
            isHeld = true;
            spritePool[0] = UnityEngine.Random.Range(0, 15);
            spritePool[1] = UnityEngine.Random.Range(0, 15);
            do
            {
                spritePool[1] = UnityEngine.Random.Range(0, 15);
            } while (spritePool[1] == spritePool[0]);
            spritePool[2] = UnityEngine.Random.Range(0, 15);
            do
            {
                spritePool[2] = UnityEngine.Random.Range(0, 15);
            } while (spritePool[2] == spritePool[0] || spritePool[2] == spritePool[1]);
            spritePool[3] = UnityEngine.Random.Range(0, 15);
            do
            {
                spritePool[3] = UnityEngine.Random.Range(0, 15);
            } while (spritePool[3] == spritePool[0] || spritePool[3] == spritePool[1] || spritePool[3] == spritePool[2]);
            Debug.LogFormat("[Question Mark #{0}] Module activated.", _moduleId);
            Debug.LogFormat("[Question Mark #{0}] Sprites are referenced in the format of #-#2 where # is the row and #2 is the column in the table in the manual", _moduleId);
            Debug.LogFormat("[Question Mark #{0}] Sprite pool is: {1}-{2}, {3}-{4}, {5}-{6}, {7}-{8}.", _moduleId, spritePool[0] / 5 + 1, spritePool[0] % 5 + 1, spritePool[1] / 5 + 1, spritePool[1] % 5 + 1, spritePool[2] / 5 + 1, spritePool[2] % 5 + 1, spritePool[3] / 5 + 1, spritePool[3] % 5 + 1);
            Debug.LogFormat("[Question Mark #{0}] Sprite values are: {1}, {2}, {3}, {4}.", _moduleId, spriteValues[spritePool[0]], spriteValues[spritePool[1]], spriteValues[spritePool[2]], spriteValues[spritePool[3]]);
            int sum = spriteValues[spritePool[0]] + spriteValues[spritePool[1]] + spriteValues[spritePool[2]] + spriteValues[spritePool[3]];
            Debug.LogFormat("[Question Mark #{0}] New sprite is: {1}-{2}.", _moduleId, (sum + 14) % 15 / 5 + 1, (sum + 4) % 5 + 1);
            releaseTimes = ReleaseTimes();
            StartCoroutine(OnHold());
        }
    }

    IEnumerator OnHold()
    {
        if (!isSolved)
        {
            int count = 0;
            while (isHeld) {
                moduleSprite.sprite = itemSprites[spritePool[count++]];
                count %= 4;
                yield return new WaitForSeconds(0.35f);
            }   
        }
    }
    
    void OnRelease()
    {
        if (!isSolved)
        {
            isHeld = false;
            int time = (int)Math.Floor(info.GetTime());
            if (releaseTimes.Contains(time % 10))
            {
                Debug.LogFormat("[Question Mark #{0}] Released at {1}. That is correct.", _moduleId, info.GetFormattedTime());
                //GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                GetComponent<KMAudio>().PlaySoundAtTransform("powerup", transform);
                GetComponent<KMBombModule>().HandlePass();
                isSolved = true;
            }
            else
            {
                Debug.LogFormat("[Question Mark #{0}] Released at {1}. That is incorrect.", _moduleId, info.GetFormattedTime());
                moduleSprite.sprite = qmarkSprite;
                GetComponent<KMBombModule>().HandleStrike();
            }
        }
    }
    
    int[] ReleaseTimes()
    {
        int time = 0;
        for (int i = 0; i < 4; i++)
        {
            time += spriteValues[spritePool[i]];
        }
        time = (time + 14) % 15;
        if (spritePool.Contains(time) && time != deathSprite)
        {
            Debug.LogFormat("[Question Mark #{0}] Closest sprite's value is: {1}.", _moduleId, spriteValues[time]);
            return new[] {spriteValues[time]};
        }
        else
        {
            double dist = Distance(0, 14);
            for (int i = 0; i < 15; i++)
            {
                if (Distance(time, i) < dist && spritePool.Contains(i) && i != deathSprite)
                {
                    dist = Distance(time, i);
                }
            }
            List<int> times = new List<int>();
            string logText = "";
            for (int i = 0; i < 15; i++)
            {
                if (Distance(time, i) == dist && spritePool.Contains(i) && i != deathSprite)
                {
                    times.Add(spriteValues[i]);
                    if (logText.Length != 0)
                    {
                        logText += ", ";
                    }
                    logText += spriteValues[i].ToString();
                }
            }
            if (logText.Length == 1)
            {
                Debug.LogFormat("[Question Mark #{0}] Closest sprite's value is: " + logText + ".", _moduleId);
            }
            else
            {
                Debug.LogFormat("[Question Mark #{0}] Closest sprites' values are: " + logText + ".", _moduleId);
            }
            return times.ToArray();
        }
    }
    
    double Distance(int from, int to)
    {
        return Math.Sqrt((double)((from / 5 - to / 5) * (from / 5 - to / 5) + (from % 5 - to % 5) * (from % 5 - to % 5)));
    }
    
    
    #pragma warning disable 0414
    private bool TwitchZenMode = false;
    private string TwitchManualCode = "Question Mark";
    private string TwitchHelpMessage = "Hold the module with \"hold\". Release the module with \"release <digit> <digit> ...\". Manual";
    #pragma warning restore 0414

    public IEnumerator TwitchHandleForcedSolve()
    {
        if (!isHeld)
        {
            module.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        while (!releaseTimes.Contains((int)info.GetTime() % 10))
        {
            yield return true;
            yield return new WaitForSeconds(0.1f);
        }
        module.OnInteractEnded();
        /**isHeld = false;
        int sprite = UnityEngine.Random.Range(0, 14);
        sprite += (sprite < deathSprite) ? 0 : 1;
        moduleSprite.sprite = itemSprites[sprite];
        Debug.LogFormat("[Question Mark #{0}] Module forcibly solved.", _moduleId);
        GetComponent<KMBombModule>().HandlePass();*/
    }

    public IEnumerator ProcessTwitchCommand(string cmd)
    {
        yield return null;
        cmd = cmd.ToLowerInvariant();
        if (cmd.StartsWith("hold"))
        {
            if (isHeld)
            {
                yield return "sendtochaterror Module is already held.";
                yield break;
            }

            yield return "Question Mark";
            yield return module;
            yield break;
        }
        else if(cmd.StartsWith("release"))
        {
            if(!cmd.StartsWith("release "))
            {
                yield return "sendtochaterror No release times specified.";
                yield break;
            }
            if(!isHeld)
            {
                yield return "sendtochaterror Module is not currently held.";
                yield break;
            }
            cmd = cmd.Substring(8);

            string[] timeList = cmd.Split(' ');
            List<int> times = new List<int>();
            for(int i = 0; i < timeList.Length; i++)
            {
                times.Add((int)timeList[i][timeList[i].Length - 1] - '0');
                if (timeList[i].Length != 1)
                {
                    yield return "sendtochaterror Release times can only be specified as the last second digit.";
                    yield break;
                }
            }

            yield return "Question Mark";
            
            int currentTime = (int)info.GetTime();
            int targetTime = -1;
            
            if (TwitchZenMode)
            {
                foreach(int time in times)
                {
                    int t = time;
                    while(t < currentTime) t += 10;
                    if(t < targetTime || targetTime == -1) targetTime = t;
                }
            }
            else
            {
                foreach(int time in times) 
                {
                    int t = time;
                    while(t <= currentTime)
                    {
                        t += 10;
                    }
                    t -= 10;
                    if(t > targetTime) targetTime = t;
                }
            }
            
            if(targetTime == -1)
            {
                yield return "sendtochaterror No valid release times specified.";
                yield break;
            }
            else if (releaseTimes.Contains(targetTime))
            {
                yield return "solve";
            }
            else
            {
                yield return "strike";
            }
            
            yield return "sendtochat Target release time: " + (targetTime / 60).ToString("D2") + ":" + (targetTime % 60).ToString("D2");

            while(true)
            {
                currentTime = (int)info.GetTime();
                if(currentTime != targetTime)
                {
                    yield return "trycancel";
                }
                else
                {
                    yield return module;
                    break;
                }
            }
            yield break;
        }
        else yield return "sendtochaterror Commands must start with \"hold\" or \"release\".";
        yield break;
    }
}

