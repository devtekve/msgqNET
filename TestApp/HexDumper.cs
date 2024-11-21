using System;
using System.Text;

namespace TestApp;

public static class HexDumper
{
    public static void PrintHexDump(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            Console.WriteLine("No data to display.");
            return;
        }

        // Process in groups of 16 bytes
        for (int i = 0; i < data.Length; i += 16)
        {
            // Print offset
            Console.Write($"{i:X8}: ");

            // Print hex representation
            for (int j = 0; j < 16; j++)
            {
                if (i + j < data.Length)
                {
                    Console.Write($"{data[i + j]:X2} ");
                }
                else
                {
                    Console.Write("   ");
                }

                // Add an extra space after 8 bytes to improve readability
                if (j == 7)
                {
                    Console.Write(" ");
                }
            }

            Console.Write(" ");

            // Print ASCII representation
            for (int j = 0; j < 16; j++)
            {
                if (i + j < data.Length)
                {
                    char c = (char)data[i + j];
                    // Print printable ASCII characters, replace non-printable with '.'
                    Console.Write(IsPrintable(c) ? c : '.');
                }
            }

            Console.WriteLine();
        }
    }

    private static bool IsPrintable(char c)
    {
        // Check if the character is a printable ASCII character
        return c >= 32 && c < 127;
    }

    // Optional method if you want to use with a string or other object
    public static void PrintHexDump(object x)
    {
        if (x == null)
        {
            Console.WriteLine("Object is null.");
            return;
        }

        // Attempt to get byte array from different types
        byte[] data = null;
        
        switch (x)
        {
            case byte[] byteArray:
                data = byteArray;
                break;
            case string str:
                data = Encoding.UTF8.GetBytes(str);
                break;
            default:
                // If it's an object with a GetData() method
                var method = x.GetType().GetMethod("GetData");
                if (method != null)
                {
                    data = (byte[])method.Invoke(x, null);
                }
                break;
        }

        if (data == null)
        {
            Console.WriteLine("Could not convert object to byte array.");
            return;
        }

        PrintHexDump(data);
    }
}