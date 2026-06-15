// Mario Kart 3D - Three.js Game Engine Part 2

window.gameEngine.createTrack = function (trackName) {
    if (this.track) this.scene.remove(this.track);

    const group = new THREE.Group();
    const theme = this.getTrackTheme(trackName);
    this.currentTheme = theme.name;

    this.scene.background = new THREE.Color(theme.skyColor);
    this.scene.fog.color = new THREE.Color(theme.skyColor);

    const ground = new THREE.Mesh(
        new THREE.PlaneGeometry(300, 300),
        new THREE.MeshStandardMaterial({ color: theme.groundColor })
    );
    ground.rotation.x = -Math.PI / 2;
    ground.position.y = -0.1;
    ground.receiveShadow = true;
    group.add(ground);

    const points = this.generateTrackPath(theme.radiusX, theme.radiusZ);

    const roadGeo = new THREE.BufferGeometry();
    const vertices = [];
    const width = 8;

    for (let i = 0; i < points.length; i++) {
        const current = points[i];
        const next = points[(i + 1) % points.length];
        const tangent = new THREE.Vector2(next.x - current.x, next.y - current.y).normalize();
        const normal = new THREE.Vector2(-tangent.y, tangent.x);
        const elevation = current.z * 0.15;
        vertices.push(
            current.x + normal.x * width, elevation, current.y + normal.y * width,
            current.x - normal.x * width, elevation, current.y - normal.y * width
        );
    }

    roadGeo.setAttribute('position', new THREE.Float32BufferAttribute(vertices, 3));
    roadGeo.computeVertexNormals();

    const road = new THREE.Mesh(roadGeo, new THREE.MeshStandardMaterial({ color: theme.roadColor, side: THREE.DoubleSide }));
    road.receiveShadow = true;
    road.castShadow = true;
    group.add(road);

    // Road markings
    const markingGeo = new THREE.BufferGeometry();
    const markingVertices = [];
    for (let i = 0; i < points.length; i++) {
        const current = points[i];
        const next = points[(i + 1) % points.length];
        const tangent = new THREE.Vector2(next.x - current.x, next.y - current.y).normalize();
        const normal = new THREE.Vector2(-tangent.y, tangent.x);
        const elevation = current.z * 0.15 + 0.02;
        markingVertices.push(
            current.x + normal.x * 0.5, elevation, current.y + normal.y * 0.5,
            current.x - normal.x * 0.5, elevation, current.y - normal.y * 0.5
        );
    }
    markingGeo.setAttribute('position', new THREE.Float32BufferAttribute(markingVertices, 3));
    const markings = new THREE.Mesh(markingGeo, new THREE.MeshBasicMaterial({ color: 0xffffff, side: THREE.DoubleSide }));
    group.add(markings);

    // Add elevated shortcut
    this.createShortcut(group, theme);

    // Boost pads
    for (let i = 0; i < 4; i++) {
        const idx = Math.floor(i * points.length / 4) + 5;
        const pt = points[idx % points.length];
        const boost = this.createBoostPad();
        boost.position.set(pt.x, pt.z * 0.15 + 0.05, pt.y);
        boost.lookAt(points[(idx + 1) % points.length].x, pt.z * 0.15 + 0.05, points[(idx + 1) % points.length].y);
        group.add(boost);
    }

    // Trees
    const treeCount = theme.name === 'space' ? 0 : 35;
    for (let i = 0; i < treeCount; i++) {
        const angle = (i / treeCount) * Math.PI * 2;
        const radius = (theme.radiusX + 15) + Math.random() * 25;
        const tree = this.createTree(theme.treeColor);
        tree.position.set(Math.cos(angle) * radius, 0, Math.sin(angle) * radius);
        const scale = 0.8 + Math.random() * 0.6;
        tree.scale.set(scale, scale, scale);
        group.add(tree);
    }

    // Decorative rocks for mountain/castle
    if (theme.name === 'mountain' || theme.name === 'castle') {
        for (let i = 0; i < 15; i++) {
            const angle = Math.random() * Math.PI * 2;
            const radius = (theme.radiusX + 12) + Math.random() * 15;
            const rock = new THREE.Mesh(
                new THREE.DodecahedronGeometry(1 + Math.random() * 2),
                new THREE.MeshStandardMaterial({ color: 0x666666 })
            );
            rock.position.set(Math.cos(angle) * radius, 0.5, Math.sin(angle) * radius);
            rock.castShadow = true;
            group.add(rock);
        }
    }

    // Item boxes
    for (let i = 0; i < 8; i++) {
        const idx = Math.floor(i * points.length / 8);
        const pt = points[idx % points.length];
        const box = this.createItemBox();
        box.position.set(pt.x, pt.z * 0.15 + 1.5, pt.y);
        box.userData = { angle: i * 0.785, speed: 1 + Math.random() };
        group.add(box);
    }

    // Clouds
    if (theme.name !== 'space') {
        for (let i = 0; i < 10; i++) {
            const cloud = this.createCloud();
            cloud.position.set((Math.random() - 0.5) * 200, 30 + Math.random() * 20, (Math.random() - 0.5) * 200);
            group.add(cloud);
        }
    }

    this.track = group;
    this.scene.add(this.track);
};

