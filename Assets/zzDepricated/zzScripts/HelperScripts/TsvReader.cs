using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using UnityEngine;

public class TsvReader
{
    public static string[][] ReadTsv(string filePath)
    {
        // Read all lines from the file
        string[] lines = File.ReadAllLines(filePath);
        string[][] entries = new string[lines.Length][];

        // Process each line
        for (int lineNum = 0; lineNum < lines.Length; lineNum++ )
        {
            UnityEngine.Debug.Log("3 - " + lines[lineNum]);
            // Split the line by the tab delimiter ('\t')
            entries[lineNum] = lines[lineNum].Split('\t');
        }
        return entries;
    }
}
