const crypto = require('crypto');
const rateLimit = require('express-rate-limit');

class AntiCheatSystem {
    constructor() {
        this.suspiciousActivities = new Map();
        this.playerValidationData = new Map();
        this.actionTimestamps = new Map();
        this.movementHistory = new Map();
        this.combatValidation = new Map();
        
        this.rateLimits = {
            movement: { maxActions: 60, windowMs: 1000 }, // 60 moves per second max
            combat: { maxActions: 10, windowMs: 1000 }, // 10 combat actions per second max
            chat: { maxActions: 5, windowMs: 1000 }, // 5 messages per second max
            trade: { maxActions: 3, windowMs: 5000 }, // 3 trades per 5 seconds max
            skill: { maxActions: 20, windowMs: 1000 } // 20 skill uses per second max
        };
        
        this.initializeValidation();
    }

    initializeValidation() {
        console.log('🛡️ Anti-cheat system initialized');
        
        setInterval(() => {
            this.cleanupOldData();
        }, 5 * 60 * 1000);
    }

    validateMovement(playerId, currentPosition, previousPosition, timestamp, deltaTime) {
        const playerHistory = this.movementHistory.get(playerId) || [];
        
        const distance = this.calculateDistance(currentPosition, previousPosition);
        const speed = distance / deltaTime;
        
        const maxSpeed = 15; // Adjust based on game mechanics
        const maxTeleportDistance = 50; // Maximum allowed instant movement
        
        if (speed > maxSpeed && distance > maxTeleportDistance) {
            this.flagSuspiciousActivity(playerId, 'SPEED_HACK', {
                speed: speed,
                maxSpeed: maxSpeed,
                distance: distance,
                position: currentPosition
            });
            return false;
        }
        
        if (distance > maxTeleportDistance && deltaTime < 0.1) {
            this.flagSuspiciousActivity(playerId, 'TELEPORT_HACK', {
                distance: distance,
                deltaTime: deltaTime,
                position: currentPosition
            });
            return false;
        }
        
        playerHistory.push({
            position: currentPosition,
            timestamp: timestamp,
            speed: speed
        });
        
        if (playerHistory.length > 10) {
            playerHistory.shift();
        }
        
        this.movementHistory.set(playerId, playerHistory);
        return true;
    }

    validateCombatAction(playerId, action, targetId, damage, timestamp) {
        const playerCombat = this.combatValidation.get(playerId) || {
            lastActions: [],
            totalDamage: 0,
            actionCount: 0
        };
        
        if (!this.checkRateLimit(playerId, 'combat', timestamp)) {
            this.flagSuspiciousActivity(playerId, 'COMBAT_SPAM', {
                action: action,
                timestamp: timestamp
            });
            return false;
        }
        
        const maxDamagePerHit = 10000; // Adjust based on game balance
        if (damage > maxDamagePerHit) {
            this.flagSuspiciousActivity(playerId, 'DAMAGE_HACK', {
                damage: damage,
                maxDamage: maxDamagePerHit,
                action: action,
                target: targetId
            });
            return false;
        }
        
        const recentActions = playerCombat.lastActions.filter(
            a => timestamp - a.timestamp < 5000
        );
        
        const totalRecentDamage = recentActions.reduce((sum, a) => sum + a.damage, 0);
        const maxDamagePerWindow = 50000; // 5 second window
        
        if (totalRecentDamage + damage > maxDamagePerWindow) {
            this.flagSuspiciousActivity(playerId, 'EXCESSIVE_DAMAGE', {
                totalDamage: totalRecentDamage + damage,
                maxDamage: maxDamagePerWindow,
                windowSize: 5000
            });
            return false;
        }
        
        playerCombat.lastActions.push({
            action: action,
            damage: damage,
            target: targetId,
            timestamp: timestamp
        });
        
        if (playerCombat.lastActions.length > 20) {
            playerCombat.lastActions.shift();
        }
        
        this.combatValidation.set(playerId, playerCombat);
        return true;
    }

    validateSkillUsage(playerId, skillId, cooldown, manaCost, timestamp) {
        const playerData = this.playerValidationData.get(playerId) || {
            skillCooldowns: new Map(),
            mana: 1000,
            lastSkillUse: 0
        };
        
        const lastUse = playerData.skillCooldowns.get(skillId) || 0;
        if (timestamp - lastUse < cooldown * 1000) {
            this.flagSuspiciousActivity(playerId, 'COOLDOWN_HACK', {
                skillId: skillId,
                cooldown: cooldown,
                timeSinceLastUse: timestamp - lastUse
            });
            return false;
        }
        
        if (playerData.mana < manaCost) {
            this.flagSuspiciousActivity(playerId, 'MANA_HACK', {
                skillId: skillId,
                manaCost: manaCost,
                currentMana: playerData.mana
            });
            return false;
        }
        
        if (!this.checkRateLimit(playerId, 'skill', timestamp)) {
            this.flagSuspiciousActivity(playerId, 'SKILL_SPAM', {
                skillId: skillId,
                timestamp: timestamp
            });
            return false;
        }
        
        playerData.skillCooldowns.set(skillId, timestamp);
        playerData.mana -= manaCost;
        playerData.lastSkillUse = timestamp;
        
        this.playerValidationData.set(playerId, playerData);
        return true;
    }