window.gameEngine.generateTrackPath = function (radiusX, radiusZ) {
    const points = [];
    const segments = 120;
    for (let i = 0; i < segments; i++) {
        const t = i / segments;
        const angle = t * Math.PI * 2;
        const rX = radiusX + Math.sin(angle * 3) * 5;
        const rZ = radiusZ + Math.cos(angle * 2) * 4;
        const x = Math.cos(angle) * rX;
        const y = Math.sin(angle) * rZ;
        const z = Math.sin(angle * 2) * 3 + Math.cos(angle * 4) * 2;
        points.push(new THREE.Vector3(x, y, z));
    }
    return points;
};

window.gameEngine.createShortcut = function (group, theme) {
    const startAngle = 0.5;
    const endAngle = 1.2;
    const shortcutPoints = [];
    const segments = 30;
    const innerRadius = theme.radiusX * 0.4;

    for (let i = 0; i <= segments; i++) {
        const t = i / segments;
        const angle = startAngle + t * (endAngle - startAngle);
        const x = Math.cos(angle) * innerRadius;
        const z = Math.sin(angle) * (theme.radiusZ * 0.4);
        const y = 3 + Math.sin(t * Math.PI) * 2;
        shortcutPoints.push(new THREE.Vector3(x, z, y));
    }

    const shortcutGeo = new THREE.BufferGeometry();
    const vertices = [];
    const width = 5;

    for (let i = 0; i < shortcutPoints.length - 1; i++) {
        const current = shortcutPoints[i];
        const next = shortcutPoints[i + 1];
        const tangent = new THREE.Vector2(next.x - current.x, next.y - current.y).normalize();
        const normal = new THREE.Vector2(-tangent.y, tangent.x);
        vertices.push(
            current.x + normal.x * width, current.z, current.y + normal.y * width,
            current.x - normal.x * width, current.z, current.y - normal.y * width
        );
    }

    shortcutGeo.setAttribute('position', new THREE.Float32BufferAttribute(vertices, 3));
    shortcutGeo.computeVertexNormals();

    const shortcut = new THREE.Mesh(
        shortcutGeo,
        new THREE.MeshStandardMaterial({ color: 0x8888ff, side: THREE.DoubleSide })
    );
    shortcut.userData.isShortcut = true;
    shortcut.receiveShadow = true;
    group.add(shortcut);

    for (let i = 0; i < shortcutPoints.length; i += 10) {
        const pt = shortcutPoints[i];
        const pillar = new THREE.Mesh(
            new THREE.CylinderGeometry(0.5, 0.5, pt.z, 8),
            new THREE.MeshStandardMaterial({ color: 0x555555 })
        );
        pillar.position.set(pt.x, pt.z / 2, pt.y);
        pillar.castShadow = true;
        group.add(pillar);
    }
};

window.gameEngine.createBoostPad = function () {
    const pad = new THREE.Mesh(
        new THREE.PlaneGeometry(4, 2),
        new THREE.MeshStandardMaterial({ color: 0x00ff88, emissive: 0x00ff88, emissiveIntensity: 0.4, side: THREE.DoubleSide })
    );
    pad.rotation.x = -Math.PI / 2;
    pad.position.y = 0.06;
    pad.userData.isBoostPad = true;
    return pad;
};

