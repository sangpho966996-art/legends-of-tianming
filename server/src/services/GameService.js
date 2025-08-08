class GameService {
    constructor() {
        this.players = new Map();
        this.guilds = new Map();
        this.worldState = {
            qingzePlains: {
                monsters: new Map(),
                bosses: new Map(),
                lastBossSpawn: Date.now()
            }
        };
        this.pvpMatches = new Map();
        this.dungeonInstances = new Map();
        
        this.startWorldTick();
    }

    startWorldTick() {
        setInterval(() => {
            this.updateWorldState();
            this.updateMonsters();
            this.checkBossSpawns();
        }, 1000);
    }

    addPlayer(playerId, playerData) {
        this.players.set(playerId, {
            ...playerData,
            lastUpdate: Date.now(),
            position: { x: 0, y: 0, z: 0 },
            health: playerData.maxHealth || 100,
            mana: playerData.maxMana || 100,
            inCombat: false,
            target: null
        });
        console.log(`Player ${playerId} added to game world`);
    }

    removePlayer(playerId) {
        if (this.players.has(playerId)) {
            this.players.delete(playerId);
            console.log(`Player ${playerId} removed from game world`);
        }
    }

    updatePlayerPosition(playerId, position) {
        const player = this.players.get(playerId);
        if (player) {
            player.position = position;
            player.lastUpdate = Date.now();
            return true;
        }
        return false;
    }

    getPlayersInRange(position, range = 50) {
        const playersInRange = [];
        for (const [id, player] of this.players) {
            const distance = this.calculateDistance(position, player.position);
            if (distance <= range) {
                playersInRange.push({ id, ...player });
            }
        }
        return playersInRange;
    }

    calculateDistance(pos1, pos2) {
        const dx = pos1.x - pos2.x;
        const dy = pos1.y - pos2.y;
        const dz = pos1.z - pos2.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }

    updateWorldState() {
        const now = Date.now();
        
        for (const [playerId, player] of this.players) {
            if (now - player.lastUpdate > 30000) {
                this.removePlayer(playerId);
            }
        }
    }

    updateMonsters() {
        for (const region of Object.values(this.worldState)) {
            for (const [monsterId, monster] of region.monsters) {
                if (monster.health <= 0 && !monster.respawnTime) {
                    monster.respawnTime = Date.now() + 30000;
                } else if (monster.respawnTime && Date.now() > monster.respawnTime) {
                    monster.health = monster.maxHealth;
                    monster.respawnTime = null;
                }
            }
        }
    }

    checkBossSpawns() {
        const now = Date.now();
        const bossSpawnInterval = 2 * 60 * 60 * 1000;
        
        for (const [regionName, region] of Object.entries(this.worldState)) {
            if (now - region.lastBossSpawn > bossSpawnInterval) {
                this.spawnWorldBoss(regionName);
                region.lastBossSpawn = now;
            }
        }
    }

    spawnWorldBoss(regionName) {
        const bossId = `boss_${regionName}_${Date.now()}`;
        const boss = {
            id: bossId,
            name: 'Ancient Guardian',
            level: 60,
            health: 50000,
            maxHealth: 50000,
            position: { x: 0, y: 0, z: 0 },
            spawned: Date.now()
        };
        
        this.worldState[regionName].bosses.set(bossId, boss);
        console.log(`World boss spawned in ${regionName}: ${boss.name}`);
        
        return boss;
    }

    getPlayerCount() {
        return this.players.size;
    }

    getWorldState() {
        return {
            playerCount: this.players.size,
            regions: Object.keys(this.worldState),
            activeBosses: Object.values(this.worldState)
                .reduce((total, region) => total + region.bosses.size, 0)
        };
    }
}

module.exports = GameService;
