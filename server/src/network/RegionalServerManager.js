const WebSocket = require('ws');
const http = require('http');

class RegionalServerManager {
    constructor() {
        this.regions = new Map();
        this.playerConnections = new Map();
        this.serverInstances = new Map();
        this.loadBalancer = new LoadBalancer();
        
        this.regionConfigs = {
            'NA': {
                name: 'North America',
                location: 'us-east-1',
                maxPlayers: 2000,
                port: 8001,
                latencyThreshold: 150
            },
            'EU': {
                name: 'Europe',
                location: 'eu-west-1',
                maxPlayers: 2000,
                port: 8002,
                latencyThreshold: 120
            },
            'SEA': {
                name: 'Southeast Asia',
                location: 'ap-southeast-1',
                maxPlayers: 2000,
                port: 8003,
                latencyThreshold: 100
            }
        };
        
        this.initializeRegionalServers();
    }

    async initializeRegionalServers() {
        console.log('🌍 Initializing regional servers...');
        
        for (const [regionCode, config] of Object.entries(this.regionConfigs)) {
            try {
                await this.createRegionalServer(regionCode, config);
                console.log(`✅ Regional server ${regionCode} (${config.name}) initialized on port ${config.port}`);
            } catch (error) {
                console.error(`❌ Failed to initialize regional server ${regionCode}:`, error);
            }
        }
        
        this.startHealthChecks();
        this.startLatencyMonitoring();
    }

    async createRegionalServer(regionCode, config) {
        const server = http.createServer();
        const wss = new WebSocket.Server({ 
            server,
            perMessageDeflate: {
                zlibDeflateOptions: {
                    level: 3,
                    chunkSize: 1024
                }
            }
        });
        
        const regionInstance = new RegionalServerInstance(regionCode, config, wss);
        
        wss.on('connection', (ws, request) => {
            this.handlePlayerConnection(ws, request, regionCode, regionInstance);
        });
        
        server.listen(config.port, () => {
            console.log(`🌍 Regional server ${regionCode} listening on port ${config.port}`);
        });
        
        this.serverInstances.set(regionCode, {
            server: server,
            wss: wss,
            instance: regionInstance,
            config: config,
            status: 'active',
            playerCount: 0,
            lastHealthCheck: Date.now()
        });
    }

    handlePlayerConnection(ws, request, regionCode, regionInstance) {
        const playerId = this.extractPlayerIdFromRequest(request);
        
        if (!playerId) {
            ws.close(1008, 'Invalid player authentication');
            return;
        }
        
        if (this.playerConnections.has(playerId)) {
            const existingConnection = this.playerConnections.get(playerId);
            existingConnection.ws.close(1000, 'Connected from another region');
        }
        
        const playerConnection = {
            playerId: playerId,
            ws: ws,
            regionCode: regionCode,
            connectedAt: Date.now(),
            lastPing: Date.now(),
            latency: 0
        };
        
        this.playerConnections.set(playerId, playerConnection);
        regionInstance.addPlayer(playerId, playerConnection);
        
        const serverInfo = this.serverInstances.get(regionCode);
        serverInfo.playerCount++;
        
        ws.on('message', (data) => {
            this.handlePlayerMessage(playerId, data, regionCode, regionInstance);
        });
        
        ws.on('close', () => {
            this.handlePlayerDisconnection(playerId, regionCode, regionInstance);
        });
        
        ws.on('pong', () => {
            playerConnection.lastPing = Date.now();
        });
        
        console.log(`👤 Player ${playerId} connected to region ${regionCode}`);
    }

    handlePlayerMessage(playerId, data, regionCode, regionInstance) {
        try {
            const message = JSON.parse(data);
            
            if (message.type === 'ping') {
                const playerConnection = this.playerConnections.get(playerId);
                if (playerConnection) {
                    playerConnection.latency = Date.now() - message.timestamp;
                }
                return;
            }
            
            regionInstance.handlePlayerMessage(playerId, message);
            
        } catch (error) {
            console.error(`Error handling message from player ${playerId}:`, error);
        }
    }

    handlePlayerDisconnection(playerId, regionCode, regionInstance) {
        this.playerConnections.delete(playerId);
        regionInstance.removePlayer(playerId);
        
        const serverInfo = this.serverInstances.get(regionCode);
        if (serverInfo) {
            serverInfo.playerCount = Math.max(0, serverInfo.playerCount - 1);
        }
        
        console.log(`👤 Player ${playerId} disconnected from region ${regionCode}`);
    }

