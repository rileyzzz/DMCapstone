using UnityEngine;

enum AudioTrack
{
    Default = 0,
    Environment,
    Industry,
    Money,
    Pollution
}

public class MusicManager : MonoBehaviour
{
    public AudioSource m_defaultMusic;
    public AudioSource m_environmentMusic;
    public AudioSource m_industryMusic;
    public AudioSource m_moneyMusic;
    public AudioSource m_pollutionMusic;

    private AudioTrack m_track = AudioTrack.Default;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SelectAudioTrack();
        UpdateTrackVolume();
    }

    private static float Approach(float cur, float target, float dt)
    {
        if (cur == target)
            return cur;

        if (cur > target)
        {
            cur -= dt;
            if (cur < target) cur = target;
        }
        else
        {
            cur += dt;
            if (cur > target) cur = target;
        }

        return cur;
    }

    void SetTrackVolume(AudioSource src, AudioTrack track)
    {
        float vol = (m_track == track) ? 1.0f : 0.0f;
        src.volume = Approach(src.volume, vol, Time.deltaTime);
    }

    void UpdateTrackVolume()
    {
        SetTrackVolume(m_defaultMusic, AudioTrack.Default);
        SetTrackVolume(m_environmentMusic, AudioTrack.Environment);
        SetTrackVolume(m_industryMusic, AudioTrack.Industry);
        SetTrackVolume(m_moneyMusic, AudioTrack.Money);
        SetTrackVolume(m_pollutionMusic, AudioTrack.Pollution);
    }

    void SelectAudioTrack()
    {
        var game = GameManager.Instance;
        if (game.Pollution > 50)
        {
            m_track = AudioTrack.Pollution;
            return;
        }

        if (game.Happiness > 30)
        {
            m_track = AudioTrack.Environment;
            return;
        }

        if (game.Pollution > 20 && game.Happiness > 10)
        {
            m_track = AudioTrack.Industry;
            return;
        }

        if (game.CurrentRound < 3 || game.PlayerMoney < 10000)
        {
            m_track = AudioTrack.Default;
        }
        else
        {
            // Lots of money but no impact.
            m_track = AudioTrack.Money;
        }
    }
}
