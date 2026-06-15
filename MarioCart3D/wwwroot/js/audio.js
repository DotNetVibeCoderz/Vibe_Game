// Mario Kart 3D - Audio Manager

window.gameAudio = {
    context: null,
    masterGain: null,
    musicGain: null,
    sfxGain: null,
    currentMusic: null,
    engineOscillator: null,
    engineGain: null,
    isMuted: false,
    initialized: false,

    init: function () {
        try {
            if (this.initialized) return true;

            const AudioContext = window.AudioContext || window.webkitAudioContext;
            this.context = new AudioContext();

            this.masterGain = this.context.createGain();
            this.masterGain.gain.value = 0.8;
            this.masterGain.connect(this.context.destination);

            this.musicGain = this.context.createGain();
            this.musicGain.gain.value = 0.5;
            this.musicGain.connect(this.masterGain);

            this.sfxGain = this.context.createGain();
            this.sfxGain.gain.value = 0.6;
            this.sfxGain.connect(this.masterGain);

            this.engineGain = this.context.createGain();
            this.engineGain.gain.value = 0;
            this.engineGain.connect(this.sfxGain);

            this.startEngineSound();
            this.initialized = true;
            console.log('Audio initialized');
            return true;
        } catch (e) {
            console.error('Audio init failed:', e);
            return false;
        }
    },

    ensureContext: function () {
        if (this.context && this.context.state === 'suspended') {
            this.context.resume();
        }
    },

    startEngineSound: function () {
        if (!this.context) return;
        if (this.engineOscillator) return;

        this.engineOscillator = this.context.createOscillator();
        this.engineOscillator.type = 'sawtooth';
        this.engineOscillator.frequency.value = 80;
        this.engineOscillator.connect(this.engineGain);
        this.engineOscillator.start();
    },

    updateEngineSound: function (speedRatio) {
        this.ensureContext();
        if (!this.engineOscillator || !this.engineGain) return;
        const targetFreq = 80 + speedRatio * 200;
        this.engineOscillator.frequency.setTargetAtTime(targetFreq, this.context.currentTime, 0.1);
        this.engineGain.gain.setTargetAtTime(speedRatio * 0.3, this.context.currentTime, 0.1);
    },

    playMusic: function (theme) {
        if (!this.context) return;
        this.ensureContext();
        this.stopMusic();

        const tempo = theme === 'space' ? 140 : 120;
        const baseFreq = theme === 'castle' ? 220 : theme === 'rainbow' ? 440 : 330;
        const waveType = theme === 'haunted' ? 'sawtooth' : 'square';

        this.currentMusic = {
            oscillator: this.context.createOscillator(),
            gain: this.context.createGain(),
            nextNoteTime: this.context.currentTime + 0.1,
            noteIndex: 0,
            tempo: tempo,
            baseFreq: baseFreq,
            waveType: waveType,
            notes: this.generateMelody(theme)
        };

        this.currentMusic.oscillator.type = waveType;
        this.currentMusic.oscillator.connect(this.currentMusic.gain);
        this.currentMusic.gain.connect(this.musicGain);
        this.currentMusic.gain.gain.value = 0.15;
        this.currentMusic.oscillator.start();

        this.scheduleMusic();
    },

    generateMelody: function (theme) {
        const melodies = {
            grassland: [0, 4, 7, 4, 0, 4, 7, 12, 7, 4, 0, -5, 0, 4, 7, 4],
            castle: [0, 3, 6, 3, 0, 3, 6, 9, 6, 3, 0, -2, 0, 3, 6, 3],
            space: [0, 4, 7, 11, 7, 4, 0, 12, 11, 7, 4, 0, -1, 4, 7, 11],
            beach: [0, 5, 9, 5, 0, 5, 9, 12, 9, 5, 0, -3, 0, 5, 9, 5],
            haunted: [0, 3, 6, 10, 6, 3, 0, -2, 0, 3, 6, 3, 0, -3, 0, 3],
            mountain: [0, 4, 7, 9, 7, 4, 0, -4, 0, 4, 7, 9, 7, 4, 0, -2]
        };
        return melodies[theme] || melodies.grassland;
    },

    scheduleMusic: function () {
        if (!this.currentMusic) return;

        const osc = this.currentMusic.oscillator;
        const noteDuration = 60 / this.currentMusic.tempo / 2;

        while (this.currentMusic.nextNoteTime < this.context.currentTime + 0.5) {
            const note = this.currentMusic.notes[this.currentMusic.noteIndex % this.currentMusic.notes.length];
            const freq = this.currentMusic.baseFreq * Math.pow(2, note / 12);

            osc.frequency.setValueAtTime(freq, this.currentMusic.nextNoteTime);

            this.currentMusic.nextNoteTime += noteDuration;
            this.currentMusic.noteIndex++;
        }

        this.currentMusic.timeoutId = setTimeout(() => this.scheduleMusic(), 200);
    },

    stopMusic: function () {
        if (this.currentMusic) {
            if (this.currentMusic.timeoutId) clearTimeout(this.currentMusic.timeoutId);
            try { this.currentMusic.oscillator.stop(); } catch { }
            this.currentMusic = null;
        }
    },

    playItemSound: function (itemName) {
        this.ensureContext();
        if (!this.context) return;

        const osc = this.context.createOscillator();
        const gain = this.context.createGain();
        osc.connect(gain);
        gain.connect(this.sfxGain);

        switch (itemName.toLowerCase()) {
            case 'green shell':
            case 'red shell':
                osc.type = 'square';
                osc.frequency.setValueAtTime(600, this.context.currentTime);
                osc.frequency.exponentialRampToValueAtTime(200, this.context.currentTime + 0.2);
                break;
            case 'star':
                osc.type = 'sine';
                osc.frequency.setValueAtTime(440, this.context.currentTime);
                osc.frequency.linearRampToValueAtTime(880, this.context.currentTime + 0.3);
                break;
            case 'bullet bill':
                osc.type = 'sawtooth';
                osc.frequency.setValueAtTime(200, this.context.currentTime);
                osc.frequency.exponentialRampToValueAtTime(800, this.context.currentTime + 0.5);
                break;
            default:
                osc.type = 'sine';
                osc.frequency.setValueAtTime(500, this.context.currentTime);
                osc.frequency.exponentialRampToValueAtTime(300, this.context.currentTime + 0.1);
                break;
        }

        gain.gain.setValueAtTime(0.3, this.context.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.01, this.context.currentTime + 0.3);

        osc.start();
        osc.stop(this.context.currentTime + 0.3);
    },

    playBoostSound: function () {
        this.ensureContext();
        if (!this.context) return;
        const osc = this.context.createOscillator();
        const gain = this.context.createGain();
        osc.type = 'sine';
        osc.connect(gain);
        gain.connect(this.sfxGain);
        osc.frequency.setValueAtTime(300, this.context.currentTime);
        osc.frequency.exponentialRampToValueAtTime(900, this.context.currentTime + 0.4);
        gain.gain.setValueAtTime(0.3, this.context.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.01, this.context.currentTime + 0.4);
        osc.start();
        osc.stop(this.context.currentTime + 0.4);
    },

    setMasterVolume: function (volume) {
        if (this.masterGain) this.masterGain.gain.value = volume;
    },

    setMusicVolume: function (volume) {
        if (this.musicGain) this.musicGain.gain.value = volume;
    },

    setSfxVolume: function (volume) {
        if (this.sfxGain) this.sfxGain.gain.value = volume;
    },

    setVolumes: function (master, music, sfx) {
        this.setMasterVolume(master);
        this.setMusicVolume(music);
        this.setSfxVolume(sfx);
    },

    mute: function () {
        this.isMuted = true;
        if (this.masterGain) this.masterGain.gain.value = 0;
    },

    unmute: function () {
        this.isMuted = false;
        if (this.masterGain) this.masterGain.gain.value = 0.8;
    }
};
