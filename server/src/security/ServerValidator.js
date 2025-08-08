const crypto = require('crypto');

class ServerValidator {
    constructor() {
        this.validationRules = new Map();
        this.playerStates = new Map();
        this.actionHistory = new Map();
        this.trustedActions = new Set(['ping', 'heartbeat', 'disconnect']);
        
        this.initializeValidationRules();
    }

    initializeValidationRules() {
        console.log('🛡️ Server validator initialized');
        
        this.validationRules.set('combat', {
            maxDamagePerHit: 10000,
            maxAttackRange: 50,
            minCooldownBetweenAttacks: 500, // milliseconds
            maxCombatActionsPerSecond: 10
        });
        
        this.validationRules.set('movement', {
            maxSpeed: 15, // units per second
            maxTeleportDistance: 50,
            maxMovementActionsPerSecond: 60,
            boundaryLimits: {
                minX: -1000, maxX: 1000,
                minY: -1000, maxY: 1000,
                minZ: -100, maxZ: 100
            }
        });
        
        this.validationRules.set('economy', {
            maxGoldTransaction: 1000000,
            maxItemStackSize: 999,
            maxTradesPerMinute: 10,
            maxAuctionListings: 20
        });
        
        this.validationRules.set('skills', {
            maxSkillLevel: 100,
            maxSkillPointsPerLevel: 5,
            maxActiveSkills: 8,
            maxSkillUsesPerSecond: 20
        });
    }

    validateAction(playerId, action, timestamp) {
        try {
            if (this.trustedActions.has(action.type)) {
                return { valid: true };
            }
            
            const playerState = this.getPlayerState(playerId);
            
            if (!this.validateTimestamp(timestamp)) {
                return {
                    valid: false,
                    reason: 'Invalid timestamp',
                    severity: 'medium'
                };
            }
            
            let validationResult;
            switch (action.type) {
                case 'movement':
                    validationResult = this.validateMovement(playerId, action, playerState, timestamp);
                    break;
                case 'combat':
                    validationResult = this.validateCombat(playerId, action, playerState, timestamp);
                    break;
                case 'skill_use':
                    validationResult = this.validateSkillUse(playerId, action, playerState, timestamp);
                    break;
                case 'economy':
                    validationResult = this.validateEconomy(playerId, action, playerState, timestamp);
                    break;
                case 'chat':
                    validationResult = this.validateChat(playerId, action, playerState, timestamp);
                    break;
                default:
                    validationResult = this.validateGenericAction(playerId, action, playerState, timestamp);
            }
            
            if (validationResult.valid) {
                this.updatePlayerState(playerId, action, timestamp);
            }
            
            this.logValidation(playerId, action, validationResult, timestamp);
            
            return validationResult;
            
        } catch (error) {
            console.error(`Validation error for player ${playerId}:`, error);
            return {
                valid: false,
                reason: 'Validation system error',
                severity: 'high'
            };
        }
    }

    validateMovement(playerId, action, playerState, timestamp) {
        const rules = this.validationRules.get('movement');
        const { position, velocity } = action;
        
        if (position.x < rules.boundaryLimits.minX || position.x > rules.boundaryLimits.maxX ||
            position.y < rules.boundaryLimits.minY || position.y > rules.boundaryLimits.maxY ||
            position.z < rules.boundaryLimits.minZ || position.z > rules.boundaryLimits.maxZ) {
            return {
                valid: false,
                reason: 'Position out of bounds',
                severity: 'high',
                details: { position, bounds: rules.boundaryLimits }
            };
        }
        
        if (playerState.lastPosition && playerState.lastMovementTime) {
            const distance = this.calculateDistance(playerState.lastPosition, position);
            const timeDelta = (timestamp - playerState.lastMovementTime) / 1000;
            const speed = distance / timeDelta;
            
            if (speed > rules.maxSpeed && distance > rules.maxTeleportDistance) {
                return {
                    valid: false,
                    reason: 'Movement speed too high',
                    severity: 'high',
                    details: { speed, maxSpeed: rules.maxSpeed, distance }
                };
            }
        }
        
        if (!this.validateActionRate(playerId, 'movement', timestamp, rules.maxMovementActionsPerSecond)) {
            return {
                valid: false,
                reason: 'Movement rate too high',
                severity: 'medium'
            };
        }
        
        return { valid: true };
    }

