const crypto = require('crypto');
const jwt = require('jsonwebtoken');
const bcrypt = require('bcrypt');
const rateLimit = require('express-rate-limit');

class SecurityManager {
    constructor() {
        this.jwtSecret = process.env.JWT_SECRET || this.generateSecureSecret();
        this.encryptionKey = process.env.ENCRYPTION_KEY || this.generateEncryptionKey();
        this.saltRounds = 12;
        
        this.rateLimiters = new Map();
        this.securityEvents = new Map();
        this.bannedIPs = new Set();
        this.suspiciousIPs = new Map();
        
        this.initializeSecurity();
    }

    initializeSecurity() {
        console.log('🔐 Security manager initialized');
        
        this.setupRateLimiters();
        
        this.startSecurityMonitoring();
        
        setInterval(() => {
            this.cleanupSecurityEvents();
        }, 60 * 60 * 1000); // Every hour
    }

    setupRateLimiters() {
        this.rateLimiters.set('auth', rateLimit({
            windowMs: 15 * 60 * 1000, // 15 minutes
            max: 5, // 5 attempts per window
            message: 'Too many authentication attempts',
            standardHeaders: true,
            legacyHeaders: false,
            handler: (req, res) => {
                this.logSecurityEvent('RATE_LIMIT_AUTH', req.ip, {
                    endpoint: req.path,
                    userAgent: req.get('User-Agent')
                });
                res.status(429).json({ error: 'Too many authentication attempts' });
            }
        }));

        this.rateLimiters.set('api', rateLimit({
            windowMs: 1 * 60 * 1000, // 1 minute
            max: 100, // 100 requests per minute
            message: 'Too many API requests',
            standardHeaders: true,
            legacyHeaders: false,
            handler: (req, res) => {
                this.logSecurityEvent('RATE_LIMIT_API', req.ip, {
                    endpoint: req.path,
                    method: req.method
                });
                res.status(429).json({ error: 'Too many API requests' });
            }
        }));

        this.rateLimiters.set('chat', rateLimit({
            windowMs: 1000, // 1 second
            max: 5, // 5 messages per second
            message: 'Chat rate limit exceeded',
            keyGenerator: (req) => req.user?.id || req.ip,
            handler: (req, res) => {
                this.logSecurityEvent('RATE_LIMIT_CHAT', req.ip, {
                    userId: req.user?.id,
                    message: req.body?.message?.substring(0, 50)
                });
                res.status(429).json({ error: 'Chat rate limit exceeded' });
            }
        }));
    }

    async hashPassword(password) {
        try {
            const salt = await bcrypt.genSalt(this.saltRounds);
            const hash = await bcrypt.hash(password, salt);
            return hash;
        } catch (error) {
            console.error('❌ Error hashing password:', error);
            throw new Error('Password hashing failed');
        }
    }

    async verifyPassword(password, hash) {
        try {
            return await bcrypt.compare(password, hash);
        } catch (error) {
            console.error('❌ Error verifying password:', error);
            return false;
        }
    }

    generateJWT(payload, expiresIn = '24h') {
        try {
            return jwt.sign(payload, this.jwtSecret, {
                expiresIn: expiresIn,
                issuer: 'legends-of-tianming',
                audience: 'game-client'
            });
        } catch (error) {
            console.error('❌ Error generating JWT:', error);
            throw new Error('Token generation failed');
        }
    }

    verifyJWT(token) {
        try {
            return jwt.verify(token, this.jwtSecret, {
                issuer: 'legends-of-tianming',
                audience: 'game-client'
            });
        } catch (error) {
            if (error.name === 'TokenExpiredError') {
                throw new Error('Token expired');
            } else if (error.name === 'JsonWebTokenError') {
                throw new Error('Invalid token');
            } else {
                console.error('❌ Error verifying JWT:', error);
                throw new Error('Token verification failed');
            }
        }
    }

    encrypt(text) {
        try {
            const iv = crypto.randomBytes(16);
            const cipher = crypto.createCipher('aes-256-cbc', this.encryptionKey);
            cipher.setAutoPadding(true);
            
            let encrypted = cipher.update(text, 'utf8', 'hex');
            encrypted += cipher.final('hex');
            
            return iv.toString('hex') + ':' + encrypted;
        } catch (error) {
            console.error('❌ Error encrypting data:', error);
            throw new Error('Encryption failed');
        }
    }

    decrypt(encryptedText) {
        try {
            const parts = encryptedText.split(':');
            const iv = Buffer.from(parts[0], 'hex');
            const encrypted = parts[1];
            
            const decipher = crypto.createDecipher('aes-256-cbc', this.encryptionKey);
            decipher.setAutoPadding(true);
            
            let decrypted = decipher.update(encrypted, 'hex', 'utf8');
            decrypted += decipher.final('utf8');
            
            return decrypted;
        } catch (error) {
            console.error('❌ Error decrypting data:', error);
            throw new Error('Decryption failed');
        }
    }

