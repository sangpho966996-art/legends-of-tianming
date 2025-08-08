const express = require('express');
const bcrypt = require('bcryptjs');
const jwt = require('jsonwebtoken');
const db = require('../config/database');
const router = express.Router();

router.post('/register', async (req, res) => {
    try {
        const { username, email, password } = req.body;
        
        if (!username || !email || !password) {
            return res.status(400).json({ error: 'Missing required fields' });
        }

        const existingUser = await db.query(
            'SELECT id FROM players WHERE username = $1 OR email = $2',
            [username, email]
        );

        if (existingUser.rows.length > 0) {
            return res.status(409).json({ error: 'Username or email already exists' });
        }

        const hashedPassword = await bcrypt.hash(password, 12);
        
        const result = await db.query(
            'INSERT INTO players (username, email, password_hash, created_at) VALUES ($1, $2, $3, NOW()) RETURNING id, username, email, created_at',
            [username, email, hashedPassword]
        );

        const user = result.rows[0];
        const token = jwt.sign(
            { userId: user.id, username: user.username },
            process.env.JWT_SECRET,
            { expiresIn: process.env.JWT_EXPIRES_IN || '24h' }
        );

        res.status(201).json({
            message: 'User registered successfully',
            user: {
                id: user.id,
                username: user.username,
                email: user.email,
                createdAt: user.created_at
            },
            token
        });
    } catch (error) {
        console.error('Registration error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.post('/login', async (req, res) => {
    try {
        const { username, password } = req.body;
        
        if (!username || !password) {
            return res.status(400).json({ error: 'Username and password required' });
        }

        const result = await db.query(
            'SELECT id, username, email, password_hash, created_at FROM players WHERE username = $1',
            [username]
        );

        if (result.rows.length === 0) {
            return res.status(401).json({ error: 'Invalid credentials' });
        }

        const user = result.rows[0];
        const isValidPassword = await bcrypt.compare(password, user.password_hash);

        if (!isValidPassword) {
            return res.status(401).json({ error: 'Invalid credentials' });
        }

        const token = jwt.sign(
            { userId: user.id, username: user.username },
            process.env.JWT_SECRET,
            { expiresIn: process.env.JWT_EXPIRES_IN || '24h' }
        );

        res.json({
            message: 'Login successful',
            user: {
                id: user.id,
                username: user.username,
                email: user.email,
                createdAt: user.created_at
            },
            token
        });
    } catch (error) {
        console.error('Login error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

router.post('/verify', async (req, res) => {
    try {
        const token = req.headers.authorization?.split(' ')[1];
        
        if (!token) {
            return res.status(401).json({ error: 'No token provided' });
        }

        const decoded = jwt.verify(token, process.env.JWT_SECRET);
        
        const result = await db.query(
            'SELECT id, username, email, created_at FROM players WHERE id = $1',
            [decoded.userId]
        );

        if (result.rows.length === 0) {
            return res.status(401).json({ error: 'Invalid token' });
        }

        res.json({
            valid: true,
            user: result.rows[0]
        });
    } catch (error) {
        console.error('Token verification error:', error);
        res.status(401).json({ error: 'Invalid token' });
    }
});

module.exports = router;
