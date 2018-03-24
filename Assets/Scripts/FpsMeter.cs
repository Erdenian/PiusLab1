using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FpsMeter : MonoBehaviour {

    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    void Update () {
        text.text = (1.0f / Time.unscaledDeltaTime).ToString();
	}
}