    validateEconomyAction(playerId, action, amount, itemId, timestamp) {
        const maxGoldTransaction = 1000000;
        if (amount > maxGoldTransaction) {
            this.flagSuspiciousActivity(playerId, 'GOLD_HACK', {
                action: action,
                amount: amount,
                maxAmount: maxGoldTransaction
            });
            return false;
        }
        
        if (action === 'trade' && !this.checkRateLimit(playerId, 'trade', timestamp)) {
            this.flagSuspiciousActivity(playerId, 'TRADE_SPAM', {
                action: action,
                amount: amount,
                timestamp: timestamp
            });
            return false;
        }
        
        return true;
    }

    checkRateLimit(playerId, actionType, timestamp) {
        const limit = this.rateLimits[actionType];
        if (!limit) return true;
        
        const playerActions = this.actionTimestamps.get(playerId) || new Map();
        const actionHistory = playerActions.get(actionType) || [];
        
        const validActions = actionHistory.filter(
            time => timestamp - time < limit.windowMs
        );
        
        if (validActions.length >= limit.maxActions) {
            return false;
        }
        
        validActions.push(timestamp);
        playerActions.set(actionType, validActions);
        this.actionTimestamps.set(playerId, playerActions);
        
        return true;
    }

    flagSuspiciousActivity(playerId, type, details) {
        const playerSuspicion = this.suspiciousActivities.get(playerId) || {
            flags: [],
            score: 0,
            firstFlag: Date.now()
        };
        
        const flag = {
            type: type,
            details: details,
            timestamp: Date.now(),
            severity: this.getSeverityScore(type)
        };
        
        playerSuspicion.flags.push(flag);
        playerSuspicion.score += flag.severity;
        
        this.suspiciousActivities.set(playerId, playerSuspicion);
        
        console.log(`🚨 Suspicious activity detected: ${playerId} - ${type}`, details);
        
        if (playerSuspicion.score >= 100) {
            this.initiatePlayerBan(playerId, 'Automated anti-cheat detection');
        } else if (playerSuspicion.score >= 50) {
            this.initiatePlayerWarning(playerId, 'Suspicious activity detected');
        }
    }

    getSeverityScore(type) {
        const severityMap = {
            'SPEED_HACK': 25,
            'TELEPORT_HACK': 30,
            'DAMAGE_HACK': 40,
            'COOLDOWN_HACK': 20,
            'MANA_HACK': 20,
            'GOLD_HACK': 35,
            'COMBAT_SPAM': 10,
            'SKILL_SPAM': 10,
            'TRADE_SPAM': 15,
            'EXCESSIVE_DAMAGE': 30
        };
        
        return severityMap[type] || 10;
    }

    initiatePlayerBan(playerId, reason) {
        console.log(`🔨 Auto-banning player ${playerId}: ${reason}`);
        
    }

    initiatePlayerWarning(playerId, reason) {
        console.log(`⚠️ Warning player ${playerId}: ${reason}`);
        
    }

    calculateDistance(pos1, pos2) {
        const dx = pos1.x - pos2.x;
        const dy = pos1.y - pos2.y;
        const dz = pos1.z - pos2.z;
        return Math.sqrt(dx * dx + dy * dy + dz * dz);
    }

    generateSecureToken(playerId, timestamp) {
        const data = `${playerId}:${timestamp}:${process.env.ANTI_CHEAT_SECRET}`;
        return crypto.createHash('sha256').update(data).digest('hex');
    }

    validateClientToken(playerId, timestamp, token) {
        const expectedToken = this.generateSecureToken(playerId, timestamp);
        return token === expectedToken && Date.now() - timestamp < 30000; // 30 second window
    }

    cleanupOldData() {
        const now = Date.now();
        const maxAge = 10 * 60 * 1000; // 10 minutes
        
        for (const [playerId, data] of this.suspiciousActivities.entries()) {
            if (now - data.firstFlag > maxAge) {
                this.suspiciousActivities.delete(playerId);
            }
        }
        
        for (const [playerId, history] of this.movementHistory.entries()) {
            const validHistory = history.filter(h => now - h.timestamp < maxAge);
            if (validHistory.length === 0) {
                this.movementHistory.delete(playerId);
            } else {
                this.movementHistory.set(playerId, validHistory);
            }
        }
        
        console.log('🧹 Anti-cheat data cleanup completed');
    }

    getPlayerReport(playerId) {
        return this.suspiciousActivities.get(playerId) || null;
    }

    getFlaggedPlayers() {
        const flagged = [];
        for (const [playerId, data] of this.suspiciousActivities.entries()) {
            if (data.score > 20) {
                flagged.push({
                    playerId: playerId,
                    score: data.score,
                    flagCount: data.flags.length,
                    firstFlag: data.firstFlag,
                    lastFlag: data.flags[data.flags.length - 1].timestamp
                });
            }
        }
        return flagged.sort((a, b) => b.score - a.score);
    }
}

module.exports = AntiCheatSystem;