    async selectOptimalRegion(playerId, clientLatencies) {
        let bestRegion = 'NA'; // Default
        let bestLatency = Infinity;
        
        for (const [regionCode, latency] of Object.entries(clientLatencies)) {
            const serverInfo = this.serverInstances.get(regionCode);
            
            if (!serverInfo || serverInfo.status !== 'active') {
                continue;
            }
            
            const loadFactor = serverInfo.playerCount / serverInfo.config.maxPlayers;
            const adjustedLatency = latency * (1 + loadFactor * 0.5);
            
            if (adjustedLatency < bestLatency && latency < serverInfo.config.latencyThreshold) {
                bestLatency = adjustedLatency;
                bestRegion = regionCode;
            }
        }
        
        return {
            region: bestRegion,
            latency: bestLatency,
            serverLoad: this.getServerLoad(bestRegion)
        };
    }

    broadcastToAllRegions(message, excludeRegion = null) {
        for (const [regionCode, serverInfo] of this.serverInstances.entries()) {
            if (regionCode !== excludeRegion && serverInfo.status === 'active') {
                serverInfo.instance.broadcast(message);
            }
        }
    }

    sendToRegion(regionCode, message) {
        const serverInfo = this.serverInstances.get(regionCode);
        if (serverInfo && serverInfo.status === 'active') {
            serverInfo.instance.broadcast(message);
        }
    }

    startHealthChecks() {
        setInterval(() => {
            this.performHealthChecks();
        }, 30000); // Every 30 seconds
    }

    async performHealthChecks() {
        for (const [regionCode, serverInfo] of this.serverInstances.entries()) {
            try {
                const healthStatus = await serverInfo.instance.getHealthStatus();
                
                if (healthStatus.status === 'healthy') {
                    serverInfo.status = 'active';
                    serverInfo.lastHealthCheck = Date.now();
                } else {
                    console.warn(`⚠️ Regional server ${regionCode} health check failed:`, healthStatus);
                    serverInfo.status = 'degraded';
                }
                
            } catch (error) {
                console.error(`❌ Health check failed for region ${regionCode}:`, error);
                serverInfo.status = 'error';
            }
        }
    }

    startLatencyMonitoring() {
        setInterval(() => {
            this.measurePlayerLatencies();
        }, 10000); // Every 10 seconds
    }

    measurePlayerLatencies() {
        const pingMessage = {
            type: 'ping',
            timestamp: Date.now()
        };
        
        for (const [playerId, connection] of this.playerConnections.entries()) {
            if (connection.ws.readyState === WebSocket.OPEN) {
                connection.ws.ping();
                connection.ws.send(JSON.stringify(pingMessage));
            }
        }
    }

    getServerLoad(regionCode) {
        const serverInfo = this.serverInstances.get(regionCode);
        if (!serverInfo) return 1.0;
        
        return serverInfo.playerCount / serverInfo.config.maxPlayers;
    }

    getRegionStats() {
        const stats = {};
        
        for (const [regionCode, serverInfo] of this.serverInstances.entries()) {
            stats[regionCode] = {
                name: serverInfo.config.name,
                status: serverInfo.status,
                playerCount: serverInfo.playerCount,
                maxPlayers: serverInfo.config.maxPlayers,
                load: this.getServerLoad(regionCode),
                lastHealthCheck: serverInfo.lastHealthCheck
            };
        }
        
        return stats;
    }

    extractPlayerIdFromRequest(request) {
        const url = new URL(request.url, 'http://localhost');
        return url.searchParams.get('playerId');
    }

    getPlayerRegion(playerId) {
        const connection = this.playerConnections.get(playerId);
        return connection ? connection.regionCode : null;
    }

    getPlayerLatency(playerId) {
        const connection = this.playerConnections.get(playerId);
        return connection ? connection.latency : null;
    }

    async shutdown() {
        console.log('🛑 Shutting down regional servers...');
        
        for (const [regionCode, serverInfo] of this.serverInstances.entries()) {
            try {
                await serverInfo.instance.shutdown();
                serverInfo.server.close();
                console.log(`✅ Regional server ${regionCode} shut down gracefully`);
            } catch (error) {
                console.error(`❌ Error shutting down regional server ${regionCode}:`, error);
            }
        }
    }
}

