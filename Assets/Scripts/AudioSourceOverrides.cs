
using System.Collections;
using UnityEngine;

public static class AudioSourceOverrides
{
    public static IEnumerator FadeOutAndDestroy(this AudioSource audioSource, float time = 0.3f)
    {
        if (time <= 0) yield break;

        var timer = 0f;
        var startVolume = audioSource.volume;
        
        while (timer < time)
        {
            timer += Time.deltaTime;
            audioSource.volume = startVolume * (1f - timer / time);
            yield return new WaitForEndOfFrame();
        }
        
        Object.Destroy(audioSource.gameObject);
    }
}