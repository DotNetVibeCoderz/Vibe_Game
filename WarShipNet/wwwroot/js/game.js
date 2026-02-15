// Global game instance
let game;
let gameConfig;

// Sound Manager using Web Audio API
const SoundManager = {
    audioCtx: null,
    
    init: function() {
        if (!this.audioCtx) {
            this.audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        }
    },

    playShoot: function() {
        if (!this.audioCtx) return;
        const oscillator = this.audioCtx.createOscillator();
        const gainNode = this.audioCtx.createGain();
        oscillator.connect(gainNode);
        gainNode.connect(this.audioCtx.destination);
        oscillator.type = 'square';
        oscillator.frequency.setValueAtTime(440, this.audioCtx.currentTime);
        oscillator.frequency.exponentialRampToValueAtTime(880, this.audioCtx.currentTime + 0.1);
        gainNode.gain.setValueAtTime(0.1, this.audioCtx.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioCtx.currentTime + 0.1);
        oscillator.start();
        oscillator.stop(this.audioCtx.currentTime + 0.1);
    },

    playExplosion: function() {
        if (!this.audioCtx) return;
        const oscillator = this.audioCtx.createOscillator();
        const gainNode = this.audioCtx.createGain();
        oscillator.connect(gainNode);
        gainNode.connect(this.audioCtx.destination);
        oscillator.type = 'sawtooth';
        oscillator.frequency.setValueAtTime(100, this.audioCtx.currentTime);
        oscillator.frequency.exponentialRampToValueAtTime(0.01, this.audioCtx.currentTime + 0.5);
        gainNode.gain.setValueAtTime(0.2, this.audioCtx.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioCtx.currentTime + 0.5);
        oscillator.start();
        oscillator.stop(this.audioCtx.currentTime + 0.5);
    },

    playPowerUp: function() {
        if (!this.audioCtx) return;
        const oscillator = this.audioCtx.createOscillator();
        const gainNode = this.audioCtx.createGain();
        oscillator.connect(gainNode);
        gainNode.connect(this.audioCtx.destination);
        oscillator.type = 'sine';
        oscillator.frequency.setValueAtTime(600, this.audioCtx.currentTime);
        oscillator.frequency.linearRampToValueAtTime(1200, this.audioCtx.currentTime + 0.2);
        gainNode.gain.setValueAtTime(0.1, this.audioCtx.currentTime);
        gainNode.gain.linearRampToValueAtTime(0.01, this.audioCtx.currentTime + 0.2);
        oscillator.start();
        oscillator.stop(this.audioCtx.currentTime + 0.2);
    }
};

class PlayScene extends Phaser.Scene {
    constructor() {
        super('PlayScene');
    }

    init(data) {
        this.difficulty = data.difficulty || 'medium';
        this.score = 0;
        this.level = 1;
        this.playerHealth = 100;
        this.isGameOver = false;
        
        // Difficulty modifiers
        this.spawnRate = 2000;
        this.enemySpeed = 100;
        
        if (this.difficulty === 'hard') {
            this.spawnRate = 1000;
            this.enemySpeed = 200;
        } else if (this.difficulty === 'beginner') {
            this.spawnRate = 3000;
            this.enemySpeed = 80;
        }
    }

    preload() {
        this.load.image('player', 'assets/player.png');
        this.load.image('enemy1', 'assets/enemy1.png');
        this.load.image('enemy2', 'assets/enemy2.png');
        this.load.image('boss', 'assets/boss.png');
        this.load.image('bullet', 'assets/bullet.png');
        this.load.image('explosion', 'assets/explosion.png');
    }

    create() {
        SoundManager.init();

        // Background (simple scrolling stars effect)
        this.cameras.main.setBackgroundColor('#000022');
        this.stars = this.add.group();
        for (let i = 0; i < 100; i++) {
            const x = Phaser.Math.Between(0, 800);
            const y = Phaser.Math.Between(0, 600);
            const star = this.add.circle(x, y, Phaser.Math.Between(1, 2), 0xffffff);
            this.stars.add(star);
        }

        // Player
        this.player = this.physics.add.sprite(400, 500, 'player');
        this.player.setCollideWorldBounds(true);
        this.player.setScale(0.8);

        // Groups
        this.bullets = this.physics.add.group();
        this.enemies = this.physics.add.group();
        this.bosses = this.physics.add.group();

        // Inputs
        this.cursors = this.input.keyboard.createCursorKeys();
        this.spaceBar = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE);
        this.lastFired = 0;

        // Spawner
        this.time.addEvent({
            delay: this.spawnRate,
            callback: this.spawnEnemy,
            callbackScope: this,
            loop: true
        });

        // Boss Timer
        this.time.addEvent({
            delay: 30000, // Boss every 30 seconds
            callback: this.spawnBoss,
            callbackScope: this,
            loop: true
        });

        // Collisions
        this.physics.add.overlap(this.bullets, this.enemies, this.hitEnemy, null, this);
        this.physics.add.overlap(this.bullets, this.bosses, this.hitBoss, null, this);
        this.physics.add.overlap(this.player, this.enemies, this.hitPlayer, null, this);
        this.physics.add.overlap(this.player, this.bosses, this.hitPlayer, null, this);

