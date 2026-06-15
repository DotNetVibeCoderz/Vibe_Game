// Mario Kart 3D - Three.js Game Engine Part 3

window.gameEngine.start = function () {
    if (this.isPlaying) return;
    this.isPlaying = true;
    this.lastTime = performance.now();

    if (typeof window.gameAudio !== 'undefined') {
        window.gameAudio.ensureContext();
    }

    this.animate();
};

window.gameEngine.stop = function () {
    this.isPlaying = false;
    if (this.animationId) {
        cancelAnimationFrame(this.animationId);
        this.animationId = null;
    }
    if (typeof window.gameAudio !== 'undefined') {
        window.gameAudio.stopMusic();
    }
};

window.gameEngine.updateKartPhysics = function (deltaTime) {
    if (!this.kart) return;

    const max = this.maxSpeed + this.boostSpeed;

    if (this.keys['ArrowUp'] || this.keys['w'] || this.keys['W']) {
        this.kartSpeed = Math.min(this.kartSpeed + this.acceleration, max);
    } else if (this.keys['ArrowDown'] || this.keys['s'] || this.keys['S']) {
        this.kartSpeed = Math.max(this.kartSpeed - this.acceleration, -this.maxSpeed * 0.5);
    } else {
        if (this.kartSpeed > 0) this.kartSpeed = Math.max(0, this.kartSpeed - this.friction);
        if (this.kartSpeed < 0) this.kartSpeed = Math.min(0, this.kartSpeed + this.friction);
    }

    const isTurning = this.keys['ArrowLeft'] || this.keys['a'] || this.keys['A'] ||
                      this.keys['ArrowRight'] || this.keys['d'] || this.keys['D'];
    const driftKey = this.keys['Shift'] || this.keys[' '];

    if (Math.abs(this.kartSpeed) > 0.3 && isTurning && driftKey) {
        this.isDrifting = true;
        this.driftTime += deltaTime;
        this.turnSpeed = 0.06;
    } else {
        if (this.isDrifting && this.driftTime > 0.8) {
            this.boostSpeed = 0.4;
            this.createParticleExplosion(this.kart.position.x, 0.5, this.kart.position.z, 0xff6600, 20);
            if (typeof window.gameAudio !== 'undefined') window.gameAudio.playBoostSound();
        }
        this.isDrifting = false;
        this.driftTime = 0;
        this.turnSpeed = 0.045;
    }

    if (this.boostSpeed > 0) {
        this.boostSpeed = Math.max(0, this.boostSpeed - deltaTime * 0.8);
    }

    if (Math.abs(this.kartSpeed) > 0.01) {
        if (this.keys['ArrowLeft'] || this.keys['a'] || this.keys['A']) {
            this.kartAngle += this.turnSpeed * Math.sign(this.kartSpeed);
        }
        if (this.keys['ArrowRight'] || this.keys['d'] || this.keys['D']) {
            this.kartAngle -= this.turnSpeed * Math.sign(this.kartSpeed);
        }
    }

    this.kart.position.x += Math.cos(this.kartAngle) * this.kartSpeed;
    this.kart.position.z -= Math.sin(this.kartAngle) * this.kartSpeed;
    this.kart.rotation.y = this.kartAngle + Math.PI / 2;

    if (typeof window.gameAudio !== 'undefined') {
        window.gameAudio.updateEngineSound(Math.abs(this.kartSpeed) / this.maxSpeed);
    }

    const dist = Math.sqrt(this.kart.position.x * this.kart.position.x + this.kart.position.z * this.kart.position.z);
    if (dist > 80) {
        const angle = Math.atan2(this.kart.position.z, this.kart.position.x);
        this.kart.position.x = Math.cos(angle) * 80;
        this.kart.position.z = Math.sin(angle) * 80;
        this.kartSpeed *= 0.5;
    }

    if (this.track) {
        this.track.children.forEach(child => {
            if (child.userData.isBoostPad) {
                const dx = this.kart.position.x - child.position.x;
                const dz = this.kart.position.z - child.position.z;
                if (Math.sqrt(dx * dx + dz * dz) < 3) {
                    this.boostSpeed = 0.5;
                    this.createParticleExplosion(this.kart.position.x, 0.5, this.kart.position.z, 0x00ff88, 15);
                    if (typeof window.gameAudio !== 'undefined') window.gameAudio.playBoostSound();
                }
            }
            if (child.userData.isItemBox) {
                const dx = this.kart.position.x - child.position.x;
                const dz = this.kart.position.z - child.position.z;
                if (Math.sqrt(dx * dx + dz * dz) < 2 && child.position.y > -50) {
                    window.dispatchEvent(new CustomEvent('itemBoxHit', { detail: { x: child.position.x, z: child.position.z } }));
                    child.position.y = -100;
                }
            }
        });
    }
};

