class SocketManager {
    constructor(io, gameService) {
        this.io = io;
        this.gameService = gameService;
        this.playerSockets = new Map();
        this.setupSocketHandlers();
    }

    setupSocketHandlers() {
        this.io.on('connection', (socket) => {
            this.handleConnection(socket);
        });
    }

    handleConnection(socket) {
        socket.on('player:join', (data) => {
            this.handlePlayerJoin(socket, data);
        });

        socket.on('player:move', (data) => {
            this.handlePlayerMove(socket, data);
        });

        socket.on('combat:attack', (data) => {
            this.handleCombatAction(socket, data);
        });

        socket.on('skill:cast', (data) => {
            this.handleSkillCast(socket, data);
        });

        socket.on('chat:message', (data) => {
            this.handleChatMessage(socket, data);
        });

        socket.on('pvp:challenge', (data) => {
            this.handlePvPChallenge(socket, data);
        });

        socket.on('disconnect', () => {
            this.handleDisconnect(socket);
        });
    }

    handlePlayerJoin(socket, data) {
        const { playerId, characterData } = data;
        
        this.playerSockets.set(playerId, socket);
        socket.playerId = playerId;
        
        this.gameService.addPlayer(playerId, characterData);
        
        socket.emit('player:joined', {
            success: true,
            worldState: this.gameService.getWorldState()
        });

        socket.broadcast.emit('player:entered', {
            playerId,
            character: characterData
        });

        console.log(`Player ${playerId} joined the game`);
    }

    handlePlayerMove(socket, data) {
        const { position, rotation } = data;
        const playerId = socket.playerId;
        
        if (this.gameService.updatePlayerPosition(playerId, position)) {
            socket.broadcast.emit('player:moved', {
                playerId,
                position,
                rotation
            });
        }
    }

    handleCombatAction(socket, data) {
        const { targetId, damage, skillId } = data;
        const attackerId = socket.playerId;
        
        const result = this.processCombatAction(attackerId, targetId, damage, skillId);
        
        if (result.success) {
            this.io.emit('combat:hit', {
                attackerId,
                targetId,
                damage: result.damage,
                skillId,
                effects: result.effects
            });
        }
    }

    handleSkillCast(socket, data) {
        const { skillId, targetId, position } = data;
        const casterId = socket.playerId;
        
        const result = this.processSkillCast(casterId, skillId, targetId, position);
        
        if (result.success) {
            this.io.emit('skill:cast', {
                casterId,
                skillId,
                targetId,
                position,
                effects: result.effects
            });
        }
    }

    handleChatMessage(socket, data) {
        const { message, channel } = data;
        const playerId = socket.playerId;
        
        const chatData = {
            playerId,
            message,
            channel,
            timestamp: Date.now()
        };

        if (channel === 'global') {
            this.io.emit('chat:message', chatData);
        } else if (channel === 'guild') {
        }
    }

    handlePvPChallenge(socket, data) {
        const { targetPlayerId } = data;
        const challengerId = socket.playerId;
        
        const targetSocket = this.playerSockets.get(targetPlayerId);
        if (targetSocket) {
            targetSocket.emit('pvp:challenge_received', {
                challengerId,
                challengerName: data.challengerName
            });
        }
    }

    handleDisconnect(socket) {
        const playerId = socket.playerId;
        if (playerId) {
            this.gameService.removePlayer(playerId);
            this.playerSockets.delete(playerId);
            
            socket.broadcast.emit('player:left', { playerId });
            console.log(`Player ${playerId} disconnected`);
        }
    }

    processCombatAction(attackerId, targetId, damage, skillId) {
        const attacker = this.gameService.players.get(attackerId);
        const target = this.gameService.players.get(targetId);
        
        if (!attacker || !target) {
            return { success: false, error: 'Invalid players' };
        }

        const distance = this.gameService.calculateDistance(attacker.position, target.position);
        if (distance > 10) { // Max attack range
            return { success: false, error: 'Target out of range' };
        }

        const finalDamage = Math.max(1, damage - (target.defense || 0));
        target.health = Math.max(0, target.health - finalDamage);
        
        return {
            success: true,
            damage: finalDamage,
            effects: target.health <= 0 ? ['death'] : []
        };
    }

    processSkillCast(casterId, skillId, targetId, position) {
        const caster = this.gameService.players.get(casterId);
        if (!caster) {
            return { success: false, error: 'Invalid caster' };
        }

        return {
            success: true,
            effects: ['skill_cast']
        };
    }

    broadcastToRegion(regionName, event, data) {
        this.io.emit(event, data);
    }
}

module.exports = SocketManager;