class RegionalServerInstance {
    constructor(regionCode, config, wss) {
        this.regionCode = regionCode;
        this.config = config;
        this.wss = wss;
        this.players = new Map();
        this.gameInstances = new Map();
        this.startTime = Date.now();
    }

    addPlayer(playerId, connection) {
        this.players.set(playerId, connection);
    }

    removePlayer(playerId) {
        this.players.delete(playerId);
    }

    handlePlayerMessage(playerId, message) {
        switch (message.type) {
            case 'movement':
                this.handleMovement(playerId, message);
                break;
            case 'combat':
                this.handleCombat(playerId, message);
                break;
            case 'chat':
                this.handleChat(playerId, message);
                break;
            default:
                console.warn(`Unknown message type: ${message.type}`);
        }
    }

    handleMovement(playerId, message) {
        const nearbyPlayers = this.getNearbyPlayers(playerId, message.position);
        
        const movementUpdate = {
            type: 'player_movement',
            playerId: playerId,
            position: message.position,
            timestamp: Date.now()
        };
        
        this.broadcastToPlayers(nearbyPlayers, movementUpdate);
    }

    handleCombat(playerId, message) {
        const combatResult = {
            type: 'combat_result',
            attacker: playerId,
            target: message.target,
            damage: message.damage,
            timestamp: Date.now()
        };
        
        this.broadcastToPlayers([playerId, message.target], combatResult);
    }

    handleChat(playerId, message) {
        const chatMessage = {
            type: 'chat_message',
            playerId: playerId,
            channel: message.channel,
            content: message.content,
            timestamp: Date.now()
        };
        
        if (message.channel === 'world') {
            this.broadcast(chatMessage);
        } else {
        }
    }

    broadcast(message) {
        const messageStr = JSON.stringify(message);
        
        for (const [playerId, connection] of this.players.entries()) {
            if (connection.ws.readyState === WebSocket.OPEN) {
                connection.ws.send(messageStr);
            }
        }
    }

    broadcastToPlayers(playerIds, message) {
        const messageStr = JSON.stringify(message);
        
        for (const playerId of playerIds) {
            const connection = this.players.get(playerId);
            if (connection && connection.ws.readyState === WebSocket.OPEN) {
                connection.ws.send(messageStr);
            }
        }
    }

    getNearbyPlayers(playerId, position, radius = 100) {
        const nearbyPlayers = [];
        
        for (const [otherPlayerId, connection] of this.players.entries()) {
            if (otherPlayerId !== playerId) {
                nearbyPlayers.push(otherPlayerId);
            }
        }
        
        return nearbyPlayers;
    }

    async getHealthStatus() {
        return {
            status: 'healthy',
            uptime: Date.now() - this.startTime,
            playerCount: this.players.size,
            memoryUsage: process.memoryUsage(),
            timestamp: Date.now()
        };
    }

    async shutdown() {
        for (const [playerId, connection] of this.players.entries()) {
            connection.ws.close(1000, 'Server shutting down');
        }
        
        this.players.clear();
    }
}

class LoadBalancer {
    constructor() {
        this.algorithms = {
            'round_robin': this.roundRobin.bind(this),
            'least_connections': this.leastConnections.bind(this),
            'weighted_response_time': this.weightedResponseTime.bind(this)
        };
        
        this.currentAlgorithm = 'least_connections';
        this.roundRobinIndex = 0;
    }

    selectServer(servers, algorithm = null) {
        const activeServers = servers.filter(s => s.status === 'active');
        
        if (activeServers.length === 0) {
            return null;
        }
        
        const selectedAlgorithm = algorithm || this.currentAlgorithm;
        return this.algorithms[selectedAlgorithm](activeServers);
    }

    roundRobin(servers) {
        const server = servers[this.roundRobinIndex % servers.length];
        this.roundRobinIndex++;
        return server;
    }

    leastConnections(servers) {
        return servers.reduce((min, server) => 
            server.playerCount < min.playerCount ? server : min
        );
    }

    weightedResponseTime(servers) {
        let bestServer = servers[0];
        let bestScore = Infinity;
        
        for (const server of servers) {
            const loadFactor = server.playerCount / server.config.maxPlayers;
            const score = server.averageLatency * (1 + loadFactor);
            
            if (score < bestScore) {
                bestScore = score;
                bestServer = server;
            }
        }
        
        return bestServer;
    }
}

module.exports = RegionalServerManager;
