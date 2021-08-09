using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordsService {

    // Select language
    public enum Language {
        Persian,
        English,
        Arabic
    };
    public Language language;

    // Array of words
    private string[] words;

    // Read all words from text file
    public void ReadString()
    {
        if (language == Language.Persian) {
            TextAsset txt = (TextAsset)Resources.Load("Texts/PersianDatabase", typeof(TextAsset));
            words = txt.text.Split('+');
        } else if (language == Language.English) {
            TextAsset txt = (TextAsset)Resources.Load("Texts/EnglishDatabase", typeof(TextAsset));
            words = txt.text.Split('+');
        } else if (language == Language.Arabic) {
            TextAsset txt = (TextAsset)Resources.Load("Texts/ArabicDatabase", typeof(TextAsset));
            words = txt.text.Split('+');
        }
    }

    // Search Array For Check User Input Exist
    public bool IsWordExist(string input) {
        //int index = System.Array.BinarySearch(words, input);
        int index = System.Array.IndexOf(words, input);
        //Debug.Log(System.Array.IndexOf(words, input));
        if (index >= 0) {
            return true;
        } else {
            return false;
        }
    }

    // Return Array Of Strings  With Desired Lenght
    public string[] GetArrayWithLength(int length) {
        string[] array = System.Array.FindAll(words, s => s.Length == length);

        return array;
    }

    // Pick Random String With Desired Length
    public string GetWordWithLength(int length)
    {
        string[] lengthWords = System.Array.FindAll(words, s => s.Length == length);

        return lengthWords[Random.Range(0, lengthWords.Length)];
    }

    // Check the word contains characters
    public bool IsWordContaionsChars(string word, char[] chars) {
        if (chars.Length < word.Length)
            return false;

        List<char> lettersChars = new List<char>(0);
        List<char> wordChars = new List<char>(0);

        lettersChars.AddRange(chars);
        wordChars.AddRange(word);

        for (int i = 0; i < word.Length; i++) {
            if (lettersChars.Contains(word[i])) {
                int cIndex = lettersChars.IndexOf(word[i]);
                if (cIndex != -1) {
                    lettersChars.Remove(lettersChars[cIndex]);
                }

                cIndex = wordChars.IndexOf(word[i]);
                if (cIndex != -1) {
                    wordChars.Remove(wordChars[cIndex]);
                }
            }
        }

        if (wordChars.Count == 0) {
            return true;
        }

        return false;
    }
}