    validateInput(input, type, options = {}) {
        if (input === null || input === undefined) {
            return { valid: false, error: 'Input is required' };
        }

        switch (type) {
            case 'username':
                return this.validateUsername(input, options);
            case 'email':
                return this.validateEmail(input);
            case 'password':
                return this.validatePassword(input, options);
            case 'characterName':
                return this.validateCharacterName(input);
            case 'guildName':
                return this.validateGuildName(input);
            case 'chatMessage':
                return this.validateChatMessage(input);
            case 'coordinates':
                return this.validateCoordinates(input);
            default:
                return { valid: false, error: 'Unknown validation type' };
        }
    }

    validateUsername(username, options = {}) {
        const minLength = options.minLength || 3;
        const maxLength = options.maxLength || 20;
        const pattern = /^[a-zA-Z0-9_-]+$/;

        if (typeof username !== 'string') {
            return { valid: false, error: 'Username must be a string' };
        }

        if (username.length < minLength || username.length > maxLength) {
            return { valid: false, error: `Username must be ${minLength}-${maxLength} characters` };
        }

        if (!pattern.test(username)) {
            return { valid: false, error: 'Username contains invalid characters' };
        }

        if (this.containsProfanity(username)) {
            return { valid: false, error: 'Username contains inappropriate content' };
        }

        return { valid: true };
    }

    validateEmail(email) {
        const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (typeof email !== 'string') {
            return { valid: false, error: 'Email must be a string' };
        }

        if (!pattern.test(email)) {
            return { valid: false, error: 'Invalid email format' };
        }

        if (email.length > 254) {
            return { valid: false, error: 'Email too long' };
        }

        return { valid: true };
    }

    validatePassword(password, options = {}) {
        const minLength = options.minLength || 8;
        const requireUppercase = options.requireUppercase !== false;
        const requireLowercase = options.requireLowercase !== false;
        const requireNumbers = options.requireNumbers !== false;
        const requireSpecialChars = options.requireSpecialChars !== false;

        if (typeof password !== 'string') {
            return { valid: false, error: 'Password must be a string' };
        }

        if (password.length < minLength) {
            return { valid: false, error: `Password must be at least ${minLength} characters` };
        }

        if (requireUppercase && !/[A-Z]/.test(password)) {
            return { valid: false, error: 'Password must contain uppercase letters' };
        }

        if (requireLowercase && !/[a-z]/.test(password)) {
            return { valid: false, error: 'Password must contain lowercase letters' };
        }

        if (requireNumbers && !/\d/.test(password)) {
            return { valid: false, error: 'Password must contain numbers' };
        }

        if (requireSpecialChars && !/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
            return { valid: false, error: 'Password must contain special characters' };
        }

        return { valid: true };
    }

    validateCharacterName(name) {
        const minLength = 2;
        const maxLength = 16;
        const pattern = /^[a-zA-Z0-9\s]+$/;

        if (typeof name !== 'string') {
            return { valid: false, error: 'Character name must be a string' };
        }

        if (name.length < minLength || name.length > maxLength) {
            return { valid: false, error: `Character name must be ${minLength}-${maxLength} characters` };
        }

        if (!pattern.test(name)) {
            return { valid: false, error: 'Character name contains invalid characters' };
        }

        if (this.containsProfanity(name)) {
            return { valid: false, error: 'Character name contains inappropriate content' };
        }

        return { valid: true };
    }

    validateGuildName(name) {
        const minLength = 3;
        const maxLength = 25;
        const pattern = /^[a-zA-Z0-9\s]+$/;

        if (typeof name !== 'string') {
            return { valid: false, error: 'Guild name must be a string' };
        }

        if (name.length < minLength || name.length > maxLength) {
            return { valid: false, error: `Guild name must be ${minLength}-${maxLength} characters` };
        }

        if (!pattern.test(name)) {
            return { valid: false, error: 'Guild name contains invalid characters' };
        }

        if (this.containsProfanity(name)) {
            return { valid: false, error: 'Guild name contains inappropriate content' };
        }

        return { valid: true };
    }

    validateChatMessage(message) {
        const maxLength = 500;

        if (typeof message !== 'string') {
            return { valid: false, error: 'Message must be a string' };
        }

        if (message.length === 0) {
            return { valid: false, error: 'Message cannot be empty' };
        }

        if (message.length > maxLength) {
            return { valid: false, error: `Message too long (max ${maxLength} characters)` };
        }

        if (this.isSpamMessage(message)) {
            return { valid: false, error: 'Message appears to be spam' };
        }

        return { valid: true };
    }

    validateCoordinates(coords) {
        if (typeof coords !== 'object' || coords === null) {
            return { valid: false, error: 'Coordinates must be an object' };
        }

        const { x, y, z } = coords;

        if (typeof x !== 'number' || typeof y !== 'number' || typeof z !== 'number') {
            return { valid: false, error: 'Coordinates must be numbers' };
        }

        if (!isFinite(x) || !isFinite(y) || !isFinite(z)) {
            return { valid: false, error: 'Coordinates must be finite numbers' };
        }

        const bounds = {
            minX: -1000, maxX: 1000,
            minY: -1000, maxY: 1000,
            minZ: -100, maxZ: 100
        };

        if (x < bounds.minX || x > bounds.maxX ||
            y < bounds.minY || y > bounds.maxY ||
            z < bounds.minZ || z > bounds.maxZ) {
            return { valid: false, error: 'Coordinates out of bounds' };
        }

        return { valid: true };
    }

