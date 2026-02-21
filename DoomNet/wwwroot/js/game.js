// DoomNet Game Engine - Three.js Implementation
// Author: Jacky the Code Bender
// Gravicode Studios

import * as THREE from 'three';
import { PointerLockControls } from 'three/addons/controls/PointerLockControls.js';

// --- Global Variables ---
let camera, scene, renderer, controls;
let moveForward = false;
let moveBackward = false;
let moveLeft = false;
let moveRight = false;
let canJump = false;
let isSprinting = false;
let isCrouching = false;

let prevTime = performance.now();
const velocity = new THREE.Vector3();
const direction = new THREE.Vector3();

// Game State
let gameState = {
    health: 100,
    ammo: 20,
    maxAmmo: 50,
    weapon: 'PISTOL',
    isPlaying: false,
    enemies: []
};

// Objects
const objects = []; // Collidable objects (walls, floor)
let raycaster;
let floorGeometry, floorMaterial;
let wallMaterial, ceilingMaterial;

// Weapons
const weapons = ['PISTOL', 'SHOTGUN', 'RIFLE'];
let currentWeaponIndex = 0;

// Initialization
init();
animate();

function init() {
    // 1. Scene Setup
    scene = new THREE.Scene();
    scene.background = new THREE.Color(0x111111); // Dark atmosphere
    scene.fog = new THREE.Fog(0x111111, 0, 750); // Fog for depth

    // 2. Camera
    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 1, 1000);
    camera.position.y = 10; // Player height

    // 3. Lighting (Dynamic)
    const light = new THREE.HemisphereLight(0xeeeeff, 0x777788, 0.75);
    light.position.set(0.5, 1, 0.75);
    scene.add(light);

    // Point lights for flickering effect (simulating torches/lamps)
    const pointLight = new THREE.PointLight(0xffaa00, 1, 100);
    pointLight.position.set(0, 20, 0);
    scene.add(pointLight);
    
    // Add some random flickering logic later in animate

    // 4. Controls (FPS Style)
    controls = new PointerLockControls(camera, document.body);

    // Menu interaction
    const menuScreen = document.getElementById('menu-screen');
    
    controls.addEventListener('lock', function () {
        menuScreen.classList.add('hidden');
        document.getElementById('ui-layer').classList.remove('hidden');
        document.getElementById('crosshair').classList.remove('hidden');
        gameState.isPlaying = true;
    });

    controls.addEventListener('unlock', function () {
        if(gameState.health > 0 && gameState.isPlaying) {
            // Pause menu could go here
            menuScreen.classList.remove('hidden');
            document.getElementById('ui-layer').classList.add('hidden');
            document.getElementById('crosshair').classList.add('hidden');
            gameState.isPlaying = false;
        }
    });

    scene.add(controls.getObject());

    // 5. Input Listeners
    const onKeyDown = function (event) {
        switch (event.code) {
            case 'ArrowUp':
            case 'KeyW':
                moveForward = true;
                break;
            case 'ArrowLeft':
            case 'KeyA':
                moveLeft = true;
                break;
            case 'ArrowDown':
            case 'KeyS':
                moveBackward = true;
                break;
            case 'ArrowRight':
            case 'KeyD':
                moveRight = true;
                break;
            case 'Space':
                if (canJump === true) velocity.y += 350; // Jump force
                canJump = false;
                break;
            case 'ShiftLeft':
                isSprinting = true;
                break;
            case 'ControlLeft':
                isCrouching = true;
                camera.position.y = 5; // Lower camera
                break;
            case 'Digit1':
                switchWeapon(0);
                break;
            case 'Digit2':
                switchWeapon(1);
                break;
            case 'Digit3':
                switchWeapon(2);
                break;
        }
    };

    const onKeyUp = function (event) {
        switch (event.code) {
            case 'ArrowUp':
            case 'KeyW':
                moveForward = false;
                break;
            case 'ArrowLeft':
            case 'KeyA':
                moveLeft = false;
                break;
            case 'ArrowDown':
            case 'KeyS':
                moveBackward = false;
                break;
            case 'ArrowRight':
            case 'KeyD':
                moveRight = false;
                break;
            case 'ShiftLeft':
                isSprinting = false;
                break;
            case 'ControlLeft':
                isCrouching = false;
                camera.position.y = 10; // Restore height
                break;
        }
    };

    document.addEventListener('keydown', onKeyDown);
    document.addEventListener('keyup', onKeyUp);
    
    // Shooting
    document.addEventListener('mousedown', function(event) {
        if(gameState.isPlaying && event.button === 0) { // Left click
            shoot();
        }
    });

    // 6. Raycaster for collision/shooting
    raycaster = new THREE.Raycaster(new THREE.Vector3(), new THREE.Vector3(0, - 1, 0), 0, 10);

    // 7. World Generation (Maze/Rooms)
    generateLevel();

    // 8. Renderer
    renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setPixelRatio(window.devicePixelRatio);
    renderer.setSize(window.innerWidth, window.innerHeight);
    renderer.shadowMap.enabled = true; // Enable shadows
    
    document.getElementById('game-container').appendChild(renderer.domElement);

    window.addEventListener('resize', onWindowResize);
}