    validateCombat(playerId, action, playerState, timestamp) {
        const rules = this.validationRules.get('combat');
        const { targetId, damage, skillId, position } = action;
        
        if (damage > rules.maxDamagePerHit) {
            return {
                valid: false,
                reason: 'Damage amount too high',
                severity: 'high',
                details: { damage, maxDamage: rules.maxDamagePerHit }
            };
        }
        
        const targetState = this.getPlayerState(targetId);
        if (targetState.position) {
            const distance = this.calculateDistance(position, targetState.position);
            if (distance > rules.maxAttackRange) {
                return {
                    valid: false,
                    reason: 'Attack range too far',
                    severity: 'high',
                    details: { distance, maxRange: rules.maxAttackRange }
                };
            }
        }
        
        if (playerState.lastCombatAction) {
            const timeSinceLastAttack = timestamp - playerState.lastCombatAction;
            if (timeSinceLastAttack < rules.minCooldownBetweenAttacks) {
                return {
                    valid: false,
                    reason: 'Attack cooldown not met',
                    severity: 'medium',
                    details: { timeSinceLastAttack, minCooldown: rules.minCooldownBetweenAttacks }
                };
            }
        }
        
        if (!this.validateActionRate(playerId, 'combat', timestamp, rules.maxCombatActionsPerSecond)) {
            return {
                valid: false,
                reason: 'Combat rate too high',
                severity: 'medium'
            };
        }
        
        if (skillId && !this.validateSkillAvailability(playerId, skillId, playerState)) {
            return {
                valid: false,
                reason: 'Skill not available',
                severity: 'medium',
                details: { skillId }
            };
        }
        
        return { valid: true };
    }

    validateSkillUse(playerId, action, playerState, timestamp) {
        const rules = this.validationRules.get('skills');
        const { skillId, targetId, position } = action;
        
        if (!this.validateSkillExists(skillId)) {
            return {
                valid: false,
                reason: 'Invalid skill ID',
                severity: 'high',
                details: { skillId }
            };
        }
        
        if (!this.validateSkillCooldown(playerId, skillId, playerState, timestamp)) {
            return {
                valid: false,
                reason: 'Skill on cooldown',
                severity: 'low',
                details: { skillId }
            };
        }
        
        if (!this.validateSkillResources(playerId, skillId, playerState)) {
            return {
                valid: false,
                reason: 'Insufficient resources',
                severity: 'medium',
                details: { skillId }
            };
        }
        
        if (!this.validateActionRate(playerId, 'skill', timestamp, rules.maxSkillUsesPerSecond)) {
            return {
                valid: false,
                reason: 'Skill use rate too high',
                severity: 'medium'
            };
        }
        
        return { valid: true };
    }

    validateEconomy(playerId, action, playerState, timestamp) {
        const rules = this.validationRules.get('economy');
        const { type, amount, itemId, targetId } = action;
        
        switch (type) {
            case 'trade':
                return this.validateTrade(playerId, action, playerState, rules);
            case 'auction':
                return this.validateAuction(playerId, action, playerState, rules);
            case 'purchase':
                return this.validatePurchase(playerId, action, playerState, rules);
            default:
                return { valid: true };
        }
    }

    validateTrade(playerId, action, playerState, rules) {
        const { amount, itemId } = action;
        
        if (amount > rules.maxGoldTransaction) {
            return {
                valid: false,
                reason: 'Trade amount too high',
                severity: 'high',
                details: { amount, maxAmount: rules.maxGoldTransaction }
            };
        }
        
        if (!this.validatePlayerResources(playerId, { gold: amount, items: [itemId] })) {
            return {
                valid: false,
                reason: 'Insufficient resources for trade',
                severity: 'medium'
            };
        }
        
        return { valid: true };
    }

    validateChat(playerId, action, playerState, timestamp) {
        const { message, channel } = action;
        
        if (message.length > 500) {
            return {
                valid: false,
                reason: 'Message too long',
                severity: 'low'
            };
        }
        
        if (!this.validateActionRate(playerId, 'chat', timestamp, 5)) { // 5 messages per second max
            return {
                valid: false,
                reason: 'Chat rate too high',
                severity: 'low'
            };
        }
        
        if (!this.validateChannelAccess(playerId, channel)) {
            return {
                valid: false,
                reason: 'No access to channel',
                severity: 'medium',
                details: { channel }
            };
        }
        
        return { valid: true };
    }

    validateGenericAction(playerId, action, playerState, timestamp) {
        
        if (!action.type) {
            return {
                valid: false,
                reason: 'Missing action type',
                severity: 'medium'
            };
        }
        
        if (!this.validateActionRate(playerId, 'general', timestamp, 100)) { // 100 actions per second max
            return {
                valid: false,
                reason: 'General action rate too high',
                severity: 'medium'
            };
        }
        
        return { valid: true };
    }

    validateTimestamp(timestamp) {
        const now = Date.now();
        const maxTimeDiff = 30000; // 30 seconds
        
        return Math.abs(now - timestamp) <= maxTimeDiff;
    }