window.gameEngine.getTrackTheme = function (trackName) {
    const themes = {
        mushroom: { name: 'grassland', groundColor: 0x2d5016, roadColor: 0x444444, borderColor: 0xff0000, treeColor: 0x228B22, skyColor: 0x87CEEB, radiusX: 40, radiusZ: 25 },
        castle: { name: 'castle', groundColor: 0x4a2511, roadColor: 0x663333, borderColor: 0xff6600, treeColor: 0x3d1f0f, skyColor: 0x2c0b0b, radiusX: 45, radiusZ: 30 },
        rainbow: { name: 'space', groundColor: 0x1a1a3e, roadColor: 0xff00ff, borderColor: 0x00ffff, treeColor: 0x000000, skyColor: 0x0a0a2e, radiusX: 50, radiusZ: 35 },
        beach: { name: 'beach', groundColor: 0xe6c288, roadColor: 0xd4a76a, borderColor: 0x0099cc, treeColor: 0x2E8B57, skyColor: 0x87CEEB, radiusX: 42, radiusZ: 28 },
        haunted: { name: 'haunted', groundColor: 0x2a2a3e, roadColor: 0x555566, borderColor: 0x9933ff, treeColor: 0x1a1a2e, skyColor: 0x1a1a2e, radiusX: 38, radiusZ: 24 },
        mountain: { name: 'mountain', groundColor: 0x5a4a3a, roadColor: 0x6b5b4b, borderColor: 0xffffff, treeColor: 0x2f4f2f, skyColor: 0x87CEEB, radiusX: 48, radiusZ: 32 }
    };
    return themes[trackName] || themes.mushroom;
};

window.gameEngine.createTree = function (color) {
    const tree = new THREE.Group();
    const trunk = new THREE.Mesh(
        new THREE.CylinderGeometry(0.3, 0.5, 2, 8),
        new THREE.MeshStandardMaterial({ color: 0x8B4513 })
    );
    trunk.position.y = 1;
    trunk.castShadow = true;
    tree.add(trunk);

    const leaves = new THREE.Mesh(
        new THREE.ConeGeometry(2, 4, 8),
        new THREE.MeshStandardMaterial({ color: color })
    );
    leaves.position.y = 3;
    leaves.castShadow = true;
    tree.add(leaves);

    return tree;
};

window.gameEngine.createItemBox = function () {
    const box = new THREE.Mesh(
        new THREE.BoxGeometry(1.2, 1.2, 1.2),
        new THREE.MeshStandardMaterial({ color: 0xffd700, emissive: 0xff6600, emissiveIntensity: 0.3, transparent: true, opacity: 0.9 })
    );
    box.castShadow = true;
    box.userData.isItemBox = true;
    return box;
};

window.gameEngine.createCloud = function () {
    const cloud = new THREE.Group();
    const material = new THREE.MeshStandardMaterial({ color: 0xffffff, transparent: true, opacity: 0.8 });
    for (let i = 0; i < 5; i++) {
        const sphere = new THREE.Mesh(new THREE.SphereGeometry(2 + Math.random() * 2, 8, 8), material);
        sphere.position.set((Math.random() - 0.5) * 6, (Math.random() - 0.5) * 2, (Math.random() - 0.5) * 4);
        cloud.add(sphere);
    }
    return cloud;
};

window.gameEngine.createParticleExplosion = function (x, y, z, color, count) {
    const particles = new THREE.Group();
    const material = new THREE.MeshBasicMaterial({ color: color });

    for (let i = 0; i < count; i++) {
        const particle = new THREE.Mesh(new THREE.SphereGeometry(0.1 + Math.random() * 0.2, 6, 6), material);
        particle.position.set(x, y, z);
        particle.userData.velocity = new THREE.Vector3(
            (Math.random() - 0.5) * 0.5,
            Math.random() * 0.5,
            (Math.random() - 0.5) * 0.5
        );
        particle.userData.life = 1.0;
        particles.add(particle);
    }

    particles.userData.isParticles = true;
    this.scene.add(particles);
    return particles;
};
