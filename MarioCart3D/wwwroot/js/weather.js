// Mario Kart 3D - Weather Effects

window.gameWeather = {
    scene: null,
    rainSystem: null,
    snowSystem: null,
    currentWeather: 'clear',

    init: function (scene) {
        this.scene = scene;
    },

    setWeather: function (weatherType) {
        this.currentWeather = weatherType;
        this.clearWeather();

        if (weatherType === 'rain') {
            this.createRain();
            this.scene.fog.density = 0.02;
        } else if (weatherType === 'snow') {
            this.createSnow();
            this.scene.fog.density = 0.015;
        } else if (weatherType === 'fog') {
            this.scene.fog.density = 0.025;
        } else {
            this.scene.fog.density = 0.005;
        }
    },

    clearWeather: function () {
        if (this.rainSystem) {
            this.scene.remove(this.rainSystem);
            this.rainSystem.geometry.dispose();
            this.rainSystem.material.dispose();
            this.rainSystem = null;
        }
        if (this.snowSystem) {
            this.scene.remove(this.snowSystem);
            this.snowSystem.geometry.dispose();
            this.snowSystem.material.dispose();
            this.snowSystem = null;
        }
    },

    createRain: function () {
        const count = 2000;
        const geometry = new THREE.BufferGeometry();
        const positions = new Float32Array(count * 3);
        const velocities = new Float32Array(count);

        for (let i = 0; i < count; i++) {
            positions[i * 3] = (Math.random() - 0.5) * 200;
            positions[i * 3 + 1] = Math.random() * 80;
            positions[i * 3 + 2] = (Math.random() - 0.5) * 200;
            velocities[i] = 0.8 + Math.random() * 0.7;
        }

        geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));

        const material = new THREE.PointsMaterial({
            color: 0x88aaff,
            size: 0.3,
            transparent: true,
            opacity: 0.7
        });

        this.rainSystem = new THREE.Points(geometry, material);
        this.rainSystem.userData = { isWeather: true, velocities: velocities };
        this.scene.add(this.rainSystem);
    },

    createSnow: function () {
        const count = 1500;
        const geometry = new THREE.BufferGeometry();
        const positions = new Float32Array(count * 3);
        const velocities = new Float32Array(count);

        for (let i = 0; i < count; i++) {
            positions[i * 3] = (Math.random() - 0.5) * 200;
            positions[i * 3 + 1] = Math.random() * 80;
            positions[i * 3 + 2] = (Math.random() - 0.5) * 200;
            velocities[i] = 0.1 + Math.random() * 0.2;
        }

        geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));

        const material = new THREE.PointsMaterial({
            color: 0xffffff,
            size: 0.6,
            transparent: true,
            opacity: 0.9
        });

        this.snowSystem = new THREE.Points(geometry, material);
        this.snowSystem.userData = { isWeather: true, velocities: velocities };
        this.scene.add(this.snowSystem);
    },

    update: function () {
        const time = Date.now() * 0.001;

        if (this.rainSystem) {
            const positions = this.rainSystem.geometry.attributes.position.array;
            const velocities = this.rainSystem.userData.velocities;

            for (let i = 0; i < velocities.length; i++) {
                positions[i * 3 + 1] -= velocities[i];
                positions[i * 3] += Math.sin(time + i) * 0.02;
                if (positions[i * 3 + 1] < 0) {
                    positions[i * 3 + 1] = 80;
                    positions[i * 3] = (Math.random() - 0.5) * 200;
                    positions[i * 3 + 2] = (Math.random() - 0.5) * 200;
                }
            }
            this.rainSystem.geometry.attributes.position.needsUpdate = true;
        }

        if (this.snowSystem) {
            const positions = this.snowSystem.geometry.attributes.position.array;
            const velocities = this.snowSystem.userData.velocities;

            for (let i = 0; i < velocities.length; i++) {
                positions[i * 3 + 1] -= velocities[i];
                positions[i * 3] += Math.sin(time * 0.5 + i * 0.1) * 0.03;
                positions[i * 3 + 2] += Math.cos(time * 0.3 + i * 0.1) * 0.02;
                if (positions[i * 3 + 1] < 0) {
                    positions[i * 3 + 1] = 80;
                }
            }
            this.snowSystem.geometry.attributes.position.needsUpdate = true;
        }
    }
};