function generateLevel() {
    // Simple procedural room generation for demo
    // Floor
    floorGeometry = new THREE.PlaneGeometry(2000, 2000, 100, 100);
    floorGeometry.rotateX(- Math.PI / 2);
    
    // Create texture pattern procedurally (checkerboard)
    const canvas = document.createElement('canvas');
    canvas.width = 128;
    canvas.height = 128;
    const context = canvas.getContext('2d');
    context.fillStyle = '#444';
    context.fillRect(0, 0, 128, 128);
    context.fillStyle = '#222';
    context.fillRect(0, 0, 64, 64);
    context.fillRect(64, 64, 64, 64);
    
    const floorTexture = new THREE.CanvasTexture(canvas);
    floorTexture.wrapS = THREE.RepeatWrapping;
    floorTexture.wrapT = THREE.RepeatWrapping;
    floorTexture.repeat.set(100, 100);
    
    floorMaterial = new THREE.MeshStandardMaterial({ map: floorTexture });
    const floor = new THREE.Mesh(floorGeometry, floorMaterial);
    scene.add(floor);

    // Ceiling
    const ceilingGeometry = new THREE.PlaneGeometry(2000, 2000, 100, 100);
    ceilingGeometry.rotateX(Math.PI / 2);
    ceilingGeometry.translate(0, 50, 0); // Height of room
    const ceilingMaterial = new THREE.MeshBasicMaterial({ color: 0x222222 });
    const ceiling = new THREE.Mesh(ceilingGeometry, ceilingMaterial);
    scene.add(ceiling);

    // Walls (Simple Maze)
    wallMaterial = new THREE.MeshStandardMaterial({ color: 0x883333 }); // Reddish brick-ish
    
    // Helper to create walls
    function createWall(x, z, width, depth) {
        const h = 50;
        const geometry = new THREE.BoxGeometry(width, h, depth);
        const wall = new THREE.Mesh(geometry, wallMaterial);
        wall.position.set(x, h/2, z);
        scene.add(wall);
        objects.push(wall); // Add to collidables
    }

    // Outer Boundary
    createWall(0, -500, 1000, 10);
    createWall(0, 500, 1000, 10);
    createWall(-500, 0, 10, 1000);
    createWall(500, 0, 10, 1000);

    // Inner obstacles
    createWall(-200, -200, 200, 20);
    createWall(200, 200, 20, 200);
    createWall(0, 0, 100, 100); // Central pillar
    
    // Add some "Enemies" (Simple Cubes for now)
    createEnemy(300, 300);
    createEnemy(-300, -300);
    createEnemy(300, -300);
}

function createEnemy(x, z) {
    const geo = new THREE.BoxGeometry(10, 20, 10);
    const mat = new THREE.MeshStandardMaterial({ color: 0x00ff00 }); // Green enemy
    const enemy = new THREE.Mesh(geo, mat);
    enemy.position.set(x, 10, z);
    enemy.userData = { type: 'enemy', health: 100, id: Math.random() };
    scene.add(enemy);
    gameState.enemies.push(enemy);
    objects.push(enemy);
}

function switchWeapon(index) {
    currentWeaponIndex = index;
    gameState.weapon = weapons[index];
    document.getElementById('weapon-name').innerText = gameState.weapon;
    
    // Simulate ammo change based on weapon
    if(gameState.weapon === 'SHOTGUN') gameState.ammo = 10;
    else if(gameState.weapon === 'RIFLE') gameState.ammo = 30;
    else gameState.ammo = 20;
    
    updateHUD();
}

function shoot() {
    if(gameState.ammo <= 0) {
        // Click sound (empty)
        return;
    }
    
    gameState.ammo--;
    updateHUD();
    
    // Muzzle flash effect (simple light pulse)
    const flash = new THREE.PointLight(0xffff00, 2, 20);
    flash.position.copy(camera.position);
    scene.add(flash);
    setTimeout(() => scene.remove(flash), 50);

    // Raycast shooting
    const shootRaycaster = new THREE.Raycaster();
    shootRaycaster.setFromCamera(new THREE.Vector2(0, 0), camera); // Center of screen
    
    const intersects = shootRaycaster.intersectObjects(gameState.enemies);
    
    if(intersects.length > 0) {
        const hitObject = intersects[0].object;
        if(hitObject.userData.type === 'enemy') {
            hitObject.userData.health -= 25; // Damage
            hitObject.material.color.setHex(0xff0000); // Flash red
            setTimeout(() => {
                if(hitObject.userData.health > 0) 
                    hitObject.material.color.setHex(0x00ff00);
            }, 100);
            
            if(hitObject.userData.health <= 0) {
                scene.remove(hitObject);
                gameState.enemies = gameState.enemies.filter(e => e !== hitObject);
                objects.splice(objects.indexOf(hitObject), 1);
            }
        }
    }
}

