using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GTS.AOC
{
    public class UIListener : MonoBehaviour
    {
        private TextMeshProUGUI counterText;
        private TextMeshProUGUI searchTypeText;
        private SearchType searchType;
        private int enumInt = 0;
        private int enumCount = 0;
        Button b;
        private void Start()
        {
            counterText = GameObject.Find("Counter_Text").GetComponent<TextMeshProUGUI>();
            searchTypeText = GameObject.Find("SearchType_Text").GetComponent<TextMeshProUGUI>();

            searchType = FindObjectOfType<Spawner>().scanType;
            enumInt = (int)searchType;
            enumCount = Enum.GetNames(typeof(SearchType)).Length;
            Debug.Log(enumCount);
            searchTypeText.text = ConvertToTitleCase(searchType);

            BaseStation bs = FindObjectOfType<BaseStation>();
            bs.OnCounterUpdated += UpdateText;

            b = GameObject.Find("StartButton").GetComponent<Button>();
            b.onClick.AddListener(() => bs.StartOnClick(searchType));
            b.onClick.AddListener(() => DisablePanel());

            Button r = GameObject.Find("Right").GetComponent<Button>();
            r.onClick.AddListener(() => ChangeType(true));

            Button l = GameObject.Find("Left").GetComponent<Button>();
            l.onClick.AddListener(() => ChangeType(false));
        }

        private void ChangeType(bool right)
        {
            if (right)
            {
                enumInt = (enumInt + 1) % enumCount;
            }
            else
            {
                enumInt = (enumInt - 1);
                if (enumInt < 0)
                {
                    enumInt = enumCount - 1;
                }
            }

            Debug.Log(enumInt);
            searchType = (SearchType)enumInt;

            searchTypeText.text = ConvertToTitleCase(searchType);
        }

        private void DisablePanel()
        {
            Destroy(GameObject.Find("BottomPanel").gameObject);
        }

        private void UpdateText(int count)
        {
            counterText.text = count.ToString();
        }

        private void OnApplicationQuit()
        {
            FindObjectOfType<BaseStation>().OnCounterUpdated -= UpdateText;
        }

        private string ConvertToTitleCase(SearchType st)
        {
            string s = st.ToString();
            string[] ss = s.Split('_');
            s = string.Empty;
            foreach (var word in ss)
            {
                s += (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(word.ToLower()) + " ");
            }
            return s;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                b.onClick.Invoke();
            }
        }
    }
}