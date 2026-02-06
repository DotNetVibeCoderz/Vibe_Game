
window.MinecraftClone = {
    scene: null,
    camera: null,
    renderer: null,
    controls: null,
    raycaster: null,
    objects: [], // Objects to collide with (chunks)
    moveForward: false,
    moveBackward: false,
    moveLeft: false,
    moveRight: false,
    canJump: false,
    prevTime: performance.now(),
    velocity: new THREE.Vector3(),
    direction: new THREE.Vector3(),
    voxels: new Map(), // Stores block data: "x,y,z" -> typeId
    cellSize: 16, // Chunk size
    textureAtlas: {},
    materials: [],
    selectedBlockType: 1, // Default selected block
    audioCtx: null,
    
    // --- Initialization ---
    init: function (containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        // 1. Setup Scene
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x87CEEB); // Sky blue
        this.scene.fog = new THREE.Fog(0x87CEEB, 10, 60);

        // 2. Setup Camera
        this.camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
        this.camera.position.y = 10;

        // 3. Setup Lights
        const light = new THREE.HemisphereLight(0xffffff, 0x777777, 0.75);
        light.position.set(0.5, 1, 0.75);
        this.scene.add(light);

        const dirLight = new THREE.DirectionalLight(0xffffff, 0.5);
        dirLight.position.set(50, 200, 100);
        dirLight.castShadow = true;
        this.scene.add(dirLight);

        // 4. Setup Renderer
        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        this.renderer.setPixelRatio(window.devicePixelRatio);
        this.renderer.setSize(window.innerWidth, window.innerHeight);
        container.appendChild(this.renderer.domElement);

        // 5. Generate Textures & Materials
        this.generateMaterials();

        // 6. Setup Controls
        this.controls = new THREE.PointerLockControls(this.camera, document.body);
        
        // Event Listeners for Controls
        const blocker = document.getElementById('ui-layer');
        const instructions = document.getElementById('instructions');

        instructions.addEventListener('click', () => {
            this.controls.lock();
        });

        this.controls.addEventListener('lock', () => {
            instructions.style.display = 'none';
            blocker.style.display = 'none';
        });

        this.controls.addEventListener('unlock', () => {
            blocker.style.display = 'block';
            instructions.style.display = '';
        });

        this.scene.add(this.controls.getObject());
        this.setupInputs();

        // 7. Raycaster for interaction
        this.raycaster = new THREE.Raycaster();

        // 8. Generate Initial World
        this.generateWorld();

        // 9. Resize Handler
        window.addEventListener('resize', () => this.onWindowResize(), false);

        // 10. Start Loop
        this.animate();
        
        // 11. Audio Setup
        this.initAudio();
        
        console.log("Minecraft Clone Initialized");
    },

    // --- Procedural Textures ---
    createCanvasTexture: function(color1, color2, noiseScale = 0.5) {
        const size = 64;
        const canvas = document.createElement('canvas');
        canvas.width = size;
        canvas.height = size;
        const ctx = canvas.getContext('2d');
        
        // Base color
        ctx.fillStyle = color1;
        ctx.fillRect(0, 0, size, size);
        
        // Noise
        for (let i = 0; i < 400; i++) {
            const x = Math.floor(Math.random() * size);
            const y = Math.floor(Math.random() * size);
            const w = Math.floor(Math.random() * 5) + 1;
            const h = Math.floor(Math.random() * 5) + 1;
            ctx.fillStyle = Math.random() > 0.5 ? color2 : color1;
            ctx.globalAlpha = Math.random() * noiseScale;
            ctx.fillRect(x, y, w, h);
        }
        
        const texture = new THREE.CanvasTexture(canvas);
        texture.magFilter = THREE.NearestFilter; // Minecraft style pixelation
        texture.minFilter = THREE.NearestFilter;
        return texture;
    },

    generateMaterials: function() {
        // Colors
        const c_grass = '#567d46';
        const c_dirt = '#5d4037';
        const c_stone = '#757575';
        const c_wood = '#5d4037'; // darker
        const c_leaves = '#2e7d32';

        const t_grass_top = this.createCanvasTexture('#6b8c42', '#567d46');
        const t_dirt = this.createCanvasTexture('#5d4037', '#4e342e');
        const t_stone = this.createCanvasTexture('#9e9e9e', '#757575');
        const t_wood = this.createCanvasTexture('#795548', '#5d4037');
        const t_leaves = this.createCanvasTexture('#4caf50', '#2e7d32');
        
        // Grass Block (Top is grass, sides are dirt-ish/grass-side, bottom dirt)
        // For simplicity in this clone, we use MultiMaterial
        // Order: right, left, top, bottom, front, back
        
        const mat_grass = [
            new THREE.MeshLambertMaterial({ map: t_dirt }), // Right
            new THREE.MeshLambertMaterial({ map: t_dirt }), // Left
            new THREE.MeshLambertMaterial({ map: t_grass_top }), // Top
            new THREE.MeshLambertMaterial({ map: t_dirt }), // Bottom
            new THREE.MeshLambertMaterial({ map: t_dirt }), // Front
            new THREE.MeshLambertMaterial({ map: t_dirt })  // Back
        ];

        const mat_dirt = new THREE.MeshLambertMaterial({ map: t_dirt });
        const mat_stone = new THREE.MeshLambertMaterial({ map: t_stone });
        const mat_wood = new THREE.MeshLambertMaterial({ map: t_wood });
        const mat_leaves = new THREE.MeshLambertMaterial({ map: t_leaves });
        
        // ID Mapping: 1=Grass, 2=Dirt, 3=Stone, 4=Wood, 5=Leaves
        this.materials = [
            null, // 0 is air
            mat_grass, // 1
            mat_dirt,  // 2
            mat_stone, // 3
            mat_wood,  // 4
            mat_leaves // 5
        ];
    },

    // --- Audio ---
    initAudio: function() {
        try {
            this.audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        } catch (e) {
            console.warn('AudioContext not supported');
        }
    },

    playSound: function(type) {
        if (!this.audioCtx) return;
        if (this.audioCtx.state === 'suspended') this.audioCtx.resume();

        const osc = this.audioCtx.createOscillator();
        const gainNode = this.audioCtx.createGain();

        osc.connect(gainNode);
        gainNode.connect(this.audioCtx.destination);

        if (type === 'place') {
            osc.type = 'sine';
            osc.frequency.setValueAtTime(600, this.audioCtx.currentTime);
            osc.frequency.exponentialRampToValueAtTime(300, this.audioCtx.currentTime + 0.1);
            gainNode.gain.setValueAtTime(0.5, this.audioCtx.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioCtx.currentTime + 0.1);
            osc.start();
            osc.stop(this.audioCtx.currentTime + 0.1);
        } else if (type === 'break') {
            osc.type = 'square';
            osc.frequency.setValueAtTime(200, this.audioCtx.currentTime);
            osc.frequency.exponentialRampToValueAtTime(100, this.audioCtx.currentTime + 0.1);
            gainNode.gain.setValueAtTime(0.5, this.audioCtx.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, this.audioCtx.currentTime + 0.1);
            osc.start();
            osc.stop(this.audioCtx.currentTime + 0.1);
        }
    },

    // --- Inputs ---
    setupInputs: function() {
        const onKeyDown = (event) => {
            switch (event.code) {
                case 'ArrowUp':
                case 'KeyW': this.moveForward = true; break;
                case 'ArrowLeft':
                case 'KeyA': this.moveLeft = true; break;
                case 'ArrowDown':
                case 'KeyS': this.moveBackward = true; break;
                case 'ArrowRight':
                case 'KeyD': this.moveRight = true; break;
                case 'Space': if (this.canJump) this.velocity.y += 18; this.canJump = false; break;
                case 'Digit1': this.selectedBlockType = 1; this.updateUI(1); break;
                case 'Digit2': this.selectedBlockType = 2; this.updateUI(2); break;
                case 'Digit3': this.selectedBlockType = 3; this.updateUI(3); break;
                case 'Digit4': this.selectedBlockType = 4; this.updateUI(4); break;
                case 'Digit5': this.selectedBlockType = 5; this.updateUI(5); break;
            }
        };

        const onKeyUp = (event) => {
            switch (event.code) {
                case 'ArrowUp':
                case 'KeyW': this.moveForward = false; break;
                case 'ArrowLeft':
                case 'KeyA': this.moveLeft = false; break;
                case 'ArrowDown':
                case 'KeyS': this.moveBackward = false; break;
                case 'ArrowRight':
                case 'KeyD': this.moveRight = false; break;
            }
        };

        const onMouseDown = (event) => {
            if (!this.controls.isLocked) return;
            
            // 0 = Left Click (Break), 2 = Right Click (Place)
            if (event.button === 0) this.interactWorld('break');
            if (event.button === 2) this.interactWorld('place');
        };

        document.addEventListener('keydown', onKeyDown);
        document.addEventListener('keyup', onKeyUp);
        document.addEventListener('mousedown', onMouseDown);
    },

    updateUI: function(id) {
        const slots = document.querySelectorAll('.slot');
        slots.forEach(s => s.classList.remove('active'));
        if(slots[id-1]) slots[id-1].classList.add('active');
    },

    // --- World Logic ---
    generateWorld: function() {
        const floorSize = 20; 
        
        // Simple flat world with some noise
        for (let x = -floorSize; x < floorSize; x++) {
            for (let z = -floorSize; z < floorSize; z++) {
                // Bedrock/Stone
                this.addBlock(x, 0, z, 3);
                
                // Random Terrain
                const height = Math.floor(Math.random() * 2) + 1;
                for (let y = 1; y <= height; y++) {
                   if (y === height) {
                       this.addBlock(x, y, z, 1); // Grass on top
                   } else {
                       this.addBlock(x, y, z, 2); // Dirt below
                   }
                }
                
                // Random Tree
                if (Math.random() > 0.98 && x > -15 && x < 15) {
                    this.createTree(x, height + 1, z);
                }
            }
        }
    },

    createTree: function(x, y, z) {
        // Trunk
        for (let i = 0; i < 4; i++) {
            this.addBlock(x, y + i, z, 4);
        }
        // Leaves
        for (let lx = x - 2; lx <= x + 2; lx++) {
            for (let ly = y + 2; ly <= y + 4; ly++) {
                for (let lz = z - 2; lz <= z + 2; lz++) {
                     // Don't replace wood
                     if (!this.voxels.has(`${lx},${ly},${lz}`)) {
                         this.addBlock(lx, ly, lz, 5);
                     }
                }
            }
        }
    },

    addBlock: function(x, y, z, typeId) {
        const key = `${x},${y},${z}`;
        if (this.voxels.has(key)) return; // Already exists

        this.voxels.set(key, typeId);

        const geometry = new THREE.BoxGeometry(1, 1, 1);
        const material = this.materials[typeId];
        
        const mesh = new THREE.Mesh(geometry, material);
        mesh.position.set(x, y, z);
        mesh.userData = { x:x, y:y, z:z }; // store grid coords
        
        this.scene.add(mesh);
        this.objects.push(mesh); // For collision/raycasting
    },

    removeBlock: function(mesh) {
        const key = `${mesh.userData.x},${mesh.userData.y},${mesh.userData.z}`;
        this.voxels.delete(key);
        
        this.scene.remove(mesh);
        
        const index = this.objects.indexOf(mesh);
        if (index > -1) {
            this.objects.splice(index, 1);
        }
        
        // Dispose memory
        if (mesh.geometry) mesh.geometry.dispose();
    },

    interactWorld: function(action) {
        this.raycaster.setFromCamera(new THREE.Vector2(0, 0), this.camera);
        const intersects = this.raycaster.intersectObjects(this.objects);

        if (intersects.length > 0) {
            const intersect = intersects[0];
            
            // Too far
            if (intersect.distance > 8) return;

            if (action === 'break') {
                this.removeBlock(intersect.object);
                this.playSound('break');
            } else if (action === 'place') {
                // Calculate position for new block based on face normal
                const x = intersect.object.userData.x + intersect.face.normal.x;
                const y = intersect.object.userData.y + intersect.face.normal.y;
                const z = intersect.object.userData.z + intersect.face.normal.z;
                
                // Prevent placing block inside the player (Self Collision check during placement)
                const playerPos = this.controls.getObject().position;
                // Simple box check
                if (x >= Math.floor(playerPos.x - 0.4) && x <= Math.floor(playerPos.x + 0.4) &&
                    z >= Math.floor(playerPos.z - 0.4) && z <= Math.floor(playerPos.z + 0.4) &&
                    y >= Math.floor(playerPos.y - 1.5) && y <= Math.floor(playerPos.y + 0.5)) {
                    return; // Occupied by player
                }

                this.addBlock(x, y, z, this.selectedBlockType);
                this.playSound('place');
            }
        }
    },
    
    // --- Physics & Collision ---
    checkCollision: function(pos) {
        // Player radius (approximated as a square box for performance)
        const r = 0.3; 
        
        // Player height checks (Feet and Head)
        // Adjust for player height. Camera is at top, so player is from y-1.6 to y
        const footY = Math.floor(pos.y - 1.5);
        const headY = Math.floor(pos.y - 0.5);

        // Check 4 corners around the player
        const corners = [
            {x: pos.x - r, z: pos.z - r},
            {x: pos.x + r, z: pos.z - r},
            {x: pos.x - r, z: pos.z + r},
            {x: pos.x + r, z: pos.z + r}
        ];

        for (let corner of corners) {
            const cx = Math.floor(corner.x);
            const cz = Math.floor(corner.z);
            
            // Check feet level
            if (this.voxels.has(`${cx},${footY},${cz}`)) return true;
            
            // Check head level (and slightly below head)
            if (this.voxels.has(`${cx},${headY},${cz}`)) return true;
            
            // Check mid-body just in case
            if (this.voxels.has(`${cx},${footY+1},${cz}`)) return true;
        }
        return false;
    },

    // --- Game Loop ---
    animate: function() {
        requestAnimationFrame(() => this.animate());

        const time = performance.now();
        if (this.controls.isLocked === true) {
            const delta = (time - this.prevTime) / 1000;

            // Deceleration
            this.velocity.x -= this.velocity.x * 10.0 * delta;
            this.velocity.z -= this.velocity.z * 10.0 * delta;
            this.velocity.y -= 9.8 * 10.0 * delta; // Gravity

            this.direction.z = Number(this.moveForward) - Number(this.moveBackward);
            this.direction.x = Number(this.moveRight) - Number(this.moveLeft);
            this.direction.normalize();

            if (this.moveForward || this.moveBackward) this.velocity.z -= this.direction.z * 400.0 * delta;
            if (this.moveLeft || this.moveRight) this.velocity.x -= this.direction.x * 400.0 * delta;

            const playerObj = this.controls.getObject();
            const originalPos = playerObj.position.clone();

            // 1. Apply X Movement and Check Collision
            this.controls.moveRight(-this.velocity.x * delta);
            if (this.checkCollision(playerObj.position)) {
                // If collision, revert X movement
                playerObj.position.x = originalPos.x;
                this.velocity.x = 0;
            }

            // 2. Apply Z Movement and Check Collision
            // Note: moveForward depends on camera direction, so we need to be careful.
            // But PointerLockControls calculates relative to view.
            // Since we updated X, we update 'originalPos' for Z check
            const posAfterX = playerObj.position.clone();
            
            this.controls.moveForward(-this.velocity.z * delta);
            if (this.checkCollision(playerObj.position)) {
                // If collision, revert Z movement
                playerObj.position.z = posAfterX.z;
                this.velocity.z = 0;
            }

            // 3. Apply Y (Gravity)
            playerObj.position.y += (this.velocity.y * delta);
            
            // Floor / Vertical Collision
            // We reuse checkCollision but we specifically care if we hit something *below*
            if (this.checkCollision(playerObj.position)) {
                 // If we are falling and hit something, it's the floor
                 if (this.velocity.y < 0) {
                     this.velocity.y = 0;
                     this.canJump = true;
                     
                     // Snap to top of block (simple approach)
                     // Since we hit something, we are slightly inside it. Push up.
                     playerObj.position.y = Math.ceil(playerObj.position.y - 1.5) + 1.5;
                 } else if (this.velocity.y > 0) {
                     // Hit head on ceiling
                     this.velocity.y = 0;
                     playerObj.position.y = Math.floor(playerObj.position.y);
                 }
            }

            // Hard floor at Y=2 (Bedrock level in our gen)
            if (playerObj.position.y < 2) {
                this.velocity.y = 0;
                playerObj.position.y = 2;
                this.canJump = true;
            }
        }

        this.prevTime = time;
        this.renderer.render(this.scene, this.camera);
    },

    onWindowResize: function() {
        this.camera.aspect = window.innerWidth / window.innerHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(window.innerWidth, window.innerHeight);
    }
};
