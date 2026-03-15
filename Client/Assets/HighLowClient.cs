using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;
using System.Collections.Generic;

public class HighLowClient : MonoBehaviour
{
    public InputField numInput;
    public Text resultText;
    public Text attemptsText;
    public Text historyText;

    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;

    private int attempts = 0;
    private List<string> guessLog = new List<string>();

    void Start()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 50001);
            NetworkStream ns = client.GetStream();
            reader = new StreamReader(ns);
            writer = new StreamWriter(ns);
            resultText.text = "Connected!";
            resultText.color = Color.white;
        }
        catch (Exception e)
        {
            resultText.text = "Failed to connect.";
            resultText.color = Color.red;
            Debug.LogError(e.Message);
        }
    }

    void Update()
    {
        if (client == null || !client.Connected) return;

        // let player press Enter to submit
        if (Input.GetKeyDown(KeyCode.Return))
            OnGuessButtonPressed();

        NetworkStream ns = client.GetStream();
        if (ns.DataAvailable)
        {
            string msg = reader.ReadLine();
            HandleServerMessage(msg);
        }
    }

    void HandleServerMessage(string msg)
    {
        resultText.text = msg;

        // color based on hint
        string lower = msg.ToLower();
        if (lower.Contains("higher"))
            resultText.color = Color.yellow;
        else if (lower.Contains("lower"))
            resultText.color = new Color(1f, 0.5f, 0f); // orange
        else if (lower.Contains("correct") || lower.Contains("win"))
        {
            resultText.color = Color.green;
            guessLog.Clear();
            attempts = 0;
            if (attemptsText != null)
                attemptsText.text = "Attempts: 0";
            RefreshHistory();
        }
        else
            resultText.color = Color.white;
    }

    void RefreshHistory()
    {
        if (historyText == null) return;
        historyText.text = string.Join("\n", guessLog.ToArray());
    }

    public void OnGuessButtonPressed()
    {
        if (string.IsNullOrEmpty(numInput.text))
        {
            resultText.text = "Enter a number first!";
            resultText.color = Color.white;
            return;
        }

        int guess;
        if (!int.TryParse(numInput.text, out guess))
        {
            resultText.text = "Invalid input";
            return;
        }

        if (guess < 0 || guess > 100)
        {
            resultText.text = "Must be between 0 and 100";
            return;
        }

        if (client != null)
        {
            writer.WriteLine(guess);
            writer.Flush();

            attempts++;
            if (attemptsText != null)
                attemptsText.text = "Attempts: " + attempts;

            guessLog.Add("Guess #" + attempts + ": " + guess);
            RefreshHistory();

            numInput.text = "";
        }
    }
}