    validateActionRate(playerId, actionType, timestamp, maxActionsPerSecond) {
        const history = this.getActionHistory(playerId, actionType);
        const windowSize = 1000; // 1 second window
        
        const recentActions = history.filter(time => timestamp - time < windowSize);
        
        if (recentActions.length >= maxActionsPerSecond) {
            return false;
        }
        
        recentActions.push(timestamp);
        this.setActionHistory(playerId, actionType, recentActions);
        
        return true;
    }

    validateSkillExists(skillId) {
        return true; // Placeholder
    }

    validateSkillCooldown(playerId, skillId, playerState, timestamp) {
        const skillCooldowns = playerState.skillCooldowns || {};
        const lastUse = skillCooldowns[skillId] || 0;
        const cooldownTime = this.getSkillCooldown(skillId);
        
        return timestamp - lastUse >= cooldownTime;
    }

    validateSkillResources(playerId, skillId, playerState) {
        const resourceCost = this.getSkillResourceCost(skillId);
        const playerResources = playerState.resources || {};
        
        return playerResources.mana >= resourceCost.mana &&
               playerResources.stamina >= resourceCost.stamina;
    }

    validateSkillAvailability(playerId, skillId, playerState) {
        return this.validateSkillExists(skillId) &&
               this.validateSkillCooldown(playerId, skillId, playerState, Date.now()) &&
               this.validateSkillResources(playerId, skillId, playerState);
    }

    validatePlayerResources(playerId, requiredResources) {
        const playerState = this.getPlayerState(playerId);
        const playerResources = playerState.resources || {};
        
        if (requiredResources.gold && playerResources.gold < requiredResources.gold) {
            return false;
        }
        
        
        return true;
    }

    validateChannelAccess(playerId, channel) {
        const playerState = this.getPlayerState(playerId);
        
        switch (channel) {
            case 'guild':
                return playerState.guildId != null;
            case 'party':
                return playerState.partyId != null;
            case 'world':
            case 'region':
                return true;
            default:
                return false;
        }
    }

    getPlayerState(playerId) {
        if (!this.playerStates.has(playerId)) {
            this.playerStates.set(playerId, {
                position: null,
                lastMovementTime: 0,
                lastCombatAction: 0,
                skillCooldowns: {},
                resources: { mana: 1000, stamina: 1000, gold: 0 },
                guildId: null,
                partyId: null
            });
        }
        
        return this.playerStates.get(playerId);
    }

    updatePlayerState(playerId, action, timestamp) {
        const playerState = this.getPlayerState(playerId);
        
        switch (action.type) {
            case 'movement':
                playerState.position = action.position;
                playerState.lastMovementTime = timestamp;
                break;
            case 'combat':
                playerState.lastCombatAction = timestamp;
                break;
            case 'skill_use':
                if (!playerState.skillCooldowns) playerState.skillCooldowns = {};
                playerState.skillCooldowns[action.skillId] = timestamp;
                
                const cost = this.getSkillResourceCost(action.skillId);
                playerState.resources.mana -= cost.mana;
                playerState.resources.stamina -= cost.stamina;
                break;
        }
    }

    getActionHistory(playerId, actionType) {
        const key = `${playerId}:${actionType}`;
        return this.actionHistory.get(key) || [];
    }

    setActionHistory(playerId, actionType, history) {
        const key = `${playerId}:${actionType}`;
        this.actionHistory.set(key, history);
    }

    calculateDistance(pos1, pos2) {
        const dx = pos1.x - pos2.x;
        const dy = pos1.y - pos2.y;
        const dz = pos1.z - pos2.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }

    getSkillCooldown(skillId) {
        return 1000; // Default 1 second cooldown
    }

    getSkillResourceCost(skillId) {
        return { mana: 50, stamina: 25 }; // Default costs
    }

    removePlayer(playerId) {
        this.playerStates.delete(playerId);
        
        for (const [key, history] of this.actionHistory.entries()) {
            if (key.startsWith(`${playerId}:`)) {
                this.actionHistory.delete(key);
            }
        }
    }

    getValidationStats() {
        return {
            totalPlayers: this.playerStates.size,
            totalValidationRules: this.validationRules.size,
            actionHistorySize: this.actionHistory.size
        };
    }

    validateSecurityToken(playerId, token, action) {
        const expectedToken = this.generateActionToken(playerId, action);
        return token === expectedToken;
    }

    generateActionToken(playerId, action) {
        const data = `${playerId}:${action.type}:${action.timestamp}:${process.env.VALIDATION_SECRET}`;
        return crypto.createHash('sha256').update(data).digest('hex').substring(0, 16);
    }
}

module.exports = ServerValidator;
