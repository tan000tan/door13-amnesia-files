using UnityEngine;
using Door13.Player;

namespace Door13.Audio
{
    /// <summary>
    /// 程序化音效：运行时合成，零音频资产。
    /// 环境低鸣（循环）/交互点击/循环提示音/开门吱呀/惊吓音（重击/金属/耳语）/脚步。
    /// 主音量经 GameSettings.masterVolume + AudioListener 控制。
    /// 分通道音量：musicVolume（环境音）、sfxVolume（交互音效）。
    /// </summary>
    public class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }

        AudioSource ambientSrc;
        AudioSource roomSrc;
        AudioSource fxSrc;
        AudioSource distSrc;          // 远处异响专用（低音量、随机间隔）

        [Range(0f, 1f)] public float ambientVolume = 0.25f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;    // 环境/音乐音量
        [Range(0f, 1f)] public float sfxVolume = 0.9f;      // 交互音效音量

        AudioClip roomHum, roomBuzz, roomMachine, roomDark;
        AudioClip distMetal, distThump, distWhisper;
        float nextDistant;

        void Awake()
        {
            Instance = this;

            var ambient = MakePad(12f);
            var click = MakeTone(660f, 0.05f, 0.3f, false);
            var stinger = MakeSweep(95f, 48f, 1.3f, 0.6f);
            var creak = MakeSweep(120f, 60f, 0.7f, 0.5f);
            var thump = MakeTone(50f, 0.35f, 0.9f, true);
            var metal = MakeSweep(600f, 220f, 0.2f, 0.55f);
            var whisper = MakeNoise(1.4f, 0.35f);
            var step = MakeTone(70f, 0.08f, 0.35f, true);

            ambientSrc = gameObject.AddComponent<AudioSource>();
            ambientSrc.clip = ambient;
            ambientSrc.loop = true;
            ambientSrc.volume = ambientVolume;
            ambientSrc.Play();

            // 房间环境音（按区域切换：默认/电流/机器/深处低语）
            roomHum = MakePad(12f);
            roomBuzz = MakeWind(10f);
            roomMachine = MakeMachine(8f);
            roomDark = MakePad(12f, 36f);

            roomSrc = gameObject.AddComponent<AudioSource>();
            roomSrc.loop = true;
            roomSrc.volume = 0f;
            roomSrc.spatialBlend = 0f;

            fxSrc = gameObject.AddComponent<AudioSource>();
            fxSrc.playOnAwake = false;

            // 远处异响源：建筑深处的呻吟/闷响/低语，随机间隔，制造"被注视感"
            distMetal = MakeSweep(170f, 55f, 2.2f, 0.32f);
            distThump = MakeTone(38f, 0.5f, 0.5f, true);
            distWhisper = MakeNoise(2.2f, 0.22f);
            distSrc = gameObject.AddComponent<AudioSource>();
            distSrc.playOnAwake = false;
            distSrc.spatialBlend = 0f;
            distSrc.volume = 0.5f;
            nextDistant = Random.Range(14f, 30f);

            _click = click; _stinger = stinger; _creak = creak;
            _thump = thump; _metal = metal; _whisper = whisper; _step = step;

            // 应用设置
            ApplySettings();
        }

        void ApplySettings()
        {
            musicVolume = GameSettings.Cur.musicVolume;
            sfxVolume = GameSettings.Cur.sfxVolume;
        }

        void Update()
        {
            // 监听设置变化
            if (musicVolume != GameSettings.Cur.musicVolume || sfxVolume != GameSettings.Cur.sfxVolume)
                ApplySettings();

            if (distSrc == null) return;
            nextDistant -= Time.unscaledDeltaTime;
            if (nextDistant <= 0f)
            {
                nextDistant = Random.Range(18f, 45f);
                int k = Random.Range(0, 3);
                if (k == 0) distSrc.PlayOneShot(distMetal, 0.55f * sfxVolume);
                else if (k == 1) distSrc.PlayOneShot(distThump, 0.7f * sfxVolume);
                else distSrc.PlayOneShot(distWhisper, 0.5f * sfxVolume);
            }
        }

        /// <summary>切换房间环境音：0默认低鸣 1电流 2机器 3深处</summary>
        public void SetRoomAmbient(int type)
        {
            if (roomSrc == null) return;
            AudioClip clip = type == 1 ? roomBuzz : type == 2 ? roomMachine
                : type == 3 ? roomDark : roomHum;
            if (roomSrc.clip == clip && roomSrc.isPlaying) return;
            roomSrc.clip = clip;
            roomSrc.volume = 0.15f * musicVolume;
            roomSrc.Play();
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        AudioClip _click, _stinger, _creak, _thump, _metal, _whisper, _step;

        public void Click() { if (fxSrc != null) fxSrc.PlayOneShot(_click, sfxVolume); }
        public void Stinger() { if (fxSrc != null) fxSrc.PlayOneShot(_stinger, sfxVolume); }
        public void DoorCreak() { if (fxSrc != null) fxSrc.PlayOneShot(_creak, sfxVolume); }

        /// <summary>脚步声：随机音高避免机械重复。</summary>
        public void Footstep()
        {
            if (fxSrc == null) return;
            fxSrc.pitch = Random.Range(0.92f, 1.08f);
            fxSrc.PlayOneShot(_step, 0.5f * sfxVolume);
            fxSrc.pitch = 1f;
        }

        /// <summary>按键数字音：随输入位数升高，触觉感更强。</summary>
        public void Blip(int index)
        {
            if (fxSrc == null) return;
            fxSrc.pitch = 1f + index * 0.06f;
            fxSrc.PlayOneShot(_click, 0.8f * sfxVolume);
            fxSrc.pitch = 1f;
        }

        /// <summary>错误蜂鸣：低频短促。</summary>
        public void Buzz()
        {
            if (fxSrc == null) return;
            fxSrc.pitch = 0.55f;
            fxSrc.PlayOneShot(_click, 0.9f * sfxVolume);
            fxSrc.pitch = 1f;
        }

        /// <summary>成功和弦：两连音。</summary>
        public void Chime()
        {
            if (fxSrc == null) return;
            StartCoroutine(ChimeCo());
        }

        System.Collections.IEnumerator ChimeCo()
        {
            fxSrc.pitch = 1.4f; fxSrc.PlayOneShot(_click, 0.7f * sfxVolume);
            yield return new WaitForSeconds(0.09f);
            fxSrc.pitch = 1.9f; fxSrc.PlayOneShot(_click, 0.7f * sfxVolume);
            fxSrc.pitch = 1f;
        }

        /// <summary>机械落锁音：谜题解开的厚重"咔哒"。</summary>
        public void Clunk()
        {
            if (fxSrc == null) return;
            fxSrc.pitch = 0.72f;
            fxSrc.PlayOneShot(_thump, 0.45f * sfxVolume);
            fxSrc.pitch = 1f;
        }

        /// <summary>纸张翻动：查看文件/便签时。</summary>
        public void Paper()
        {
            if (fxSrc == null) return;
            fxSrc.pitch = Random.Range(0.9f, 1.15f);
            fxSrc.PlayOneShot(_whisper, 0.35f * sfxVolume);
            fxSrc.pitch = 1f;
        }

        /// <summary>拾取音：柔和确认。</summary>
        public void Pickup()
        {
            if (fxSrc == null) return;
            fxSrc.pitch = 1.25f;
            fxSrc.PlayOneShot(_click, 0.55f * sfxVolume);
            fxSrc.pitch = 1f;
        }

        /// <summary>惊吓音：1重击 2金属 3耳语</summary>
        public void Scare(int type)
        {
            if (fxSrc == null) return;
            switch (type)
            {
                case 1: fxSrc.PlayOneShot(_thump, sfxVolume); break;
                case 2: fxSrc.PlayOneShot(_metal, sfxVolume); break;
                default: fxSrc.PlayOneShot(_whisper, sfxVolume); break;
            }
        }

        /// <summary>柔和氛围垫：低频三音和弦 + 慢呼吸LFO，12秒无缝循环。</summary>
        static AudioClip MakePad(float seconds, float baseFreq = 55f)
        {
            const int rate = 22050;
            int n = (int)(rate * seconds);
            var data = new float[n];
            float[] ratio = { 1f, 1.5f, 2f };
            float[] lfo = { 0.07f, 0.11f, 0.05f };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float seam = Mathf.SmoothStep(0f, 1f, Mathf.Min(t, seconds - t) * 1.5f);
                float v = 0f;
                for (int k = 0; k < 3; k++)
                    v += Mathf.Sin(2f * Mathf.PI * baseFreq * ratio[k] * t)
                       * (0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * lfo[k] * t));
                data[i] = v / 3f * 0.32f * seam;
            }
            return Build("Pad", data, rate);
        }

        /// <summary>通风系统风声：低通噪声+缓慢起伏。</summary>
        static AudioClip MakeWind(float seconds)
        {
            const int rate = 22050;
            int n = (int)(rate * seconds);
            var data = new float[n];
            float last = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                last = Mathf.Lerp(last, Random.value * 2f - 1f, 0.035f);
                float swell = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * t / seconds * 3f);
                float seam = Mathf.SmoothStep(0f, 1f, Mathf.Min(t, seconds - t) * 1.5f);
                data[i] = last * 0.55f * swell * seam;
            }
            return Build("Wind", data, rate);
        }

        /// <summary>电流嗡鸣：高频细抖</summary>
        static AudioClip MakeBuzz(float seconds)
        {
            const int rate = 22050;
            int n = (int)(rate * seconds);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float fade = Mathf.SmoothStep(0f, 1f, Mathf.Min(t, seconds - t) * 2f);
                float v = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 100f * t)) * 0.12f
                        + Mathf.Sin(2f * Mathf.PI * 50f * t) * 0.15f
                        + (Random.value * 2f - 1f) * 0.03f;
                data[i] = v * fade;
            }
            return Build("Buzz", data, rate);
        }

        /// <summary>机器低频脉动</summary>
        static AudioClip MakeMachine(float seconds)
        {
            const int rate = 22050;
            int n = (int)(rate * seconds);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float fade = Mathf.SmoothStep(0f, 1f, Mathf.Min(t, seconds - t) * 2f);
                float pulse = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 1.4f * t);
                float v = (Mathf.Sin(2f * Mathf.PI * 36f * t) * 0.5f + (Random.value * 2f - 1f) * 0.06f) * pulse;
                data[i] = v * 0.4f * fade;
            }
            return Build("Machine", data, rate);
        }

        static AudioClip MakeTone(float freq, float seconds, float vol, bool withNoise)
        {
            const int rate = 22050;
            int n = (int)(rate * seconds);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float env = Mathf.Min(1f, (seconds - t) * 4f);
                float v = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.8f;
                if (withNoise) v += (Random.value * 2f - 1f) * 0.2f;
                data[i] = v * vol * env;
            }
            return Build("Tone", data, rate);
        }

        static AudioClip MakeSweep(float from, float to, float seconds, float vol)
        {
            const int rate = 22050;
            int n = (int)(rate * seconds);
            var data = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float f = Mathf.Lerp(from, to, t / seconds);
                phase += 2f * Mathf.PI * f / rate;
                float env = Mathf.Min(1f, (seconds - t) * 3f) * Mathf.Min(1f, t * 20f);
                data[i] = Mathf.Sin(phase) * vol * env;
            }
            return Build("Sweep", data, rate);
        }

        static AudioClip MakeNoise(float seconds, float vol)
        {
            const int rate = 22050;
            int n = (int)(rate * seconds);
            var data = new float[n];
            float last = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                // 低通噪声，模拟耳语气流
                last = Mathf.Lerp(last, Random.value * 2f - 1f, 0.12f);
                float env = Mathf.Sin(Mathf.PI * t / seconds);
                data[i] = last * vol * env;
            }
            return Build("Noise", data, rate);
        }

        static AudioClip Build(string name, float[] data, int rate)
        {
            var clip = AudioClip.Create(name, data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
