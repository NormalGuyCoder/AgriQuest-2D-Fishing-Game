using UnityEngine;

[DefaultExecutionOrder(-300)]
public class DetachOnStart : MonoBehaviour
{
    void Awake()
    {
        if (transform.parent != null)
        {

            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            Vector3 scale = transform.localScale;

            transform.SetParent(null);

            transform.position = pos;
            transform.rotation = rot;
            transform.localScale = scale;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}