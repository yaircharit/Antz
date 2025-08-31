using TMPro;
using UnityEngine;

public class PopupWindow : MonoBehaviour
{
    [Header("Text GameObjects")]
    [SerializeField] public TMP_Text headerObject;
    [SerializeField] public TMP_Text bodyObject;

    public string Header
    {
        get {  return headerObject.text; }
        set { headerObject.text = value; }
    }

    public string Body
    {
        get { return bodyObject.text; }
        set { bodyObject.text = value; }
    }

    public bool Active
    {
        get { return gameObject.activeInHierarchy; }
        set { gameObject.SetActive( value); }
    }

    //private void LateUpdate()
    //{
    //    // Make the popup window face the player's camera
    //    if (Camera.main != null)
    //    {
    //        transform.LookAt(Camera.main.transform);
    //        transform.Rotate(0, 180, 0); // Fix mirror flip
    //        // Optionally, keep upright
    //        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    //    }
    //}
}

