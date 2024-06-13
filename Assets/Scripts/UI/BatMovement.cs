using Enums;
using UnityEngine;

public class BatMovement : MonoBehaviour
{
    public float timer = 0;
    public float resetInterval = 25f;
    public Vector3 startPosition = new Vector3(0, 0, 0);

    private void Start()
    {
        transform.position = startPosition;
        Application.targetFrameRate = 60;
        
        AudioManager.Instance.PlaySoundOneShoot(ESoundType.Bat);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= resetInterval)
        {
            AudioManager.Instance.PlaySoundOneShoot(ESoundType.Bat);
            timer = 0;
            transform.position = startPosition;
        }
        else
        {
            transform.position += new Vector3(-5f, 3f, 0);
        }
        
    }
}
