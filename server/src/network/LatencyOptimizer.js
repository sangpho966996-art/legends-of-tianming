class LatencyOptimizer {
    constructor() {
        this.playerLatencies = new Map();
        this.serverMetrics = new Map();
        this.optimizationStrategies = new Map();
        this.compressionSettings = {
            enabled: true,
            threshold: 1024, // Compress messages larger than 1KB
            level: 6 // Compression level 1-9
        };
        
        this.initializeOptimizations();
    }

    initializeOptimizations() {
        console.log('⚡ Latency optimizer initialized');
        
        setInterval(() => {
            this.optimizeConnections();
        }, 30000); // Every 30 seconds
        
        setInterval(() => {
            this.measureLatencies();
        }, 5000); // Every 5 seconds
    }

    optimizeMessage(playerId, message) {
        const playerLatency = this.getPlayerLatency(playerId);
        const strategy = this.getOptimizationStrategy(playerLatency);
        
        let optimizedMessage = { ...message };
        
        if (strategy.useCompression && JSON.stringify(message).length > this.compressionSettings.threshold) {
            optimizedMessage = this.compressMessage(message);
        }
        
        if (strategy.reduceFrequency && message.type === 'position_update') {
            if (!this.shouldSendUpdate(playerId, message.timestamp)) {
                return null; // Skip this update
            }
        }
        
        if (strategy.prioritizeMessages) {
            optimizedMessage.priority = this.getMessagePriority(message.type);
        }
        
        if (message.type === 'movement' && strategy.addPrediction) {
            optimizedMessage.prediction = this.generateMovementPrediction(playerId, message);
        }
        
        return optimizedMessage;
    }

    getOptimizationStrategy(latency) {
        if (latency < 50) {
            return {
                useCompression: false,
                reduceFrequency: false,
                prioritizeMessages: false,
                addPrediction: false,
                updateRate: 60 // 60 FPS
            };
        } else if (latency < 100) {
            return {
                useCompression: true,
                reduceFrequency: false,
                prioritizeMessages: true,
                addPrediction: true,
                updateRate: 30 // 30 FPS
            };
        } else if (latency < 200) {
            return {
                useCompression: true,
                reduceFrequency: true,
                prioritizeMessages: true,
                addPrediction: true,
                updateRate: 20 // 20 FPS
            };
        } else {
            return {
                useCompression: true,
                reduceFrequency: true,
                prioritizeMessages: true,
                addPrediction: true,
                updateRate: 10 // 10 FPS
            };
        }
    }

    compressMessage(message) {
        const compressed = {
            ...message,
            compressed: true,
            originalSize: JSON.stringify(message).length
        };
        
        if (message.position) {
            compressed.position = this.compressPosition(message.position);
        }
        
        if (message.players && Array.isArray(message.players)) {
            compressed.players = this.compressPlayerArray(message.players);
        }
        
        if (message.type === 'position_update') {
            delete compressed.timestamp; // Server will add timestamp
            delete compressed.metadata; // Remove non-essential data
        }
        
        return compressed;
    }

    compressPosition(position) {
        return {
            x: Math.round(position.x * 100) / 100,
            y: Math.round(position.y * 100) / 100,
            z: Math.round(position.z * 100) / 100
        };
    }

    compressPlayerArray(players) {
        return players.map(player => ({
            id: player.id,
            pos: this.compressPosition(player.position),
            hp: Math.round(player.health),
        }));
    }

    shouldSendUpdate(playerId, timestamp) {
        const strategy = this.optimizationStrategies.get(playerId);
        if (!strategy) return true;
        
        const lastUpdate = strategy.lastUpdate || 0;
        const updateInterval = 1000 / strategy.updateRate;
        
        if (timestamp - lastUpdate >= updateInterval) {
            strategy.lastUpdate = timestamp;
            return true;
        }
        
        return false;
    }

    getMessagePriority(messageType) {
        const priorities = {
            'combat': 1, // Highest priority
            'movement': 2,
            'skill_use': 2,
            'chat': 3,
            'inventory': 4,
            'ui_update': 5 // Lowest priority
        };
        
        return priorities[messageType] || 3;
    }

    generateMovementPrediction(playerId, movementMessage) {
        const playerHistory = this.getPlayerMovementHistory(playerId);
        
        if (playerHistory.length < 2) {
            return null;
        }
        
        const current = movementMessage.position;
        const previous = playerHistory[playerHistory.length - 1].position;
        const timeDelta = movementMessage.timestamp - playerHistory[playerHistory.length - 1].timestamp;
        
        const velocity = {
            x: (current.x - previous.x) / timeDelta,
            y: (current.y - previous.y) / timeDelta,
            z: (current.z - previous.z) / timeDelta
        };
        
        const predictionTime = 100; // 100ms ahead
        const predictedPosition = {
            x: current.x + velocity.x * predictionTime,
            y: current.y + velocity.y * predictionTime,
            z: current.z + velocity.z * predictionTime
        };
        
        return {
            velocity: velocity,
            predictedPosition: predictedPosition,
            confidence: this.calculatePredictionConfidence(playerHistory)
        };
    }

    calculatePredictionConfidence(history) {
        if (history.length < 3) return 0.5;
        
        let consistencyScore = 0;
        for (let i = 2; i < history.length; i++) {
            const vel1 = this.calculateVelocity(history[i-2], history[i-1]);
            const vel2 = this.calculateVelocity(history[i-1], history[i]);
            
            const similarity = this.calculateVelocitySimilarity(vel1, vel2);
            consistencyScore += similarity;
        }
        
        return Math.min(1.0, consistencyScore / (history.length - 2));
    }

    calculateVelocity(pos1, pos2) {
        const timeDelta = pos2.timestamp - pos1.timestamp;
        return {
            x: (pos2.position.x - pos1.position.x) / timeDelta,
            y: (pos2.position.y - pos1.position.y) / timeDelta,
            z: (pos2.position.z - pos1.position.z) / timeDelta
        };
    }

    calculateVelocitySimilarity(vel1, vel2) {
        const dx = Math.abs(vel1.x - vel2.x);
        const dy = Math.abs(vel1.y - vel2.y);
        const dz = Math.abs(vel1.z - vel2.z);
        
        const maxDiff = 10; // Maximum expected velocity difference
        const similarity = 1 - Math.min(1, (dx + dy + dz) / maxDiff);
        
        return similarity;
    }

    measureLatencies() {
    }

    updatePlayerLatency(playerId, latency) {
        const playerData = this.playerLatencies.get(playerId) || {
            samples: [],
            average: 0,
            min: Infinity,
            max: 0
        };
        
        playerData.samples.push({
            latency: latency,
            timestamp: Date.now()
        });
        
        if (playerData.samples.length > 20) {
            playerData.samples.shift();
        }
        
        const latencies = playerData.samples.map(s => s.latency);
        playerData.average = latencies.reduce((sum, l) => sum + l, 0) / latencies.length;
        playerData.min = Math.min(...latencies);
        playerData.max = Math.max(...latencies);
        
        this.playerLatencies.set(playerId, playerData);
        
        this.updateOptimizationStrategy(playerId, playerData.average);
    }

    updateOptimizationStrategy(playerId, averageLatency) {
        const strategy = this.getOptimizationStrategy(averageLatency);
        strategy.lastUpdate = 0; // Reset update timer
        this.optimizationStrategies.set(playerId, strategy);
    }

    getPlayerLatency(playerId) {
        const playerData = this.playerLatencies.get(playerId);
        return playerData ? playerData.average : 100; // Default to 100ms
    }

    getPlayerMovementHistory(playerId) {
        return [];
    }

    optimizeConnections() {
        for (const [playerId, latencyData] of this.playerLatencies.entries()) {
            const strategy = this.optimizationStrategies.get(playerId);
            
            if (!strategy) continue;
            
            if (latencyData.average > 200 && strategy.updateRate > 10) {
                strategy.updateRate = Math.max(10, strategy.updateRate - 5);
                console.log(`⚡ Reduced update rate for player ${playerId} to ${strategy.updateRate} FPS`);
            } else if (latencyData.average < 50 && strategy.updateRate < 60) {
                strategy.updateRate = Math.min(60, strategy.updateRate + 5);
                console.log(`⚡ Increased update rate for player ${playerId} to ${strategy.updateRate} FPS`);
            }
        }
    }

    optimizeBandwidth(playerId, availableBandwidth) {
        const strategy = this.optimizationStrategies.get(playerId) || {};
        
        if (availableBandwidth < 100) { // Less than 100 KB/s
            strategy.reduceQuality = true;
            strategy.updateRate = Math.min(strategy.updateRate, 15);
            strategy.useCompression = true;
        } else if (availableBandwidth > 500) { // More than 500 KB/s
            strategy.reduceQuality = false;
            strategy.updateRate = Math.max(strategy.updateRate, 30);
        }
        
        this.optimizationStrategies.set(playerId, strategy);
    }

    adaptToNetworkConditions(playerId, conditions) {
        const strategy = this.optimizationStrategies.get(playerId) || {};
        
        if (conditions.packetLoss > 0.05) { // More than 5% packet loss
            strategy.useReliableDelivery = true;
            strategy.reduceFrequency = true;
            strategy.prioritizeMessages = true;
        }
        
        if (conditions.jitter > 50) { // More than 50ms jitter
            strategy.addPrediction = true;
            strategy.bufferSize = Math.max(strategy.bufferSize || 100, 200);
        }
        
        this.optimizationStrategies.set(playerId, strategy);
    }

    getOptimizationMetrics() {
        const metrics = {
            totalPlayers: this.playerLatencies.size,
            averageLatency: 0,
            optimizedConnections: 0,
            compressionRatio: 0,
            bandwidthSaved: 0
        };
        
        if (this.playerLatencies.size > 0) {
            const totalLatency = Array.from(this.playerLatencies.values())
                .reduce((sum, data) => sum + data.average, 0);
            metrics.averageLatency = totalLatency / this.playerLatencies.size;
        }
        
        metrics.optimizedConnections = Array.from(this.optimizationStrategies.values())
            .filter(strategy => strategy.useCompression || strategy.reduceFrequency).length;
        
        return metrics;
    }

    removePlayer(playerId) {
        this.playerLatencies.delete(playerId);
        this.optimizationStrategies.delete(playerId);
    }

    updateCompressionSettings(settings) {
        this.compressionSettings = { ...this.compressionSettings, ...settings };
        console.log('⚡ Compression settings updated:', this.compressionSettings);
    }
}

module.exports = LatencyOptimizer;