        // UI Update
        window.updateGameUI(this.score, this.playerHealth);
    }

    update(time, delta) {
        if (this.isGameOver) return;

        // Player Movement
        if (this.cursors.left.isDown) {
            this.player.setVelocityX(-300);
        } else if (this.cursors.right.isDown) {
            this.player.setVelocityX(300);
        } else {
            this.player.setVelocityX(0);
        }

        if (this.cursors.up.isDown) {
            this.player.setVelocityY(-300);
        } else if (this.cursors.down.isDown) {
            this.player.setVelocityY(300);
        } else {
            this.player.setVelocityY(0);
        }

        // Shooting
        if (this.spaceBar.isDown && time > this.lastFired) {
            this.fireBullet();
            this.lastFired = time + 200; // Fire rate
        }

        // Starfield Effect
        this.stars.children.iterate((star) => {
            star.y += 2;
            if (star.y > 600) star.y = 0;
        });

        // Cleanup Offscreen
        this.bullets.children.iterate((bullet) => {
            if (bullet && bullet.y < -10) bullet.destroy();
        });

        this.enemies.children.iterate((enemy) => {
            if (enemy && enemy.y > 610) enemy.destroy();
        });
    }

    fireBullet() {
        const bullet = this.bullets.create(this.player.x, this.player.y - 30, 'bullet');
        bullet.setVelocityY(-400);
        SoundManager.playShoot();
    }

    spawnEnemy() {
        const x = Phaser.Math.Between(50, 750);
        const type = Phaser.Math.Between(0, 1) === 0 ? 'enemy1' : 'enemy2';
        const enemy = this.enemies.create(x, -50, type);
        enemy.setVelocityY(this.enemySpeed + Phaser.Math.Between(-20, 20));
        
        if (type === 'enemy2') {
            // Sine wave movement for enemy 2
            this.tweens.add({
                targets: enemy,
                x: x + 100,
                duration: 2000,
                ease: 'Sine.easeInOut',
                yoyo: true,
                repeat: -1
            });
        }
    }

    spawnBoss() {
        if (this.bosses.countActive() > 0) return; // Only one boss at a time

        const boss = this.bosses.create(400, -100, 'boss');
        boss.setVelocityY(50);
        boss.setData('health', 20 * (this.level));
        
        this.tweens.add({
            targets: boss,
            y: 150,
            duration: 2000,
            ease: 'Power2',
            onComplete: () => {
                boss.setVelocityY(0);
                // Boss movement pattern
                this.tweens.add({
                    targets: boss,
                    x: 600,
                    duration: 3000,
                    ease: 'Sine.easeInOut',
                    yoyo: true,
                    repeat: -1
                });
            }
        });
    }

    hitEnemy(bullet, enemy) {
        bullet.destroy();
        
        // Create explosion effect (simple particle or just destroy)
        this.add.sprite(enemy.x, enemy.y, 'explosion').play('explode').destroy(); 
        // Note: animation not set up, just using sprite for now as placeholder
        
        enemy.destroy();
        this.score += 10;
        SoundManager.playExplosion();
        window.updateGameUI(this.score, this.playerHealth);
    }

    hitBoss(bullet, boss) {
        bullet.destroy();
        let hp = boss.getData('health');
        hp--;
        boss.setData('health', hp);
        
        if (hp <= 0) {
            boss.destroy();
            this.score += 500;
            this.level++;
            SoundManager.playExplosion();
            // Increase difficulty slightly
            this.spawnRate = Math.max(500, this.spawnRate - 200);
        } else {
            // Flash red
            boss.setTint(0xff0000);
            this.time.delayedCall(100, () => boss.clearTint());
        }
        window.updateGameUI(this.score, this.playerHealth);
    }

    hitPlayer(player, enemy) {
        enemy.destroy();
        this.playerHealth -= 10;
        SoundManager.playExplosion();
        
        this.cameras.main.shake(200, 0.01);
        
        window.updateGameUI(this.score, this.playerHealth);

        if (this.playerHealth <= 0) {
            this.physics.pause();
            player.setTint(0xff0000);
            this.isGameOver = true;
            window.endGame(this.score);
        }
    }
}

// Window functions for Blazor Interop

window.initGame = (containerId) => {
    // Destroy previous instance if exists
    if (game) {
        game.destroy(true);
    }
};

window.startGame = (difficulty) => {
    if (game) {
        game.destroy(true);
    }

    const config = {
        type: Phaser.AUTO,
        parent: 'game-container',
        width: 800,
        height: 600,
        physics: {
            default: 'arcade',
            arcade: {
                gravity: { y: 0 },
                debug: false
            }
        },
        scene: [PlayScene],
        transparent: true
    };

    game = new Phaser.Game(config);
    game.scene.start('PlayScene', { difficulty: difficulty });
};

window.updateGameUI = (score, health) => {
    // This function will be defined in Blazor or we update DOM directly
    const scoreEl = document.getElementById('score-val');
    const healthEl = document.getElementById('health-val');
    if(scoreEl) scoreEl.innerText = score;
    if(healthEl) healthEl.innerText = health;
};

window.endGame = (score) => {
    // Call Blazor method
    DotNet.invokeMethodAsync('WarShipNet', 'GameOver', score);
};
