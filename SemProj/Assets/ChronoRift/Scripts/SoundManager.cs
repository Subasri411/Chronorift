using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<SoundManager>();
            return instance;
        }
        private set => instance = value;
    }

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    private AudioClip jumpClip;
    private AudioClip eggClip;
    private AudioClip altarClip;
    private AudioClip gateClip;
    private AudioClip victoryClip;
    private AudioClip clickClip;
    private AudioClip deniedClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = 0.25f;

        GenerateClips();
        StartAmbience();
    }

    private void GenerateClips()
    {
        jumpClip = CreateTone(0.15f, 44100, (t, dur) => {
            float freq = Mathf.Lerp(160f, 480f, t / dur);
            float env = 1f - (t / dur);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.4f;
        });

        eggClip = CreateTone(0.35f, 44100, (t, dur) => {
            float p = t / dur;
            float freq = p < 0.33f ? 523.25f : (p < 0.66f ? 659.25f : 783.99f);
            float notePhase = (p % 0.33f) / 0.33f;
            float env = 1f - notePhase;
            float s = Mathf.Sin(2f * Mathf.PI * freq * t);
            return s * env * 0.5f;
        });

        altarClip = CreateTone(0.7f, 44100, (t, dur) => {
            float env = Mathf.Sin((t / dur) * Mathf.PI);
            float f1 = 220f + Mathf.Sin(t * 15f) * 8f;
            float f2 = 329.63f;
            float f3 = 440f;
            float s = (Mathf.Sin(2f * Mathf.PI * f1 * t) + Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.7f + Mathf.Sin(2f * Mathf.PI * f3 * t) * 0.5f) / 2.2f;
            return s * env * 0.6f;
        });

        gateClip = CreateTone(0.9f, 44100, (t, dur) => {
            float env = Mathf.Sin((t / dur) * Mathf.PI);
            float rumble = Mathf.Sin(2f * Mathf.PI * 55f * t) + Mathf.Sin(2f * Mathf.PI * 73f * t);
            float noise = (Random.value * 2f - 1f) * 0.3f;
            return (rumble * 0.6f + noise) * env * 0.6f;
        });

        victoryClip = CreateTone(1.4f, 44100, (t, dur) => {
            float p = t / dur;
            float freq;
            if (p < 0.2f) freq = 523.25f;
            else if (p < 0.4f) freq = 659.25f;
            else if (p < 0.6f) freq = 783.99f;
            else if (p < 0.8f) freq = 987.77f;
            else freq = 1046.5f;
            float notePhase = (p % 0.2f) / 0.2f;
            float env = 1f - (p > 0.8f ? (p - 0.8f) / 0.2f : notePhase * 0.7f);
            float s = Mathf.Sin(2f * Mathf.PI * freq * t) + Mathf.Sin(4f * Mathf.PI * freq * t) * 0.25f;
            return s * env * 0.5f;
        });

        clickClip = CreateTone(0.04f, 44100, (t, dur) => {
            float env = 1f - (t / dur);
            return Mathf.Sin(2f * Mathf.PI * 1200f * t) * env * 0.3f;
        });

        deniedClip = CreateTone(0.25f, 44100, (t, dur) => {
            float env = 1f - (t / dur);
            float s = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 130f * t));
            return s * env * 0.35f;
        });
    }

    private void StartAmbience()
    {
        AudioClip ambient = CreateTone(4.0f, 22050, (t, dur) => {
            float wave1 = Mathf.Sin(2f * Mathf.PI * 110f * t);
            float wave2 = Mathf.Sin(2f * Mathf.PI * 164.81f * t) * 0.6f;
            float wave3 = Mathf.Sin(2f * Mathf.PI * 220f * t + Mathf.Sin(t * 1.5f)) * 0.4f;
            return (wave1 + wave2 + wave3) * 0.15f;
        });
        bgmSource.clip = ambient;
        bgmSource.Play();
    }

    private AudioClip CreateTone(float duration, int sampleRate, System.Func<float, float, float> generator)
    {
        int sampleCount = Mathf.RoundToInt(duration * sampleRate);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            samples[i] = Mathf.Clamp(generator(t, duration), -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("ChronoTone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlayJump() { if (Application.isPlaying && sfxSource != null && jumpClip != null) sfxSource.PlayOneShot(jumpClip, 0.7f); }
    public void PlayEggPickup() { if (Application.isPlaying && sfxSource != null && eggClip != null) sfxSource.PlayOneShot(eggClip, 0.85f); }
    public void PlayAltarUnlock() { if (Application.isPlaying && sfxSource != null && altarClip != null) sfxSource.PlayOneShot(altarClip, 0.9f); }
    public void PlayGateOpen() { if (Application.isPlaying && sfxSource != null && gateClip != null) sfxSource.PlayOneShot(gateClip, 0.95f); }
    public void PlayVictory() { if (Application.isPlaying && sfxSource != null && victoryClip != null) sfxSource.PlayOneShot(victoryClip, 1f); }
    public void PlayButtonClick() { if (Application.isPlaying && sfxSource != null && clickClip != null) sfxSource.PlayOneShot(clickClip, 0.6f); }
    public void PlayDenied() { if (Application.isPlaying && sfxSource != null && deniedClip != null) sfxSource.PlayOneShot(deniedClip, 0.7f); }
}
