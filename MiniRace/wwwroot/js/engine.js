
window.miniRace = {
    scene: null,
    camera: null,
    renderer: null,
    player: { mesh: null, speed: 0, turnSpeed: 0.05, maxSpeed: 0.8, color: 0xff0000 },
    enemies: [],
    bullets: [],
    obstacles: [],
    track: null,
    animationId: null,
    isGameRunning: false,
    dotNetRef: null,
    audioCtx: null,
    gameData: { laps: 0, position: 1, level: 1 },

    // Init Engine
    init: function (dotNetReference) {
        this.dotNetRef = dotNetReference;
        console.log("MiniRace Engine Initialized");
    },

    setupScene: function () {
        const container = document.getElementById('game-container');
        if (!container) {
            console.error("Game container not found!");
            return;
        }

        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x87CEEB); // Sky blue
        this.scene.fog = new THREE.Fog(0x87CEEB, 10, 100);

        this.camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
        this.camera.position.set(0, 5, 10);
        this.camera.lookAt(0, 0, 0);

        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        this.renderer.setSize(window.innerWidth, window.innerHeight);
        this.renderer.shadowMap.enabled = true;
        
        container.innerHTML = '';
        container.appendChild(this.renderer.domElement);

        // Lighting
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
        this.scene.add(ambientLight);

        const dirLight = new THREE.DirectionalLight(0xffffff, 0.8);
        dirLight.position.set(10, 20, 10);
        dirLight.castShadow = true;
        this.scene.add(dirLight);

        // Resize handler
        window.addEventListener('resize', this.onResize.bind(this));
    },

    onResize: function() {
        if (this.camera && this.renderer) {
            this.camera.aspect = window.innerWidth / window.innerHeight;
            this.camera.updateProjectionMatrix();
            this.renderer.setSize(window.innerWidth, window.innerHeight);
        }
    },

    setupAudio: function() {
        try {
            if (!this.audioCtx) {
                this.audioCtx = new (window.AudioContext || window.webkitAudioContext)();
            }
        } catch(e) {
            console.log("Audio not supported");
        }
    },

    playSound: function(type) {
        if (!this.audioCtx) return;
        if (this.audioCtx.state === 'suspended') this.audioCtx.resume();

        const osc = this.audioCtx.createOscillator();
        const gain = this.audioCtx.createGain();
        osc.connect(gain);
        gain.connect(this.audioCtx.destination);

        if (type === 'shoot') {
            osc.type = 'square';
            osc.frequency.setValueAtTime(400, this.audioCtx.currentTime);
            osc.frequency.exponentialRampToValueAtTime(100, this.audioCtx.currentTime + 0.1);
            gain.gain.setValueAtTime(0.1, this.audioCtx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.01, this.audioCtx.currentTime + 0.1);
            osc.start();
            osc.stop(this.audioCtx.currentTime + 0.1);
        } else if (type === 'hit') {
            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(100, this.audioCtx.currentTime);
            gain.gain.setValueAtTime(0.2, this.audioCtx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.01, this.audioCtx.currentTime + 0.3);
            osc.start();
            osc.stop(this.audioCtx.currentTime + 0.3);
        } else if (type === 'start') {
            osc.type = 'sine';
            osc.frequency.setValueAtTime(200, this.audioCtx.currentTime);
            osc.frequency.linearRampToValueAtTime(600, this.audioCtx.currentTime + 0.5);
            gain.gain.setValueAtTime(0.1, this.audioCtx.currentTime);
            osc.start();
            osc.stop(this.audioCtx.currentTime + 0.5);
        }
    },

    startGame: function (colorHex, difficulty) {
        this.stopGame(); // Ensure clear previous state

        this.setupScene(); 
        this.setupAudio();

        this.createTrack();
        this.createPlayer(colorHex);
        this.createEnemies(difficulty);
        
        this.isGameRunning = true;
        this.gameData = { laps: 0, position: 1, level: Math.floor(Math.random() * 5) + 1 };
        
        this.animate();
        this.playSound('start');
    },

    stopGame: function() {
        this.isGameRunning = false;
        if (this.animationId) {
            cancelAnimationFrame(this.animationId);
            this.animationId = null;
        }
        
        // Clean up scene
        if (this.renderer && this.renderer.domElement) {
             const container = document.getElementById('game-container');
             if(container && container.contains(this.renderer.domElement)){
                 container.removeChild(this.renderer.domElement);
             }
        }
        
        window.removeEventListener('resize', this.onResize.bind(this));
    },

    createTrack: function () {
        // Ground
        const geometry = new THREE.PlaneGeometry(200, 200);
        const material = new THREE.MeshStandardMaterial({ color: 0x228B22 }); // Green grass
        const ground = new THREE.Mesh(geometry, material);
        ground.rotation.x = -Math.PI / 2;
        ground.receiveShadow = true;
        this.scene.add(ground);

        // Track Path (Visual - Grey Strip)
        const trackGeo = new THREE.RingGeometry(20, 35, 32);
        const trackMat = new THREE.MeshStandardMaterial({ color: 0x555555, side: THREE.DoubleSide });
        const track = new THREE.Mesh(trackGeo, trackMat);
        track.rotation.x = -Math.PI / 2;
        track.position.y = 0.05;
        track.receiveShadow = true;
        this.scene.add(track);
        this.track = track;

        // Random Obstacles
        this.obstacles = [];
        for (let i = 0; i < 10; i++) {
            const size = Math.random() * 2 + 1;
            const geo = new THREE.BoxGeometry(size, size, size);
            const mat = new THREE.MeshStandardMaterial({ color: 0x8B4513 });
            const mesh = new THREE.Mesh(geo, mat);
            
            // Random pos inside/outside track
            const angle = Math.random() * Math.PI * 2;
            const radius = Math.random() > 0.5 ? 15 : 40; 
            mesh.position.set(Math.cos(angle) * radius, size/2, Math.sin(angle) * radius);
            
            this.scene.add(mesh);
            this.obstacles.push(mesh);
        }
    },

    createPlayer: function (colorHex) {
        const geometry = new THREE.BoxGeometry(1.5, 0.8, 2.5);
        // Ensure hex string is parsed correctly
        let colorValue = colorHex;
        if (typeof colorHex === 'string') {
             // Three.js handles CSS strings like '#ff0000' automatically in recent versions,
             // but passing hex number is safe.
             // If passing integer directly, use it.
        }
        const material = new THREE.MeshStandardMaterial({ color: colorValue });
        this.player.mesh = new THREE.Mesh(geometry, material);
        this.player.mesh.position.set(27.5, 0.4, 0); // Start on track
        this.player.mesh.castShadow = true;
        
        this.addWheels(this.player.mesh);
        this.scene.add(this.player.mesh);
        this.player.speed = 0;
    },

    addWheels: function(carMesh) {
        const wheelGeo = new THREE.CylinderGeometry(0.4, 0.4, 0.3, 16);
        const wheelMat = new THREE.MeshStandardMaterial({ color: 0x111111 });
        const positions = [
            { x: 0.8, y: -0.2, z: 1 },
            { x: -0.8, y: -0.2, z: 1 },
            { x: 0.8, y: -0.2, z: -1 },
            { x: -0.8, y: -0.2, z: -1 }
        ];
        
        positions.forEach(pos => {
            const wheel = new THREE.Mesh(wheelGeo, wheelMat);
            wheel.rotation.z = Math.PI / 2;
            wheel.position.set(pos.x, pos.y, pos.z);
            carMesh.add(wheel);
        });
    },

    createEnemies: function (difficulty) {
        this.enemies = [];
        const count = difficulty === 'hard' ? 5 : (difficulty === 'medium' ? 3 : 1);
        const colors = [0xFFD700, 0xFF00FF, 0x00FFFF, 0xFFA500, 0x800080];

        for (let i = 0; i < count; i++) {
            const geometry = new THREE.BoxGeometry(1.5, 0.8, 2.5);
            const material = new THREE.MeshStandardMaterial({ color: colors[i % colors.length] });
            const mesh = new THREE.Mesh(geometry, material);
            
            const angle = (i + 1) * 0.2;
            mesh.position.set(Math.cos(angle) * 27.5, 0.4, Math.sin(angle) * 27.5);
            mesh.lookAt(Math.cos(angle + 0.1) * 27.5, 0.4, Math.sin(angle + 0.1) * 27.5);
            
            this.addWheels(mesh);
            this.scene.add(mesh);
            
            this.enemies.push({
                mesh: mesh,
                speed: 0.3 + (Math.random() * 0.2), 
                angle: angle
            });
        }
    },

    shoot: function () {
        if (!this.isGameRunning) return;
        this.playSound('shoot');
        
        const bulletGeo = new THREE.SphereGeometry(0.3, 8, 8);
        const bulletMat = new THREE.MeshBasicMaterial({ color: 0xFFFF00 });
        const bullet = new THREE.Mesh(bulletGeo, bulletMat);
        
        bullet.position.copy(this.player.mesh.position);
        bullet.quaternion.copy(this.player.mesh.quaternion);
        bullet.translateY(0.5);
        bullet.translateZ(-1.5);
        
        this.scene.add(bullet);
        this.bullets.push({ mesh: bullet, life: 100 });
    },

    keys: { w: false, a: false, s: false, d: false },
    handleInput: function (code, isDown) {
        if (code === 'KeyW' || code === 'ArrowUp') this.keys.w = isDown;
        if (code === 'KeyS' || code === 'ArrowDown') this.keys.s = isDown;
        if (code === 'KeyA' || code === 'ArrowLeft') this.keys.a = isDown;
        if (code === 'KeyD' || code === 'ArrowRight') this.keys.d = isDown;
        if (code === 'Space' && isDown) this.shoot();
    },

    animate: function () {
        if (!this.isGameRunning) return;
        
        this.animationId = requestAnimationFrame(this.animate.bind(this));

        // Player Movement
        if (this.keys.w) this.player.speed = Math.min(this.player.speed + 0.02, this.player.maxSpeed);
        else if (this.keys.s) this.player.speed = Math.max(this.player.speed - 0.02, -this.player.maxSpeed / 2);
        else this.player.speed *= 0.95;

        if (Math.abs(this.player.speed) > 0.01) {
            if (this.keys.a) this.player.mesh.rotation.y += this.player.turnSpeed;
            if (this.keys.d) this.player.mesh.rotation.y -= this.player.turnSpeed;
        }

        this.player.mesh.translateZ(-this.player.speed);

        // Camera Follow
        const relativeCameraOffset = new THREE.Vector3(0, 5, 10);
        const cameraOffset = relativeCameraOffset.applyMatrix4(this.player.mesh.matrixWorld);
        this.camera.position.lerp(cameraOffset, 0.1);
        this.camera.lookAt(this.player.mesh.position);

        // Enemy AI
        this.enemies.forEach(enemy => {
            if (!enemy.mesh.visible) return;

            enemy.angle += enemy.speed * 0.02;
            const radius = 27.5;
            
            const targetX = Math.cos(enemy.angle + 0.1) * radius;
            const targetZ = Math.sin(enemy.angle + 0.1) * radius;
            
            enemy.mesh.position.x = Math.cos(enemy.angle) * radius;
            enemy.mesh.position.z = Math.sin(enemy.angle) * radius;
            enemy.mesh.lookAt(targetX, 0.4, targetZ);
        });

        // Bullets
        for (let i = this.bullets.length - 1; i >= 0; i--) {
            const b = this.bullets[i];
            b.mesh.translateZ(-1.0);
            b.life--;

            let hit = false;
            this.enemies.forEach(enemy => {
                if (enemy.mesh.visible && b.mesh.position.distanceTo(enemy.mesh.position) < 2) {
                    enemy.mesh.visible = false;
                    hit = true;
                    this.playSound('hit');
                }
            });

            if (b.life <= 0 || hit) {
                this.scene.remove(b.mesh);
                this.bullets.splice(i, 1);
            }
        }

        this.renderer.render(this.scene, this.camera);
        
        // Callback
        if (Math.floor(Date.now() / 100) % 10 === 0 && this.dotNetRef) {
            try {
                const aliveEnemies = this.enemies.filter(e => e.mesh.visible).length;
                this.dotNetRef.invokeMethodAsync('UpdateGameState', {
                    speed: Math.round(this.player.speed * 100),
                    enemiesLeft: aliveEnemies
                });
            } catch(e) {
                // Ignore disposed object error
            }
        }
    }
};

window.addEventListener('keydown', (e) => {
    if(window.miniRace && window.miniRace.isGameRunning) window.miniRace.handleInput(e.code, true);
});

window.addEventListener('keyup', (e) => {
    if(window.miniRace && window.miniRace.isGameRunning) window.miniRace.handleInput(e.code, false);
});
