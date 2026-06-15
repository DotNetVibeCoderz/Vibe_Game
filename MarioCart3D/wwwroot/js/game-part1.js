// Mario Kart 3D - Three.js Game Engine Part 1

window.gameEngine = {
    scene: null,
    camera: null,
    renderer: null,
    kart: null,
    aiKarts: [],
    track: null,
    animationId: null,
    isPlaying: false,
    keys: {},
    kartAngle: 0,
    kartSpeed: 0,
    maxSpeed: 0.9,
    acceleration: 0.025,
    friction: 0.012,
    turnSpeed: 0.045,
    boostSpeed: 0,
    isDrifting: false,
    driftTime: 0,
    lastTime: 0,
    currentTheme: 'grassland',

    init: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('Canvas not found:', canvasId);
            return false;
        }

        if (typeof THREE === 'undefined') {
            console.error('Three.js is not loaded');
            return false;
        }

        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x87CEEB);
        this.scene.fog = new THREE.Fog(0x87CEEB, 20, 250);

        const width = canvas.clientWidth || canvas.parentElement.clientWidth || window.innerWidth;
        const height = canvas.clientHeight || canvas.parentElement.clientHeight || window.innerHeight;

        this.camera = new THREE.PerspectiveCamera(60, width / height, 0.1, 1000);
        this.camera.position.set(0, 8, 15);
        this.camera.lookAt(0, 0, 0);

        this.renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true, alpha: false });
        this.renderer.setSize(width, height);
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;

        const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
        this.scene.add(ambientLight);

        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
        directionalLight.position.set(50, 100, 50);
        directionalLight.castShadow = true;
        this.scene.add(directionalLight);

        this.createKart();
        this.createTrack('mushroom');
        this.setupInput();

        if (typeof window.gameWeather !== 'undefined') {
            window.gameWeather.init(this.scene);
        }

        if (typeof window.gameAudio !== 'undefined') {
            window.gameAudio.init();
        }

        window.addEventListener('resize', () => this.onResize(canvas));

        console.log('Game engine initialized successfully');
        return true;
    },

    setupInput: function () {
        document.addEventListener('keydown', (e) => { this.keys[e.key] = true; });
        document.addEventListener('keyup', (e) => { this.keys[e.key] = false; });
    },

    createKart: function (color) {
        const kartGroup = new THREE.Group();

        const bodyColor = color || 0xe94560;
        const body = new THREE.Mesh(
            new THREE.BoxGeometry(2, 0.8, 3),
            new THREE.MeshStandardMaterial({ color: bodyColor })
        );
        body.position.y = 0.8;
        body.castShadow = true;
        kartGroup.add(body);
        kartGroup.userData.body = body;

        const spoiler = new THREE.Mesh(
            new THREE.BoxGeometry(1.8, 0.2, 0.5),
            new THREE.MeshStandardMaterial({ color: bodyColor })
        );
        spoiler.position.set(0, 1.6, -1.4);
        spoiler.castShadow = true;
        kartGroup.add(spoiler);

        const spoilerLeft = new THREE.Mesh(
            new THREE.BoxGeometry(0.1, 0.6, 0.2),
            new THREE.MeshStandardMaterial({ color: 0x333333 })
        );
        spoilerLeft.position.set(-0.7, 1.3, -1.4);
        kartGroup.add(spoilerLeft);

        const spoilerRight = new THREE.Mesh(
            new THREE.BoxGeometry(0.1, 0.6, 0.2),
            new THREE.MeshStandardMaterial({ color: 0x333333 })
        );
        spoilerRight.position.set(0.7, 1.3, -1.4);
        kartGroup.add(spoilerRight);

        const seat = new THREE.Mesh(
            new THREE.BoxGeometry(1.5, 0.5, 1),
            new THREE.MeshStandardMaterial({ color: 0x333333 })
        );
        seat.position.set(0, 1.3, -0.5);
        seat.castShadow = true;
        kartGroup.add(seat);

        const wheelGeometry = new THREE.CylinderGeometry(0.4, 0.4, 0.3, 16);
        const wheelMaterial = new THREE.MeshStandardMaterial({ color: 0x111111 });
        const positions = [[-1.1, 0.4, 1], [1.1, 0.4, 1], [-1.1, 0.4, -1], [1.1, 0.4, -1]];
        positions.forEach(pos => {
            const wheel = new THREE.Mesh(wheelGeometry, wheelMaterial);
            wheel.rotation.z = Math.PI / 2;
            wheel.position.set(...pos);
            wheel.castShadow = true;
            kartGroup.add(wheel);
        });

        const head = new THREE.Mesh(
            new THREE.SphereGeometry(0.5, 16, 16),
            new THREE.MeshStandardMaterial({ color: 0xffcc99 })
        );
        head.position.set(0, 2, -0.3);
        head.castShadow = true;
        kartGroup.add(head);

        const hat = new THREE.Mesh(
            new THREE.ConeGeometry(0.55, 0.6, 16),
            new THREE.MeshStandardMaterial({ color: 0xff0000 })
        );
        hat.position.set(0, 2.5, -0.3);
        hat.castShadow = true;
        kartGroup.add(hat);

        const headlightGeo = new THREE.CylinderGeometry(0.15, 0.15, 0.1, 16);
        const headlightMat = new THREE.MeshStandardMaterial({ color: 0xffffcc, emissive: 0xffffcc, emissiveIntensity: 0.5 });
        const leftLight = new THREE.Mesh(headlightGeo, headlightMat);
        leftLight.rotation.x = Math.PI / 2;
        leftLight.position.set(-0.6, 0.9, 1.5);
        kartGroup.add(leftLight);

        const rightLight = new THREE.Mesh(headlightGeo, headlightMat);
        rightLight.rotation.x = Math.PI / 2;
        rightLight.position.set(0.6, 0.9, 1.5);
        kartGroup.add(rightLight);

        return kartGroup;
    },

    setPlayerKart: function (color) {
        if (this.kart) this.scene.remove(this.kart);
        this.kart = this.createKart(color);
        this.scene.add(this.kart);
    },

    createAiKarts: function (aiData) {
        this.aiKarts.forEach(k => this.scene.remove(k));
        this.aiKarts = [];

        const colors = [0x2ecc71, 0xff69b4, 0xf39c12, 0x9acd32, 0x3498db, 0x9b59b6, 0x1abc9c];

        aiData.forEach((ai, index) => {
            const kart = this.createKart(colors[index % colors.length]);
            kart.position.set(ai.x, 0, ai.z);
            kart.userData.aiId = ai.id;
            this.scene.add(kart);
            this.aiKarts.push(kart);
        });
    },

    updateAiKarts: function (aiData) {
        aiData.forEach(ai => {
            const kart = this.aiKarts.find(k => k.userData.aiId === ai.id);
            if (kart) {
                kart.position.set(ai.x, ai.y || 0, ai.z);
                kart.rotation.y = ai.rotationY;
            }
        });
    }
};
