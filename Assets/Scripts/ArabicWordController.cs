using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ArabicWordController : MonoBehaviour {

    // Modal Coroutine
    public Coroutine modalCoroutine = null;
    // Leaderboard item prefab
    public GameObject wordListItemPrefab;

    // Error Objects
    private GameObject errorDialog;
    private Image errorDialogImageUI;
    private Text errorDialogTextUI;

    // User letters InputField
    private InputField lettersInputField;
    // Save directory path Text
    private Text saveDirectoryText;
    // Generate Progress Image
    private GameObject progressUI;
    // Words Service
    private WordsService ws;

    // Leaderboard objects
    private GameObject wordsListUI;
    private ScrollRect wordsListScrollRect;
    private GameObject wordsListContent;
    // Words list item instances parent
    private Transform wordsListItemInstancesParent;

    // Use this for initialization
    void Start () {
        lettersInputField = GameObject.Find("Canvas/LettersInputField").GetComponent<InputField>();

        saveDirectoryText = GameObject.Find("Canvas/SaveDirectory/SaveDirectoryText").GetComponent<Text>();

        errorDialog = GameObject.Find("Canvas/ErrorDialog");
        errorDialogImageUI = errorDialog.GetComponent<Image>();
        errorDialogTextUI = errorDialog.transform.GetChild(0).GetComponent<Text>();
        errorDialog.SetActive(false);

        progressUI = GameObject.Find("Canvas/ProgressUI");
        progressUI.SetActive(false);

        // Create default path if not exists
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsEditor) {
            string defaultPath = "C:\\Word Finder";
            Directory.CreateDirectory(defaultPath);
            saveDirectoryText.text = defaultPath;
        } else if (Application.platform == RuntimePlatform.Android) {
            saveDirectoryText.text = PathForDocumentsFile("");
        }


        // Instance of WordsService
        ws = new WordsService();
        // Set WordsService to arabic language
        ws.language = WordsService.Language.Arabic;
        // Get all words
        ws.ReadString();

        wordsListUI = GameObject.Find("Canvas/WordsListUI");
        wordsListScrollRect = GameObject.Find("Canvas/WordsListUI/WordsListPanel").GetComponent<ScrollRect>();
        wordsListContent = GameObject.Find("Canvas/WordsListUI/WordsListPanel/WordsListViewport/WordsListContent");

        wordsListItemInstancesParent = GameObject.Find("WordsListItemInstances").GetComponent<Transform>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // Return all possible strings from given string
    public IEnumerable<string> Permutate(string source, int count) {
        if (source.Length == 1) {
            yield return source;
        } else if (count == 1) {
            for (var n = 0; n < source.Length; n++) {
                yield return source.Substring(n, 1);
            }
        } else {
            for (var n = 0; n < source.Length; n++)
                foreach (var suffix in Permutate(
                    source.Substring(0, n)
                        + source.Substring(n + 1, source.Length - n - 1), count - 1)) {
                    yield return source.Substring(n, 1) + suffix;
                }
        }
    }

    // Callback for generate words Button
    public void OnClickGenerateWordsButton() {
        if (saveDirectoryText.text.Trim().ToString().Length > 0) {
            if (lettersInputField.text.Trim().ToString().Length > 0) {
                StartCoroutine(FindWords());
            } else {
                modalCoroutine = StartCoroutine(ShowModalDialog(errorDialogImageUI, "Please Enter Letters!", 1.5f));
            }
        } else {
            modalCoroutine = StartCoroutine(ShowModalDialog(errorDialogImageUI, "Please Select A Folder For Saving Words Files!", 1.5f));
        }
    }

    // Callback for words save directory Button
    public void OnClickWordsSaveDirectoryButton() {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);
        saveDirectoryText.text = WriteResult(paths).Trim();
    }

    // Show Modal Dialog
    IEnumerator ShowModalDialog(Image img, string message, float time) {
        if (modalCoroutine != null)
            StopCoroutine(modalCoroutine);

        errorDialogTextUI.text = message;
        errorDialog.SetActive(true);
        Color imageBaseColor = img.color;
        Color textBaseColor = errorDialogTextUI.color;

        // fade from transparent to opaque
        for (float i = 0; i <= time; i += Time.deltaTime) {
            // set color with i as alpha
            img.color = new Color(imageBaseColor.r, imageBaseColor.g, imageBaseColor.b, i);
            errorDialogTextUI.color = new Color(errorDialogTextUI.color.r, errorDialogTextUI.color.g, errorDialogTextUI.color.b, i);
            yield return null;
        }

        // fade from opaque to transparent
        for (float i = time; i >= 0; i -= Time.deltaTime) {
            // set color with i as alpha
            img.color = new Color(imageBaseColor.r, imageBaseColor.g, imageBaseColor.b, i);
            errorDialogTextUI.color = new Color(errorDialogTextUI.color.r, errorDialogTextUI.color.g, errorDialogTextUI.color.b, i);
            yield return null;
        }

        errorDialog.SetActive(false);
    }

    // Directory path maker
    private string WriteResult(string[] paths) {
        string _path = "";

        if (paths.Length == 0) {
            return "";
        }

        _path = "";
        foreach (var p in paths) {
            _path += p + "\n";
        }

        return _path;
    }

    // Find Words
    private IEnumerator FindWords() {
        // Get user input
        string letters = lettersInputField.text.Trim().ToString();

        // Remove conflicts with arabic letters
        if (letters.Contains("ک"))
            letters = letters.Replace("ک", "ك");

        // Show progress UI
        progressUI.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        // Placeholder of database words
        List<string> dbWords = new List<string>();

        // Check if user input contains 'ا' because we should generate possible words that contains 'آ' char too
        string newLetters1 = "";
        string newLetters2 = "";
        string newLetters3 = "";
        if (letters.Contains("ا")) {
            newLetters1 = letters;
            int i1 = newLetters1.IndexOf('ا');
            StringBuilder sb1 = new StringBuilder(newLetters1);
            sb1[i1] = 'آ';
            newLetters1 = sb1.ToString();

            newLetters2 = letters;
            int i2 = newLetters2.IndexOf('ا');
            StringBuilder sb2 = new StringBuilder(newLetters2);
            sb2[i2] = 'أ';
            newLetters2 = sb2.ToString();

            newLetters3 = letters;
            int i3 = newLetters3.IndexOf('ا');
            StringBuilder sb3 = new StringBuilder(newLetters3);
            sb3[i3] = 'إ';
            newLetters3 = sb3.ToString();
        }

        if (letters.Contains("آ")) {
            newLetters1 = letters;
            int i1 = newLetters1.IndexOf('آ');
            StringBuilder sb1 = new StringBuilder(newLetters1);
            sb1[i1] = 'ا';
            newLetters1 = sb1.ToString();

            newLetters2 = letters;
            int i2 = newLetters2.IndexOf('آ');
            StringBuilder sb2 = new StringBuilder(newLetters2);
            sb2[i2] = 'أ';
            newLetters2 = sb2.ToString();

            newLetters3 = letters;
            int i3 = newLetters3.IndexOf('آ');
            StringBuilder sb3 = new StringBuilder(newLetters3);
            sb3[i3] = 'إ';
            newLetters3 = sb3.ToString();
        }

        if (letters.Contains("أ")) {
            newLetters1 = letters;
            int i1 = newLetters1.IndexOf('أ');
            StringBuilder sb1 = new StringBuilder(newLetters1);
            sb1[i1] = 'آ';
            newLetters1 = sb1.ToString();

            newLetters2 = letters;
            int i2 = newLetters2.IndexOf('أ');
            StringBuilder sb2 = new StringBuilder(newLetters2);
            sb2[i2] = 'ا';
            newLetters2 = sb2.ToString();

            newLetters3 = letters;
            int i3 = newLetters3.IndexOf('أ');
            StringBuilder sb3 = new StringBuilder(newLetters3);
            sb3[i3] = 'إ';
            newLetters3 = sb3.ToString();
        }

        if (letters.Contains("إ")) {
            newLetters1 = letters;
            int i1 = newLetters1.IndexOf('إ');
            StringBuilder sb1 = new StringBuilder(newLetters1);
            sb1[i1] = 'آ';
            newLetters1 = sb1.ToString();

            newLetters2 = letters;
            int i2 = newLetters2.IndexOf('إ');
            StringBuilder sb2 = new StringBuilder(newLetters2);
            sb2[i2] = 'ا';
            newLetters2 = sb2.ToString();

            newLetters3 = letters;
            int i3 = newLetters3.IndexOf('إ');
            StringBuilder sb3 = new StringBuilder(newLetters3);
            sb3[i3] = 'أ';
            newLetters3 = sb3.ToString();
        }

        // Get all words with minimum length of 2 and maximum length of user input letters count
        for (int i = 2; i <= letters.Trim().ToString().Length; i++) {
            dbWords.AddRange(ws.GetArrayWithLength(i));
        }

        // Get final words
        List<string> finalWords = new List<string>();
        for (int i = 0; i < dbWords.Count; i++) {
            string word = dbWords[i];

            // Check the word has chars of letters array
            if (ws.IsWordContaionsChars(word, letters.ToArray())) {
                // Check we didn't add the word previously
                if (!finalWords.Contains(word)) {
                    // Add final word
                    finalWords.Add(word);
                }
            }

            if (!newLetters1.Equals("")) {
                // Check the word has chars of letters array
                if (ws.IsWordContaionsChars(word, newLetters1.ToArray())) {
                    // Check we didn't add the word previously
                    if (!finalWords.Contains(word)) {
                        // Add final word
                        finalWords.Add(word);
                    }
                }
            }

            if (!newLetters2.Equals("")) {
                // Check the word has chars of letters array
                if (ws.IsWordContaionsChars(word, newLetters2.ToArray())) {
                    // Check we didn't add the word previously
                    if (!finalWords.Contains(word)) {
                        // Add final word
                        finalWords.Add(word);
                    }
                }
            }

            if (!newLetters3.Equals("")) {
                // Check the word has chars of letters array
                if (ws.IsWordContaionsChars(word, newLetters2.ToArray())) {
                    // Check we didn't add the word previously
                    if (!finalWords.Contains(word)) {
                        // Add final word
                        finalWords.Add(word);
                    }
                }
            }
        }

        // Path of text file for storing final words
        string filePath = saveDirectoryText.text.Trim().ToString() + "/" + letters.Trim().ToString() + ".txt";
        // Remove previous text file if exists
        if (File.Exists(filePath)) {
            Debug.Log(filePath + " already exists.");
            File.Delete(filePath);
        }
        // Write all words to the text file
        File.WriteAllLines(filePath, finalWords.ToArray());
        // Fill words list
        ShowWordsList(finalWords.ToArray());

        yield return new WaitForSeconds(0.1f);
        // Hide progress UI
        progressUI.SetActive(false);
    }

    // Callback for exit button
    public void OnClickExitButton() {
        Application.Quit();
    }

    // Return Back UI Instances
    private void ReturnBackInstances(Transform from, Transform to, int returnCount, bool setActiveChild) {

        GameObject[] fromChilds = new GameObject[returnCount];
        for (int i = 0; i < returnCount; i++) {
            fromChilds[i] = from.GetChild(i).gameObject;
            fromChilds[i].SetActive(setActiveChild);
            fromChilds[i].GetComponent<Button>().onClick.RemoveAllListeners();
        }

        for (int i = 0; i < returnCount; i++) {
            fromChilds[i].transform.SetParent(to);
        }
    }

    // Show words list
    private void ShowWordsList(string[] words) {
        int currentItemsListIndex = 0;

        // Check if words list is filled
        if (wordsListContent.transform.childCount != 0) {
            // Check if words list items are more that new words
            if (wordsListContent.transform.childCount > words.Length) {
                // Delete unused items
                for (int x = wordsListContent.transform.childCount - 1; x >= words.Length; x--) {
                    wordsListContent.transform.GetChild(x).SetParent(wordsListItemInstancesParent);
                }
                for (int i = wordsListItemInstancesParent.childCount - 1; i >= 0; i--) {
                    Destroy(wordsListItemInstancesParent.transform.GetChild(i).gameObject);
                }

                // Update words list with new words
                currentItemsListIndex = words.Length;
                for (int i = 0; i < currentItemsListIndex; i++) {
                    Transform childTransform = wordsListContent.transform.GetChild(i);
                    string word = words[i];
                    // Change word item text
                    Text wordListItemText = childTransform.transform.GetChild(0).GetComponent<Text>();
                    wordListItemText.text = Fa.faConvert(word);
                    // Add click listener for word item
                    childTransform.GetComponent<Button>().onClick.RemoveAllListeners();
                    childTransform.GetComponent<Button>().onClick.AddListener(() => OnClickWordItemButton(word));
                }
            } else if (wordsListContent.transform.childCount == words.Length) {
                // Update words list with new words
                currentItemsListIndex = wordsListContent.transform.childCount;
                for (int i = 0; i < currentItemsListIndex; i++) {
                    Transform childTransform = wordsListContent.transform.GetChild(i);
                    string word = words[i];
                    // Change word item text
                    Text wordListItemText = childTransform.transform.GetChild(0).GetComponent<Text>();
                    wordListItemText.text = Fa.faConvert(word);
                    // Add click listener for word item
                    childTransform.GetComponent<Button>().onClick.RemoveAllListeners();
                    childTransform.GetComponent<Button>().onClick.AddListener(() => OnClickWordItemButton(word));
                }
            } else if (wordsListContent.transform.childCount < words.Length) {
                // Update words list with new words
                currentItemsListIndex = wordsListContent.transform.childCount;
                for (int i = 0; i < currentItemsListIndex; i++) {
                    Transform childTransform = wordsListContent.transform.GetChild(i);
                    string word = words[i];
                    // Change word item text
                    Text wordListItemText = childTransform.GetChild(0).GetComponent<Text>();
                    wordListItemText.text = Fa.faConvert(word);
                    // Add click listener for word item
                    childTransform.GetComponent<Button>().onClick.RemoveAllListeners();
                    childTransform.GetComponent<Button>().onClick.AddListener(() => OnClickWordItemButton(word));
                }

                // Instantiate new items
                currentItemsListIndex = words.Length - wordsListContent.transform.childCount;
                // Word index
                int wordIndex = wordsListContent.transform.childCount;
                for (int i = 0; i < currentItemsListIndex; i++) {
                    GameObject wordItem = GameObject.Instantiate(wordListItemPrefab, Vector3.zero, Quaternion.identity, wordsListContent.transform) as GameObject;
                    wordItem.name = "Word Item " + i;
                    string word = words[wordIndex + i];
                    // Change word item text
                    Text wordListItemText = wordItem.transform.GetChild(0).GetComponent<Text>();
                    wordListItemText.text = Fa.faConvert(word);
                    // Add click listener for word item
                    wordItem.GetComponent<Button>().onClick.AddListener(() => OnClickWordItemButton(word));
                }
            }
        } else {
            // Instantiate new items
            for (int w = 0; w < words.Length; w++) {
                GameObject wordItem = GameObject.Instantiate(wordListItemPrefab, Vector3.zero, Quaternion.identity, wordsListContent.transform) as GameObject;
                wordItem.name = "Word Item " + w;
                string word = words[w];

                // Change word item text
                Text wordListItemText = wordItem.transform.GetChild(0).GetComponent<Text>();
                wordListItemText.text = Fa.faConvert(word);

                // Add click listener for word item
                wordItem.GetComponent<Button>().onClick.AddListener(() => OnClickWordItemButton(word));
            }

            currentItemsListIndex = words.Length;
        }
    }

    // Callback for words list item Button click
    private void OnClickWordItemButton(string word) {
        //Debug.Log(word);
        CopyToClipboard(word);
    }

    // Copy string to clipboard
    private void CopyToClipboard(string s) {
        TextEditor te = new TextEditor();
        te.text = s;
        te.SelectAll();
        te.Copy();
    }

    private string PathForDocumentsFile(string filename) {
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);
        } else if (Application.platform == RuntimePlatform.Android) {
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        } else {
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
    }
}
