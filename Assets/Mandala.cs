using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using rnd = UnityEngine.Random;
using KModkit;
public class Mandala : MonoBehaviour {
    public KMSelectable mandala;
    public TextMesh firstMandalaText;
    public TextMesh secondMandalaText;
    public Color[] colors;
    int firstMandala;
    int[] secondMandala = new int[2];
    int[,] values = new int[3, 2];
    string[] table = new string[] {
        ".0123456",
        "789ABCDE",
        "FGHIJKLM",
        "NOPQRSTU",
        "VWXYZabc",
        "defghijk",
        "lmnopqrs",
        "tuvwxyz" };
    string binary;
    double heldTime;
    double releasedTime;
    bool released;
    int stage;
    public KMBombInfo bomb;
    public KMBombModule module;
    public KMAudio sound;
    int moduleId;
    static int moduleIdCounter = 1;
    bool solved;
	void Awake () {
        moduleId = moduleIdCounter++;
        mandala.OnInteract += delegate ()
        {
            if (!solved)
                released = false;
                StartCoroutine(Hold());
            mandala.AddInteractionPunch();
            sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
            return false;
        };

        mandala.OnInteractEnded += delegate ()
        {
            released = true;
            StopAllCoroutines();
            sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
            if (solved)
                return;
            Release();
        };
        module.OnActivate += delegate () { GenerateModule(); };
    }
	
	// Update is called once per frame
	void GenerateModule () {
        firstMandala = rnd.Range(0, 7);
        firstMandalaText.text = table[firstMandala][firstMandala].ToString();
        secondMandala:
        for (int i = 0; i < 2; i++) { secondMandala[i] = rnd.Range(0, 8); }
        if (secondMandala[0] == secondMandala[1]) { goto secondMandala; }
        binary = Convert.ToString(firstMandala, 2);
        while (binary.Length < 3) binary = "0" + binary;
        secondMandalaText.text = table[secondMandala[0]][secondMandala[1]].ToString();
        values[0,0] = secondMandala[0] + 1;
        values[0,1] = secondMandala[1] + 1;
        values[1,1] = secondMandala[0] + 1;
        values[1,0] = secondMandala[1] + 1;
        values[2, 0] = secondMandala[0] + secondMandala[1];
        values[2, 1] = Math.Abs(secondMandala[0] - secondMandala[1]);
        Color color = colors[rnd.Range(0, colors.Length - 2)];
        firstMandalaText.color = color;
        secondMandalaText.color = color;
    }
    IEnumerator Hold()
    {
        heldTime = bomb.GetTime();
        int colorIndex = 0;
        while (!released)
        {
            colorIndex++;
            if (colorIndex == colors.Length) colorIndex = 0;
            firstMandalaText.color = colors[colorIndex];
            secondMandalaText.color = colors[colorIndex];
            yield return new WaitForSeconds(0.2f);
        }
    }
    void Release()
    {
        releasedTime = bomb.GetTime();
        if ((int)heldTime == (int)releasedTime)
        {
            if (binary[stage] == '1')
            {
                if ((int)releasedTime % 60 == 10 * (values[stage, 0] % 6) + values[stage, 1])
                {
                    stage++;
                    if (stage == 3)
                    {
                        module.HandlePass();
                        solved = true;
                    }
                    else module.HandleStrike();
                }
            }
            else module.HandleStrike();
        }
        else
        {
            if (binary[stage] == '0')
            {
                if ((int)heldTime % 60 % 10 == values[stage, 0] && (int)releasedTime % 60 % 10 == values[stage, 1])
                {
                    stage++;
                    if (stage == 3)
                    {
                        module.HandlePass();
                        solved = true;
                    }
                }
                else module.HandleStrike();
            }
            else module.HandleStrike();
        } 
        
    }
}