    containsProfanity(text) {
        const profanityList = [
            'badword1', 'badword2', 'badword3'
        ];

        const lowerText = text.toLowerCase();
        return profanityList.some(word => lowerText.includes(word));
    }

    isSpamMessage(message) {
        if (/(.)\1{10,}/.test(message)) {
            return true;
        }

        const words = message.split(/\s+/);
        const wordCount = {};
        for (const word of words) {
            wordCount[word] = (wordCount[word] || 0) + 1;
            if (wordCount[word] > 5) {
                return true;
            }
        }

        const capsRatio = (message.match(/[A-Z]/g) || []).length / message.length;
        if (capsRatio > 0.7 && message.length > 10) {
            return true;
        }

        return false;
    }

    sanitizeInput(input) {
        if (typeof input !== 'string') {
            return input;
        }

        return input
            .replace(/[<>]/g, '') // Remove angle brackets
            .replace(/javascript:/gi, '') // Remove javascript: protocol
            .replace(/on\w+=/gi, '') // Remove event handlers
            .trim();
    }

    startSecurityMonitoring() {
        console.log('👁️ Security monitoring started');
        
        setInterval(() => {
            this.analyzeSuspiciousActivity();
        }, 5 * 60 * 1000); // Every 5 minutes
    }

    logSecurityEvent(type, ip, details = {}) {
        const event = {
            type: type,
            ip: ip,
            timestamp: Date.now(),
            details: details
        };

        if (!this.securityEvents.has(ip)) {
            this.securityEvents.set(ip, []);
        }
        this.securityEvents.get(ip).push(event);

        this.updateSuspiciousIP(ip, type);

        console.log(`🚨 Security event: ${type} from ${ip}`, details);
    }

    updateSuspiciousIP(ip, eventType) {
        const suspiciousData = this.suspiciousIPs.get(ip) || {
            score: 0,
            events: [],
            firstSeen: Date.now()
        };

        const scores = {
            'RATE_LIMIT_AUTH': 10,
            'RATE_LIMIT_API': 5,
            'RATE_LIMIT_CHAT': 3,
            'INVALID_TOKEN': 8,
            'FAILED_LOGIN': 5,
            'SUSPICIOUS_ACTIVITY': 15
        };

        suspiciousData.score += scores[eventType] || 1;
        suspiciousData.events.push({
            type: eventType,
            timestamp: Date.now()
        });

        this.suspiciousIPs.set(ip, suspiciousData);

        if (suspiciousData.score >= 100) {
            this.banIP(ip, 'Automated security system');
        }
    }

    analyzeSuspiciousActivity() {
        for (const [ip, data] of this.suspiciousIPs.entries()) {
            const recentEvents = data.events.filter(
                event => Date.now() - event.timestamp < 60 * 60 * 1000 // Last hour
            );

            if (recentEvents.length > 50) {
                this.logSecurityEvent('SUSPICIOUS_ACTIVITY', ip, {
                    reason: 'High event frequency',
                    eventCount: recentEvents.length
                });
            }
        }
    }

    banIP(ip, reason) {
        this.bannedIPs.add(ip);
        console.log(`🔨 IP banned: ${ip} - ${reason}`);
        
    }

    isIPBanned(ip) {
        return this.bannedIPs.has(ip);
    }

    unbanIP(ip) {
        this.bannedIPs.delete(ip);
        this.suspiciousIPs.delete(ip);
        console.log(`✅ IP unbanned: ${ip}`);
    }

    generateSecureSecret() {
        return crypto.randomBytes(64).toString('hex');
    }

    generateEncryptionKey() {
        return crypto.randomBytes(32).toString('hex');
    }

    generateSecureToken(length = 32) {
        return crypto.randomBytes(length).toString('hex');
    }

    cleanupSecurityEvents() {
        const maxAge = 24 * 60 * 60 * 1000; // 24 hours
        const now = Date.now();

        for (const [ip, events] of this.securityEvents.entries()) {
            const recentEvents = events.filter(event => now - event.timestamp < maxAge);
            
            if (recentEvents.length === 0) {
                this.securityEvents.delete(ip);
            } else {
                this.securityEvents.set(ip, recentEvents);
            }
        }

        for (const [ip, data] of this.suspiciousIPs.entries()) {
            if (now - data.firstSeen > maxAge && data.score < 50) {
                this.suspiciousIPs.delete(ip);
            }
        }

        console.log('🧹 Security events cleanup completed');
    }

    getSecurityStats() {
        return {
            bannedIPs: this.bannedIPs.size,
            suspiciousIPs: this.suspiciousIPs.size,
            totalSecurityEvents: Array.from(this.securityEvents.values())
                .reduce((total, events) => total + events.length, 0),
            rateLimiters: this.rateLimiters.size
        };
    }

    getRateLimiter(type) {
        return this.rateLimiters.get(type);
    }
}

module.exports = SecurityManager;