function updateHUD() {
    document.getElementById('health-display').innerText = `HEALTH: ${gameState.health}%`;
    document.getElementById('ammo-display').innerText = `AMMO: ${gameState.ammo}`;
    
    if(gameState.health <= 0) {
        gameOver();
    }
}

function gameOver() {
    gameState.isPlaying = false;
    controls.unlock();
    document.getElementById('gameover-screen').classList.remove('hidden');
    document.getElementById('ui-layer').classList.add('hidden');
    document.getElementById('crosshair').classList.add('hidden');
}

function takeDamage(amount) {
    gameState.health -= amount;
    updateHUD();
    // Screen red flash effect could be added here
}

function onWindowResize() {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
}

function animate() {
    requestAnimationFrame(animate);

    const time = performance.now();
    
    // Ensure game loop only runs when playing
    if (controls.isLocked === true) {
        const delta = (time - prevTime) / 1000;

        velocity.x -= velocity.x * 10.0 * delta;
        velocity.z -= velocity.z * 10.0 * delta;
        velocity.y -= 9.8 * 100.0 * delta; // Gravity

        direction.z = Number(moveForward) - Number(moveBackward);
        direction.x = Number(moveRight) - Number(moveLeft);
        direction.normalize(); // Ensure consistent speed in all directions

        const speed = isSprinting ? 1200.0 : 800.0;
        
        if (moveForward || moveBackward) velocity.z -= direction.z * speed * delta;
        if (moveLeft || moveRight) velocity.x -= direction.x * speed * delta;

        controls.moveRight(- velocity.x * delta);
        controls.moveForward(- velocity.z * delta);
        
        controls.getObject().position.y += (velocity.y * delta); // New behavior

        // Collision with floor
        if (controls.getObject().position.y < 10) {
            velocity.y = 0;
            controls.getObject().position.y = 10;
            canJump = true;
        }
        
        // Simple Enemy AI (Move towards player)
        gameState.enemies.forEach(enemy => {
            const dist = enemy.position.distanceTo(controls.getObject().position);
            if(dist > 5 && dist < 400) {
                enemy.lookAt(controls.getObject().position);
                enemy.translateZ(10 * delta); // Move speed
            }
            
            // Damage player if close
            if(dist < 15) {
                takeDamage(0.5); // Continuous damage
            }
        });
    }

    prevTime = time;
    renderer.render(scene, camera);
}

// --- Global Functions for HTML Buttons ---
window.startGame = function() {
    // Reset Game State
    gameState.health = 100;
    gameState.ammo = 20;
    gameState.weapon = 'PISTOL';
    currentWeaponIndex = 0;
    
    // Reset Position
    controls.getObject().position.set(0, 10, 0);
    velocity.set(0,0,0);
    
    updateHUD();
    document.getElementById('weapon-name').innerText = 'PISTOL';
    
    // Lock pointer
    controls.lock();

};

window.showOptions = function() {
    document.getElementById('options-screen').classList.remove('hidden');
    document.getElementById('menu-screen').classList.add('hidden');
};

window.closeOptions = function() {
    document.getElementById('options-screen').classList.add('hidden');
    document.getElementById('menu-screen').classList.remove('hidden');
};

window.showAbout = function() {
    document.getElementById('about-screen').classList.remove('hidden');
    document.getElementById('menu-screen').classList.add('hidden');
};

window.closeAbout = function() {
    document.getElementById('about-screen').classList.add('hidden');
    document.getElementById('menu-screen').classList.remove('hidden');
};

window.exitGame = function() {
    // In a real desktop app, this would close the window via IPC
    alert("Good Bye ;D");
    // Panggil method C# dengan nama class Razor page
    DotNet.invokeMethodAsync("DoomNet", "ExitApp").then(() => console.log("Close App.."));
    //window.close();
};

window.restartGame = function() {
    document.getElementById('gameover-screen').classList.add('hidden');
    startGame();
};

window.toMenu = function() {
    document.getElementById('gameover-screen').classList.add('hidden');
    document.getElementById('menu-screen').classList.remove('hidden');
};