window.gameEngine.updateCamera = function () {
    if (!this.kart || !this.camera) return;
    const offsetX = Math.cos(this.kartAngle) * 15;
    const offsetZ = -Math.sin(this.kartAngle) * 15;

    const targetX = this.kart.position.x - offsetX;
    const targetZ = this.kart.position.z - offsetZ;

    this.camera.position.x += (targetX - this.camera.position.x) * 0.1;
    this.camera.position.z += (targetZ - this.camera.position.z) * 0.1;
    this.camera.position.y += (8 - this.camera.position.y) * 0.1;
    this.camera.lookAt(this.kart.position);
};

window.gameEngine.updateParticles = function (deltaTime) {
    const toRemove = [];
    this.scene.children.forEach(child => {
        if (child.userData.isParticles) {
            child.children.forEach(p => {
                p.position.add(p.userData.velocity);
                p.userData.velocity.y -= deltaTime * 0.5;
                p.userData.life -= deltaTime;
                p.scale.setScalar(Math.max(0, p.userData.life));
            });
            child.userData.life = (child.userData.life || 1) - deltaTime;
            if (child.userData.life <= 0) {
                toRemove.push(child);
            }
        }
    });

    toRemove.forEach(p => this.scene.remove(p));
};

window.gameEngine.animate = function () {
    if (!this.isPlaying) return;
    this.animationId = requestAnimationFrame(() => this.animate());

    const now = performance.now();
    const deltaTime = Math.min((now - this.lastTime) / 1000, 0.1);
    this.lastTime = now;

    const time = now * 0.001;

    this.updateKartPhysics(deltaTime);
    this.updateCamera();
    this.updateParticles(deltaTime);

    if (typeof window.gameWeather !== 'undefined') {
        window.gameWeather.update();
    }

    if (this.track) {
        this.track.children.forEach(child => {
            if (child.userData && child.userData.speed) {
                child.rotation.y += 0.02 * child.userData.speed;
                child.rotation.x += 0.01 * child.userData.speed;
                child.position.y = 1 + Math.sin(time * 2 + child.userData.angle) * 0.3;
            }
        });
    }

    this.renderer.render(this.scene, this.camera);
};

window.gameEngine.onResize = function (canvas) {
    if (!this.camera || !this.renderer) return;
    const width = canvas.clientWidth || canvas.parentElement.clientWidth || window.innerWidth;
    const height = canvas.clientHeight || canvas.parentElement.clientHeight || window.innerHeight;
    this.camera.aspect = width / height;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(width, height);
};

window.gameEngine.changeTrack = function (trackName) {
    this.createTrack(trackName);
};

window.gameEngine.changeKartColor = function (colorHex) {
    if (this.kart && this.kart.userData.body) {
        this.kart.userData.body.material.color.setHex(colorHex);
    }
};

window.gameEngine.setWeather = function (weatherType) {
    if (typeof window.gameWeather !== 'undefined') {
        window.gameWeather.setWeather(weatherType);
    }
};

window.gameEngine.playMusic = function (theme) {
    if (typeof window.gameAudio !== 'undefined') {
        window.gameAudio.init();
        window.gameAudio.playMusic(theme);
    }
};

window.gameEngine.setVolume = function (master, music, sfx) {
    if (typeof window.gameAudio !== 'undefined') {
        window.gameAudio.setVolumes(master, music, sfx);
    }
};

window.gameEngine.getPlayerPosition = function () {
    if (!this.kart) return { x: 0, z: 0 };
    return { x: this.kart.position.x, z: this.kart.position.z };
};

console.log('Mario Kart 3D game engine loaded');
