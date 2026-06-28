using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    private AudioClip attackClip;
    private AudioClip killClip;
    private AudioClip hoverClip;
    private AudioClip clickClip;
    private AudioClip bgmClip;
    private AudioClip gameOverClip;
    private AudioClip abilityClip;
    private AudioClip loadingClip;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        // Add AudioSources
        sfxSource = gameObject.AddComponent<AudioSource>();
        bgmSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = 0.25f; // Standard background music volume
        sfxSource.volume = 0.5f;

        // Pre-generate procedural sound clips
        attackClip = CreateAttackClip();
        killClip = CreateKillClip();
        hoverClip = CreateHoverClip();
        clickClip = CreateClickClip();
        bgmClip = CreateBGMClip();
        gameOverClip = CreateGameOverClip();
        abilityClip = CreateAbilityClip();
        loadingClip = CreateLoadingClip();

        // Start playing Background Music
        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    public void PlayAttackSound()
    {
        if (sfxSource != null && attackClip != null)
            sfxSource.PlayOneShot(attackClip, 0.6f);
    }

    public void PlayKillSound()
    {
        if (sfxSource != null && killClip != null)
            sfxSource.PlayOneShot(killClip, 0.7f);
    }

    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverClip != null)
            sfxSource.PlayOneShot(hoverClip, 0.3f);
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickClip != null)
            sfxSource.PlayOneShot(clickClip, 0.5f);
    }

    public void PlayGameOverSound()
    {
        if (bgmSource != null)
            bgmSource.Stop(); // stop looping BGM immediately

        if (sfxSource != null && gameOverClip != null)
            sfxSource.PlayOneShot(gameOverClip, 0.8f);
    }

    public void PlayAbilitySound()
    {
        if (sfxSource != null && abilityClip != null)
            sfxSource.PlayOneShot(abilityClip, 0.7f);
    }

    public void PlayLoadingSound()
    {
        if (sfxSource != null && loadingClip != null)
            sfxSource.PlayOneShot(loadingClip, 0.25f);
    }

    // Call this to restart the BGM loop (e.g. on scene reload)
    public void RestartBGM()
    {
        if (bgmSource != null && bgmClip != null && !bgmSource.isPlaying)
        {
            bgmSource.clip = bgmClip;
            bgmSource.Play();
        }
    }

    // Procedural Synth Sound Generators

    private AudioClip CreateAttackClip()
    {
        int sampleRate = 44100;
        float duration = 0.12f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Downward pitch sweep from 1000Hz to 150Hz
            float freq = Mathf.Lerp(1000f, 150f, t);
            // Dynamic phase calculation
            float phase = 2f * Mathf.PI * freq * (t * duration);
            float wave = Mathf.Sin(phase);
            // Exponential volume decay
            float envelope = Mathf.Exp(-6f * t);
            // White noise blend for a nice swoosh sound
            float noise = Random.Range(-0.06f, 0.06f);
            samples[i] = (wave + noise) * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("AttackSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateKillClip()
    {
        int sampleRate = 44100;
        float duration = 0.28f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Downward sweep noise & low frequency pulse (explosion)
            float noise = Random.Range(-1f, 1f);
            float envelope = Mathf.Exp(-4f * t);
            float pitchEnv = Mathf.Lerp(350f, 40f, t);
            float sinePhase = 2f * Mathf.PI * pitchEnv * (t * duration);
            float bassPulse = Mathf.Sin(sinePhase);
            // Combine noise and bass thud
            samples[i] = (noise * 0.55f + bassPulse * 0.45f) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("KillSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateHoverClip()
    {
        int sampleRate = 44100;
        float duration = 0.05f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = 950f;
            float wave = Mathf.Sin(2f * Mathf.PI * freq * (t * duration));
            float envelope = Mathf.Exp(-12f * t);
            samples[i] = wave * envelope * 0.2f;
        }

        AudioClip clip = AudioClip.Create("HoverSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateClickClip()
    {
        int sampleRate = 44100;
        float duration = 0.09f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Fast upward sweep
            float freq = Mathf.Lerp(500f, 1100f, t);
            float wave = Mathf.Sin(2f * Mathf.PI * freq * (t * duration));
            float envelope = Mathf.Exp(-6f * t);
            samples[i] = wave * envelope * 0.3f;
        }

        AudioClip clip = AudioClip.Create("ClickSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateBGMClip()
    {
        // 8-second chiptune step sequencer
        int sampleRate = 44100;
        float stepDuration = 0.22f;
        int stepSamples = (int)(sampleRate * stepDuration);
        int totalSteps = 32;
        int totalSamples = stepSamples * totalSteps;
        float[] samples = new float[totalSamples];

        // 32-step bassline progression
        float[] bassFreqs = {
            110.0f, 110.0f, 110.0f, 110.0f, 110.0f, 110.0f, 110.0f, 110.0f, // Am
            87.3f,  87.3f,  87.3f,  87.3f,  87.3f,  87.3f,  87.3f,  87.3f,  // F
            130.8f, 130.8f, 130.8f, 130.8f, 130.8f, 130.8f, 130.8f, 130.8f, // C
            98.0f,  98.0f,  98.0f,  98.0f,  98.0f,  98.0f,  98.0f,  98.0f   // G
        };

        // 32-step melody progression
        float[] melodyFreqs = {
            220.0f, 261.6f, 329.6f, 440.0f, 329.6f, 261.6f, 220.0f, 329.6f, // Am
            174.6f, 220.0f, 261.6f, 349.2f, 261.6f, 220.0f, 174.6f, 261.6f, // F
            261.6f, 329.6f, 392.0f, 523.3f, 392.0f, 329.6f, 261.6f, 392.0f, // C
            196.0f, 246.9f, 293.7f, 392.0f, 293.7f, 246.9f, 196.0f, 293.7f  // G
        };

        for (int step = 0; step < totalSteps; step++)
        {
            float baseBass = bassFreqs[step];
            float baseMelody = melodyFreqs[step];

            int startIdx = step * stepSamples;
            for (int i = 0; i < stepSamples; i++)
            {
                int sampleIdx = startIdx + i;
                float localT = (float)i / stepSamples;
                float absoluteT = (float)sampleIdx / sampleRate;

                // 1. Retro Bass: Square wave with low-pass style filter overlay
                float bassPhase = 2f * Mathf.PI * baseBass * absoluteT;
                float bassWave = Mathf.Sign(Mathf.Sin(bassPhase));
                float bassVolume = 0.12f * Mathf.Exp(-2f * localT);

                // 2. Chiptune Melody: Triangle wave (sweeps up/down linearly)
                float melodyPhase = 2f * Mathf.PI * baseMelody * absoluteT;
                float melodyWave = (Mathf.PingPong(melodyPhase / Mathf.PI, 2f) - 1f);
                // Pluck envelope for retro sound
                float melodyVolume = 0.14f * Mathf.Exp(-4f * localT);

                // 3. Retro Beat: Noise-based hi-hat / snare
                float noise = Random.Range(-1f, 1f);
                float noiseVolume = 0f;

                if (step % 4 == 2)
                {
                    // Snare hit (exponential decay noise)
                    noiseVolume = 0.06f * Mathf.Exp(-12f * localT);
                }
                else if (step % 2 == 0)
                {
                    // Hi-hat hit (very quick decay noise)
                    noiseVolume = 0.03f * Mathf.Exp(-25f * localT);
                }

                // Sum all components and store
                samples[sampleIdx] = (bassWave * bassVolume) + (melodyWave * melodyVolume) + (noise * noiseVolume);
            }
        }

        AudioClip clip = AudioClip.Create("BGM_Loop", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateGameOverClip()
    {
        int sampleRate = 44100;
        float duration = 1.2f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        float[] notes = { 261.6f, 196.0f, 155.6f, 130.8f }; // C4, G3, Eb3, C3
        float noteDuration = duration / notes.Length;
        int noteSamples = (int)(sampleRate * noteDuration);

        for (int i = 0; i < sampleCount; i++)
        {
            int noteIndex = Mathf.Clamp(i / noteSamples, 0, notes.Length - 1);
            float freq = notes[noteIndex];
            float localT = (float)(i % noteSamples) / noteSamples;
            float absoluteT = (float)i / sampleRate;

            float phase = 2f * Mathf.PI * freq * absoluteT;
            float wave = Mathf.Sin(phase);
            float envelope = Mathf.Exp(-3.2f * localT); // quick pluck decay for each note
            
            // Add a low-frequency hum to make it extra dramatic
            float subHum = Mathf.Sin(2f * Mathf.PI * 65.4f * absoluteT) * 0.3f; // C2 hum

            samples[i] = (wave * 0.7f + subHum * 0.3f) * envelope * 0.45f;
        }

        AudioClip clip = AudioClip.Create("GameOverSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateAbilityClip()
    {
        int sampleRate = 44100;
        float duration = 0.45f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Sweep up rapidly from 400Hz to 1600Hz
            float freq = Mathf.Lerp(400f, 1600f, t);
            float phase = 2f * Mathf.PI * freq * (t * duration);
            
            // Triangle wave for smooth chiptune glow
            float wave = (Mathf.PingPong(phase / Mathf.PI, 2f) - 1f);
            
            // Quick volume sweep in/out
            float envelope = Mathf.Sin(t * Mathf.PI); // swell curve

            samples[i] = wave * envelope * 0.35f;
        }

        AudioClip clip = AudioClip.Create("AbilitySound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateLoadingClip()
    {
        int sampleRate = 44100;
        float duration = 0.03f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = 1200f;
            float wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * (t * duration)));
            float envelope = Mathf.Exp(-20f * t);
            samples[i] = wave * envelope * 0.18f;
        }

        AudioClip clip = AudioClip.Create("LoadingTick", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
