using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] public GameObject Player;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, -10);
    }
}
